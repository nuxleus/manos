using System;
using System.Net.Sockets;

namespace Manos.IO.Managed
{
    internal abstract class IPSocket<TFragment, TStream> : Socket<TFragment, TStream, IPEndPoint>
        where TFragment : class
        where TStream : IStream<TFragment>
    {
        protected bool disposed;
        private IPEndPoint localname;
        private IPEndPoint peername;
        protected Socket socket;

        protected IPSocket(Context context, AddressFamily addressFamily, ProtocolFamily protocolFamily)
            : base(context, addressFamily)
        {
            System.Net.Sockets.AddressFamily family = addressFamily == AddressFamily.InterNetwork
                                                          ? System.Net.Sockets.AddressFamily.InterNetwork
                                                          : System.Net.Sockets.AddressFamily.InterNetworkV6;

            SocketType type = protocolFamily == ProtocolFamily.Tcp
                                  ? SocketType.Stream
                                  : SocketType.Dgram;

            ProtocolType protocol = protocolFamily == ProtocolFamily.Tcp
                                        ? ProtocolType.Tcp
                                        : ProtocolType.Udp;

            socket = new Socket(family, type, protocol);
        }

        protected IPSocket(Context context, AddressFamily addressFamily, Socket socket)
            : base(context, addressFamily)
        {
            this.socket = socket;
        }

        public override IPEndPoint LocalEndpoint
        {
            get
            {
                if (localname == null)
                {
                    var ep = (System.Net.IPEndPoint) socket.LocalEndPoint;
                    localname = new IPEndPoint(new IPAddress(ep.Address), ep.Port);
                }
                return localname;
            }
        }

        public override IPEndPoint RemoteEndpoint
        {
            get
            {
                if (peername == null)
                {
                    var ep = (System.Net.IPEndPoint) socket.RemoteEndPoint;
                    peername = new IPEndPoint(new IPAddress(ep.Address), ep.Port);
                }
                return peername;
            }
        }

        public new Context Context
        {
            get { return (Context) base.Context; }
        }

        public override void Bind(IPEndPoint endpoint)
        {
            socket.Bind(new System.Net.IPEndPoint(endpoint.Address.address, endpoint.Port));
        }

        protected virtual void CheckDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        protected override void Dispose(bool disposing)
        {
            socket.Dispose();
            disposed = true;
            base.Dispose(disposing);
        }
    }
}