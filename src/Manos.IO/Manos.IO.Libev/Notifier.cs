using System;
using System.Runtime.InteropServices;
using Libev;

namespace Manos.IO.Libev
{
    internal class Notifier : INotifier, IBaseWatcher, IDisposable
    {
        private readonly IOWatcher iowatcher;
        private readonly Pipe pipe;
        private IntPtr data;

        public Notifier(Context context, Action callback)
        {
            data = Marshal.AllocHGlobal(1);

            pipe = new Pipe();
            iowatcher = new IOWatcher(pipe.Out, EventTypes.Read, context.Loop, (iow, ev) =>
                                                                                   {
                                                                                       pipe.Read(data, 1);
                                                                                       if (callback != null)
                                                                                       {
                                                                                           callback();
                                                                                       }
                                                                                   });
        }

        #region INotifier Members

        public void Notify()
        {
            pipe.Write(data, 1);
        }

        public void Start()
        {
            iowatcher.Start();
        }

        public void Stop()
        {
            iowatcher.Stop();
        }

        public bool IsRunning
        {
            get { return iowatcher.IsRunning; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        ~Notifier()
        {
            Dispose(false);
        }

        protected void Dispose(bool disposing)
        {
            if (data != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(data);
                data = IntPtr.Zero;
            }
        }
    }
}