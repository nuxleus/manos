using System;

namespace Manos.IO.Managed
{
    internal class AsyncWatcher : Watcher, IAsyncWatcher
    {
        private readonly Action callback;
        private bool pending;

        public AsyncWatcher(Context context, Action callback)
            : base(context)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            this.callback = callback;
        }

        #region IAsyncWatcher Members

        public void Send()
        {
            if (!pending && IsRunning)
            {
                Context.Enqueue(delegate
                                    {
                                        pending = false;
                                        callback();
                                    });
                pending = true;
            }
        }

        public override void Start()
        {
            base.Start();
            pending = false;
        }

        public override void Stop()
        {
            base.Stop();
            pending = false;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            Context.Remove(this);
        }
    }
}