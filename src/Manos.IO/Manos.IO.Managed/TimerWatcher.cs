using System;
using System.Threading;

namespace Manos.IO.Managed
{
    internal class TimerWatcher : Watcher, ITimerWatcher
    {
        private readonly Action cb;
        private TimeSpan after;
        private int invocationConcurrency;
        private Timer timer;

        public TimerWatcher(Context context, Action callback, TimeSpan after, TimeSpan repeat)
            : base(context)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            cb = callback;
            timer = new Timer(Invoke);
            this.after = after;
            Repeat = repeat;
        }

        #region ITimerWatcher Members

        public override void Start()
        {
            base.Start();
            timer.Change((int) after.TotalMilliseconds,
                         Repeat == TimeSpan.Zero ? Timeout.Infinite : (int) Repeat.TotalMilliseconds);
        }

        public override void Stop()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            base.Stop();
        }

        public void Again()
        {
            after = TimeSpan.Zero;
            Start();
        }

        public TimeSpan Repeat { get; set; }

        #endregion

        private void Invoke(object state)
        {
            try
            {
                if (Interlocked.Increment(ref invocationConcurrency) == 1)
                {
                    if (IsRunning)
                    {
                        Context.Enqueue(cb);
                        after = TimeSpan.Zero;
                    }
                }
            }
            finally
            {
                Interlocked.Decrement(ref invocationConcurrency);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (timer != null)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                timer.Dispose();
                timer = null;
            }
            Context.Remove(this);
        }
    }
}