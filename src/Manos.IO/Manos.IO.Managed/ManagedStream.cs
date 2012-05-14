using System;
using System.Collections.Generic;
using System.Threading;

namespace Manos.IO.Managed
{
    internal abstract class ManagedStream<TFragment> : FragmentStream<TFragment>
        where TFragment : class
    {
        protected byte[] buffer;
        protected bool readAllowed;
        private int readTimeoutInterval = -1;
        private Timer readTimer;
        protected bool writeAllowed;
        private int writeTimeoutInterval = -1;
        private Timer writeTimer;

        protected ManagedStream(Context ctx, int bufferSize)
            : base(ctx)
        {
            buffer = new byte[bufferSize];
        }

        public new Context Context
        {
            get { return (Context) base.Context; }
        }

        public override bool CanTimeout
        {
            get { return true; }
        }

        public override TimeSpan ReadTimeout
        {
            get { return readTimer == null ? TimeSpan.Zero : TimeSpan.FromMilliseconds(readTimeoutInterval); }
            set
            {
                if (value < TimeSpan.Zero)
                    throw new ArgumentException("value");

                readTimeoutInterval = value == TimeSpan.Zero ? -1 : (int) value.TotalMilliseconds;

                if (readTimer == null)
                {
                    readTimer = new Timer(HandleReadTimerElapsed);
                }
                readTimer.Change(readTimeoutInterval, readTimeoutInterval);
            }
        }

        public override TimeSpan WriteTimeout
        {
            get { return writeTimer == null ? TimeSpan.Zero : TimeSpan.FromMilliseconds(writeTimeoutInterval); }
            set
            {
                if (value < TimeSpan.Zero)
                    throw new ArgumentException("value");

                writeTimeoutInterval = value == TimeSpan.Zero ? -1 : (int) value.TotalMilliseconds;

                if (writeTimer == null)
                {
                    writeTimer = new Timer(HandleWriteTimerElapsed);
                }
                writeTimer.Change(writeTimeoutInterval, writeTimeoutInterval);
            }
        }

        protected void ResetReadTimeout()
        {
            if (readTimer != null)
            {
                readTimer.Change(readTimeoutInterval, readTimeoutInterval);
            }
        }

        protected void ResetWriteTimeout()
        {
            if (writeTimer != null)
            {
                writeTimer.Change(writeTimeoutInterval, writeTimeoutInterval);
            }
        }

        private void HandleReadTimerElapsed(object state)
        {
            if (readAllowed)
            {
                RaiseError(new TimeoutException());
                PauseReading();
            }
        }

        private void HandleWriteTimerElapsed(object state)
        {
            if (writeAllowed)
            {
                RaiseError(new TimeoutException());
                PauseWriting();
            }
        }

        public override IDisposable Read(Action<TFragment> onData, Action<Exception> onError, Action onClose)
        {
            IDisposable result = base.Read(onData, onError, onClose);
            ResumeReading();
            return result;
        }

        public override void Write(IEnumerable<TFragment> data)
        {
            base.Write(data);
            ResumeWriting();
        }

        public override void ResumeReading()
        {
            CheckDisposed();

            if (!readAllowed)
            {
                readAllowed = true;
                DispatchRead();
            }
        }

        public override void ResumeWriting()
        {
            CheckDisposed();

            if (!writeAllowed)
            {
                writeAllowed = true;
                HandleWrite();
            }
        }

        public override void PauseReading()
        {
            CheckDisposed();

            readAllowed = false;
        }

        public override void PauseWriting()
        {
            CheckDisposed();

            writeAllowed = false;
        }

        public override void Flush()
        {
        }

        protected virtual void DispatchRead()
        {
            if (readAllowed)
            {
                DoRead();
            }
        }

        protected override void HandleWrite()
        {
            if (writeAllowed)
            {
                base.HandleWrite();
            }
        }

        protected abstract void DoRead();

        protected override void Dispose(bool disposing)
        {
            buffer = null;
            if (readTimer != null)
            {
                readTimer.Dispose();
            }
            if (writeTimer != null)
            {
                writeTimer.Dispose();
            }
            readTimer = null;
            writeTimer = null;
            base.Dispose(disposing);
        }
    }
}