using System;
using System.Reflection;
using Manos.Http;
using Manos.IO;

namespace Manos.Spdy
{
    public delegate void SpdyConnectionCallback(IHttpTransaction transaction);

    public class SpdyServer : IDisposable
    {
        public static readonly string ServerVersion;
        private readonly SpdyConnectionCallback callback;
        private bool closeOnEnd;
        private ITcpServerSocket socket;

        static SpdyServer()
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            ServerVersion = "Manos/SPDY/" + v;
        }

        public SpdyServer(Context context, SpdyConnectionCallback callback, ITcpServerSocket socket,
                          bool closeOnEnd = false)
        {
            this.callback = callback;
            this.socket = socket;
            this.closeOnEnd = closeOnEnd;
            Context = context;
        }

        public Context Context { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            if (socket != null)
            {
                socket.Dispose();
                socket = null;
            }
        }

        #endregion

        public void Listen(string host, int port)
        {
            socket.Listen(port, ConnectionAccepted);
        }

        private void ConnectionAccepted(ITcpSocket socket)
        {
            var t = new SpdySession(Context, socket, callback);
        }
    }
}