using System;

namespace Manos.IO.Managed
{
    internal class Notifier : INotifier
    {
        private readonly Action callback;
        private readonly Context context;
        private readonly object syncRoot = new object();
        private int count;

        public Notifier(Context context, Action callback)
        {
            this.callback = callback;
            this.context = context;
        }

        #region INotifier Members

        public void Notify()
        {
            lock (syncRoot)
            {
                if (IsRunning)
                {
                    context.Enqueue(callback);
                }
                else
                {
                    count++;
                }
            }
        }

        public void Start()
        {
            lock (syncRoot)
            {
                if (!IsRunning)
                {
                    while (count > 0)
                    {
                        context.Enqueue(callback);
                        count--;
                    }
                    IsRunning = true;
                }
            }
        }

        public void Stop()
        {
            lock (syncRoot)
            {
                if (IsRunning)
                {
                    IsRunning = false;
                }
            }
        }

        public bool IsRunning { get; protected set; }

        public void Dispose()
        {
        }

        #endregion
    }
}