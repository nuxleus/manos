using System;
using System.Runtime.InteropServices;
using Manos.IO;

namespace Libev
{
    internal abstract class Watcher : IBaseWatcher
    {
        private bool disposed;
        protected GCHandle gc_handle;
        protected IntPtr watcher_ptr;

        internal Watcher(Loop loop)
        {
            Loop = loop;
            gc_handle = GCHandle.Alloc(this);
        }

        public Loop Loop { get; private set; }

        #region IBaseWatcher Members

        public bool IsRunning { get; private set; }

        public virtual void Dispose()
        {
            if (disposed)
            {
                return;
            }
            Stop();
            DestroyWatcher();
            watcher_ptr = IntPtr.Zero;
            gc_handle.Free();
            GC.SuppressFinalize(this);
            disposed = true;
        }

        public virtual void Start()
        {
            if (IsRunning)
                return;
            IsRunning = true;
            StartImpl();
        }

        public virtual void Stop()
        {
            if (!IsRunning)
                return;
            IsRunning = false;
            StopImpl();
        }

        #endregion

        ~Watcher()
        {
            Dispose();
        }

        protected abstract void StartImpl();

        protected abstract void StopImpl();

        protected abstract void DestroyWatcher();
    }
}