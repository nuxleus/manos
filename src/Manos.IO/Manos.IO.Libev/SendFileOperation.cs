using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Unix.Native;

namespace Manos.IO.Libev
{
    internal class SendFileOperation : IDisposable, IEnumerable<ByteBuffer>
    {
        private readonly Context context;
        private readonly string file;
        private readonly EventedByteStream target;
        private bool completed;
        private long length;
        private long position;
        private int sourceFd;

        public SendFileOperation(Context context, EventedByteStream target, string file)
        {
            this.context = context;
            this.target = target;
            this.file = file;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (sourceFd > 0)
            {
                CloseFile();
            }
            GC.SuppressFinalize(this);
        }

        #endregion

        #region IEnumerable<ByteBuffer> Members

        public IEnumerator<ByteBuffer> GetEnumerator()
        {
            return Run().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        ~SendFileOperation()
        {
            if (sourceFd > 0)
            {
                CloseFile();
            }
        }

        private void OpenFile()
        {
            sourceFd = Syscall.open(file, OpenFlags.O_RDONLY, FilePermissions.ACCESSPERMS);
            if (sourceFd == -1)
            {
                completed = true;
                Console.Error.WriteLine("Error sending file '{0}' error: '{1}'", file, Stdlib.GetLastError());
            }
            else
            {
                Stat stat;
                int r = Syscall.fstat(sourceFd, out stat);
                if (r == -1)
                {
                    completed = true;
                }
                else
                {
                    length = stat.st_size;
                    target.ResumeWriting();
                }
            }
        }

        private void CloseFile()
        {
            Syscall.close(sourceFd);
            sourceFd = 0;
        }

        private void SendNextBlock()
        {
            context.Eio.SendFile(target.Handle.ToInt32(), sourceFd, position, length - position, (len, err) =>
                                                                                                     {
                                                                                                         if (len >= 0)
                                                                                                         {
                                                                                                             position +=
                                                                                                                 len;
                                                                                                         }
                                                                                                         else
                                                                                                         {
                                                                                                             completed =
                                                                                                                 true;
                                                                                                         }
                                                                                                         if (position ==
                                                                                                             length)
                                                                                                         {
                                                                                                             completed =
                                                                                                                 true;
                                                                                                         }
                                                                                                         target.
                                                                                                             ResumeWriting
                                                                                                             ();
                                                                                                     });
        }

        private IEnumerable<ByteBuffer> Run()
        {
            while (!completed)
            {
                if (sourceFd == 0)
                {
                    OpenFile();
                }
                SendNextBlock();
                yield return null;
            }
            Dispose();
        }
    }
}