using System;

namespace Manos.IO
{
    internal abstract class Socket<TFragment, TStream, TEndPoint> : IStreamSocket<TFragment, TStream, TEndPoint>
        where TFragment : class
        where TStream : IStream<TFragment>
        where TEndPoint : EndPoint
    {
        public Socket(Context context, AddressFamily addressFamily)
        {
            AddressFamily = addressFamily;
            Context = context;
        }

        #region IStreamSocket<TFragment,TStream,TEndPoint> Members

        public AddressFamily AddressFamily { get; private set; }

        public Context Context { get; private set; }

        public bool IsConnected { get; protected set; }

        public bool IsBound { get; protected set; }

        public abstract TEndPoint LocalEndpoint { get; }

        public abstract TEndPoint RemoteEndpoint { get; }

        public abstract void Bind(TEndPoint endpoint);

        public abstract void Connect(TEndPoint endpoint, Action callback, Action<Exception> error);

        public abstract TStream GetSocketStream();

        public virtual void Close()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        ~Socket()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}