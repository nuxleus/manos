using System;

namespace Manos.IO.Managed
{
    internal class PrepareWatcher : Watcher, IPrepareWatcher
    {
        private readonly Action cb;

        public PrepareWatcher(Context context, Action callback)
            : base(context)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            cb = callback;
        }

        public void Invoke()
        {
            if (IsRunning)
            {
                cb();
            }
        }

        protected override void Dispose(bool disposing)
        {
            Context.Remove(this);
        }
    }
}