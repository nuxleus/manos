using System;
using System.Collections.Generic;
using Mono.Unix.Native;

namespace Manos.IO.Libev
{
    internal class FileStream : FragmentStream<ByteBuffer>, IByteStream
    {
        private readonly bool canRead;
        private readonly bool canWrite;
        private readonly byte[] readBuffer;
        private long position;
        private bool readEnabled, writeEnabled;

        private FileStream(Context context, IntPtr handle, int blockSize, bool canRead, bool canWrite)
            : base(context)
        {
            Handle = handle;
            readBuffer = new byte[blockSize];
            this.canRead = canRead;
            this.canWrite = canWrite;
        }

        public new Context Context
        {
            get { return (Context) base.Context; }
        }

        public IntPtr Handle { get; private set; }

        #region IByteStream Members

        public override long Position
        {
            get { return position; }
            set { SeekTo(value); }
        }

        public override bool CanRead
        {
            get { return canRead; }
        }

        public override bool CanWrite
        {
            get { return canWrite; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override void SeekBy(long delta)
        {
            if (position + delta < 0)
                throw new ArgumentException("delta");

            position += delta;
        }

        public override void SeekTo(long position)
        {
            if (position < 0)
                throw new ArgumentException("position");

            this.position = position;
        }

        public override void Flush()
        {
        }

        public override void Write(IEnumerable<ByteBuffer> data)
        {
            base.Write(data);
            ResumeWriting();
        }

        public void Write(byte[] data)
        {
            CheckDisposed();

            Write(new ByteBuffer(data));
        }

        public override IDisposable Read(Action<ByteBuffer> onData, Action<Exception> onError, Action onClose)
        {
            IDisposable result = base.Read(onData, onError, onClose);
            ResumeReading();
            return result;
        }

        public override void ResumeReading()
        {
            CheckDisposed();

            if (!canRead)
                throw new InvalidOperationException();

            if (!readEnabled)
            {
                readEnabled = true;
                ReadNextBuffer();
            }
        }

        public override void ResumeWriting()
        {
            CheckDisposed();

            if (!canWrite)
                throw new InvalidOperationException();

            if (!writeEnabled)
            {
                writeEnabled = true;
                HandleWrite();
            }
        }

        public override void PauseReading()
        {
            CheckDisposed();

            readEnabled = false;
        }

        public override void PauseWriting()
        {
            CheckDisposed();

            writeEnabled = false;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (Handle != IntPtr.Zero)
            {
                Syscall.close(Handle.ToInt32());
                Handle = IntPtr.Zero;
            }
            base.Dispose(disposing);
        }

        private void ReadNextBuffer()
        {
            if (!readEnabled)
            {
                return;
            }

            Context.Eio.Read(Handle.ToInt32(), readBuffer, position, readBuffer.Length, OnReadDone);
        }

        private void OnReadDone(int result, byte[] buffer, int error)
        {
            if (result < 0)
            {
                PauseReading();
                RaiseError(new IOException(string.Format("Error reading from file: {0}", Errors.ErrorToString(error))));
            }
            else if (result > 0)
            {
                position += result;
                var newBuffer = new byte[result];
                Buffer.BlockCopy(readBuffer, 0, newBuffer, 0, result);
                RaiseData(new ByteBuffer(newBuffer));
                ReadNextBuffer();
            }
            else
            {
                PauseReading();
                RaiseEndOfStream();
            }
        }

        protected override void HandleWrite()
        {
            if (writeEnabled)
            {
                base.HandleWrite();
            }
        }

        protected override WriteResult WriteSingleFragment(ByteBuffer buffer)
        {
            byte[] bytes = buffer.Bytes;
            if (buffer.Position > 0)
            {
                bytes = new byte[buffer.Length];
                Array.Copy(buffer.Bytes, buffer.Position, bytes, 0, buffer.Length);
            }
            Context.Eio.Write(Handle.ToInt32(), bytes, position, buffer.Length, OnWriteDone);
            return WriteResult.Consume;
        }

        private void OnWriteDone(int result, int error)
        {
            if (result < 0)
            {
                RaiseError(new IOException(string.Format("Error writing to file: {0}", Errors.ErrorToString(error))));
            }
            else
            {
                position += result;
                HandleWrite();
            }
        }

        protected override long FragmentSize(ByteBuffer fragment)
        {
            return fragment.Length;
        }

        public static FileStream Open(Context context, string fileName, int blockSize,
                                      OpenFlags openFlags)
        {
            int fd = Syscall.open(fileName, openFlags,
                                  FilePermissions.S_IRUSR | FilePermissions.S_IWUSR | FilePermissions.S_IROTH);
            OpenFlags mask = OpenFlags.O_RDONLY | OpenFlags.O_RDWR | OpenFlags.O_WRONLY;
            bool canRead = (openFlags & mask) == OpenFlags.O_RDONLY
                           || (openFlags & mask) == OpenFlags.O_RDWR;
            bool canWrite = (openFlags & mask) == OpenFlags.O_WRONLY
                            || (openFlags & mask) == OpenFlags.O_RDWR;
            return new FileStream(context, new IntPtr(fd), blockSize, canRead, canWrite);
        }

        public static FileStream Create(Context context, string fileName, int blockSize)
        {
            return Open(context, fileName, blockSize,
                        OpenFlags.O_RDWR | OpenFlags.O_CREAT | OpenFlags.O_TRUNC);
        }
    }
}