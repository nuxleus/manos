namespace Manos.IO.Managed
{
    internal abstract class Watcher : IBaseWatcher
    {
        public Watcher(Context context)
        {
            Context = context;
        }

        public Context Context { get; private set; }

        #region IBaseWatcher Members

        public virtual void Start()
        {
            IsRunning = true;
        }

        public virtual void Stop()
        {
            IsRunning = false;
        }

        public bool IsRunning { get; protected set; }

        public virtual void Dispose()
        {
            Dispose(true);
        }

        #endregion

        protected abstract void Dispose(bool disposing);
    }
}