using System;
using Libev;

namespace Manos.IO.Libev
{
    internal class TcpSocket : IPSocket<ByteBuffer, IByteStream>, ITcpSocket, ITcpServerSocket
    {
        private IOWatcher listener;
        private TcpSocketStream stream;

        public TcpSocket(Context context, AddressFamily addressFamily)
            : base(context, addressFamily, ProtocolFamily.Tcp)
        {
        }

        private TcpSocket(Context context, AddressFamily addressFamily, int fd, IPEndPoint local, IPEndPoint remote)
            : base(context, addressFamily, fd)
        {
            stream = new TcpSocketStream(this, new IntPtr(fd));
            localname = local;
            peername = remote;
            IsConnected = true;
            IsBound = true;
        }

        #region ITcpServerSocket Members

        public void Listen(int backlog, Action<ITcpSocket> callback)
        {
            CheckDisposed();
            if (stream != null)
                throw new InvalidOperationException();

            if (listener != null)
            {
                listener.Stop();
                listener.Dispose();
            }

            int error;
            int result = SocketFunctions.manos_socket_listen(fd, backlog, out error);

            if (result < 0)
            {
                throw Errors.SocketFailure("Listen failure", error);
            }

            listener = new IOWatcher(new IntPtr(fd), EventTypes.Read, Context.Loop, delegate
                                                                                        {
                                                                                            ManosIPEndpoint ep;
                                                                                            int client =
                                                                                                SocketFunctions.
                                                                                                    manos_socket_accept(
                                                                                                        fd, out ep,
                                                                                                        out error);
                                                                                            if (client < 0 && error != 0)
                                                                                            {
                                                                                                throw new Exception(
                                                                                                    string.Format(
                                                                                                        "Error while accepting: {0}",
                                                                                                        Errors.
                                                                                                            ErrorToString
                                                                                                            (error)));
                                                                                            }
                                                                                            else if (client > 0)
                                                                                            {
                                                                                                var socket =
                                                                                                    new TcpSocket(
                                                                                                        Context,
                                                                                                        AddressFamily,
                                                                                                        client,
                                                                                                        LocalEndpoint,
                                                                                                        ep);
                                                                                                callback(socket);
                                                                                            }
                                                                                        });
            listener.Start();
        }

        #endregion

        #region ITcpSocket Members

        public override IByteStream GetSocketStream()
        {
            CheckDisposed();
            if (!IsConnected)
                throw new SocketException("Not conntected", SocketError.NotConnected);
            if (listener != null)
                throw new InvalidOperationException();

            if (stream == null)
            {
                stream = new TcpSocketStream(this, new IntPtr(fd));
            }
            return stream;
        }

        public override void Connect(IPEndPoint endpoint, Action callback, Action<Exception> error)
        {
            CheckDisposed();

            if (endpoint == null)
                throw new ArgumentNullException("endpoint");
            if (callback == null)
                throw new ArgumentNullException("callback");
            if (error == null)
                throw new ArgumentNullException("error");

            if (localname != null && localname.AddressFamily != endpoint.AddressFamily)
                throw new ArgumentException();
            if (IsConnected)
                throw new InvalidOperationException();

            int err;
            ManosIPEndpoint ep = endpoint;
            err = SocketFunctions.manos_socket_connect_ip(fd, ref ep, out err);
            if (err != 0)
            {
                throw Errors.SocketFailure("Connect failure", err);
            }
            else
            {
                var connectWatcher = new IOWatcher(new IntPtr(fd), EventTypes.Write, Context.Loop, (watcher, revents) =>
                                                                                                       {
                                                                                                           watcher.Stop();
                                                                                                           watcher.
                                                                                                               Dispose();

                                                                                                           int result =
                                                                                                               SocketFunctions
                                                                                                                   .
                                                                                                                   manos_socket_peername_ip
                                                                                                                   (fd,
                                                                                                                    out
                                                                                                                        ep,
                                                                                                                    out
                                                                                                                        err);
                                                                                                           if (result <
                                                                                                               0)
                                                                                                           {
                                                                                                               error(
                                                                                                                   Errors
                                                                                                                       .
                                                                                                                       SocketFailure
                                                                                                                       ("Connect failure",
                                                                                                                        err));
                                                                                                           }
                                                                                                           else
                                                                                                           {
                                                                                                               peername
                                                                                                                   =
                                                                                                                   endpoint;

                                                                                                               IsConnected
                                                                                                                   =
                                                                                                                   true;

                                                                                                               callback();
                                                                                                           }
                                                                                                       });
                connectWatcher.Start();
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (listener != null)
            {
                listener.Stop();
                listener.Dispose();
                listener = null;
            }
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }
            base.Dispose(disposing);
        }

        #region Nested type: TcpSocketStream

        private class TcpSocketStream : EventedByteStream, ISendfileCapable
        {
            private TcpSocket parent;
            private byte[] receiveBuffer = new byte[4096];

            public TcpSocketStream(TcpSocket parent, IntPtr handle)
                : base(parent.Context, handle)
            {
                this.parent = parent;
            }

            public override long Position
            {
                get { throw new NotSupportedException(); }
                set { throw new NotSupportedException(); }
            }

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            #region ISendfileCapable Members

            public void SendFile(string file)
            {
                CheckDisposed();
                if (file == null)
                    throw new ArgumentNullException("file");

                Write(new SendFileOperation(Context, this, file));
            }

            #endregion

            protected override void Dispose(bool disposing)
            {
                if (parent != null)
                {
                    RaiseEndOfStream();

                    receiveBuffer = null;
                    parent = null;
                }
                base.Dispose(disposing);
            }

            public override void Flush()
            {
            }

            protected override void HandleRead()
            {
                int err;
                int received = SocketFunctions.manos_socket_receive(Handle.ToInt32(), receiveBuffer,
                                                                    receiveBuffer.Length, out err);
                if (received < 0 && err != 0 || received == 0)
                {
                    if (received < 0)
                    {
                        RaiseError(Errors.SocketStreamFailure("Read failure", err));
                    }
                    Close();
                }
                else
                {
                    var newBuffer = new byte[received];
                    Buffer.BlockCopy(receiveBuffer, 0, newBuffer, 0, received);

                    RaiseData(new ByteBuffer(newBuffer));
                }
            }

            protected override WriteResult WriteSingleFragment(ByteBuffer buffer)
            {
                int err;
                int sent = SocketFunctions.manos_socket_send(Handle.ToInt32(), buffer.Bytes, buffer.Position,
                                                             buffer.Length, out err);
                if (sent < 0 && err != 0)
                {
                    RaiseError(Errors.SocketStreamFailure("Write failure", err));
                    return WriteResult.Error;
                }
                else
                {
                    buffer.Skip(sent);
                    return buffer.Length == 0 ? WriteResult.Consume : WriteResult.Continue;
                }
            }
        }

        #endregion
    }
}