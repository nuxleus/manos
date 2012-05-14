using System;
using System.Net.Sockets;

namespace Manos.IO.Managed
{
    internal class TcpSocket : IPSocket<ByteBuffer, IByteStream>, ITcpSocket, ITcpServerSocket
    {
        private TcpStream stream;
        private bool wasDisposing;

        public TcpSocket(Context context, AddressFamily addressFamily)
            : base(context, addressFamily, ProtocolFamily.Tcp)
        {
        }

        private TcpSocket(Context context, AddressFamily addressFamily, Socket socket)
            : base(context, addressFamily, socket)
        {
        }

        #region ITcpServerSocket Members

        public void Listen(int backlog, Action<ITcpSocket> callback)
        {
            try
            {
                socket.Listen(backlog);
                AcceptOne(callback);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                throw new SocketException("Listen failure", Errors.ErrorToSocketError(e.SocketErrorCode));
            }
        }

        #endregion

        #region ITcpSocket Members

        public override void Connect(IPEndPoint endpoint, Action callback, Action<Exception> error)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");
            if (error == null)
                throw new ArgumentNullException("error");

            socket.BeginConnect(endpoint.Address.address, endpoint.Port, (ar) =>
                                                                             {
                                                                                 Context.Enqueue(delegate
                                                                                                     {
                                                                                                         if (!disposed)
                                                                                                         {
                                                                                                             try
                                                                                                             {
                                                                                                                 socket.
                                                                                                                     EndConnect
                                                                                                                     (ar);
                                                                                                                 IsConnected
                                                                                                                     =
                                                                                                                     true;
                                                                                                                 callback
                                                                                                                     ();
                                                                                                             }
                                                                                                             catch (
                                                                                                                 System.
                                                                                                                     Net
                                                                                                                     .
                                                                                                                     Sockets
                                                                                                                     .
                                                                                                                     SocketException
                                                                                                                     e)
                                                                                                             {
                                                                                                                 error(
                                                                                                                     new SocketException
                                                                                                                         ("Connect failure",
                                                                                                                          Errors
                                                                                                                              .
                                                                                                                              ErrorToSocketError
                                                                                                                              (e
                                                                                                                                   .
                                                                                                                                   SocketErrorCode)));
                                                                                                             }
                                                                                                         }
                                                                                                     });
                                                                             }, null);
        }

        public override IByteStream GetSocketStream()
        {
            CheckDisposed();

            if (stream == null)
            {
                stream = new TcpStream(this);
            }
            return stream;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            GetSocketStream().Dispose();
            wasDisposing = disposing;
        }

        private void EndDispose()
        {
            disposed = true;
            base.Dispose(wasDisposing);
        }

        private void AcceptOne(Action<ITcpSocket> callback)
        {
            socket.BeginAccept(ar =>
                                   {
                                       if (!disposed)
                                       {
                                           Socket sock = socket.EndAccept(ar);

                                           Context.Enqueue(delegate
                                                               {
                                                                   callback(new TcpSocket(Context, AddressFamily, sock));
                                                                   AcceptOne(callback);
                                                               });
                                       }
                                   }, null);
        }

        #region Nested type: TcpStream

        private class TcpStream : ManagedByteStream, ISendfileCapable
        {
            private readonly TcpSocket parent;

            internal TcpStream(TcpSocket parent)
                : base(parent.Context, 4*1024)
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
                parent.socket.BeginSendFile(file, ar => { parent.socket.EndSendFile(ar); }, null);
            }

            #endregion

            protected override void Dispose(bool disposing)
            {
                if (!parent.disposed)
                {
                    parent.socket.BeginDisconnect(false, ar =>
                                                             {
                                                                 Context.Enqueue(delegate
                                                                                     {
                                                                                         try
                                                                                         {
                                                                                             ((Socket) ar.AsyncState).
                                                                                                 EndDisconnect(ar);
                                                                                             ((Socket) ar.AsyncState).
                                                                                                 Dispose();
                                                                                         }
                                                                                         catch
                                                                                         {
                                                                                         }

                                                                                         RaiseEndOfStream();

                                                                                         parent.EndDispose();

                                                                                         base.Dispose(disposing);
                                                                                     });
                                                             }, parent.socket);
                }
            }

            protected override void DoRead()
            {
                System.Net.Sockets.SocketError se;
                parent.socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, out se, ReadCallback, null);
            }

            private void ReadCallback(IAsyncResult ar)
            {
                Context.Enqueue(delegate
                                    {
                                        if (!parent.disposed)
                                        {
                                            ResetReadTimeout();

                                            System.Net.Sockets.SocketError error;
                                            int len = parent.socket.EndReceive(ar, out error);

                                            if (error != System.Net.Sockets.SocketError.Success)
                                            {
                                                RaiseError(new SocketException("Read failure",
                                                                               Errors.ErrorToSocketError(error)));
                                            }
                                            else if (len == 0)
                                            {
                                                RaiseEndOfStream();
                                                Close();
                                            }
                                            else
                                            {
                                                var newBuffer = new byte[len];
                                                Buffer.BlockCopy(buffer, 0, newBuffer, 0, len);

                                                RaiseData(new ByteBuffer(newBuffer));
                                                DispatchRead();
                                            }
                                        }
                                    });
            }

            protected override WriteResult WriteSingleFragment(ByteBuffer fragment)
            {
                parent.socket.BeginSend(fragment.Bytes, fragment.Position, fragment.Length, SocketFlags.None,
                                        WriteCallback, null);
                return WriteResult.Consume;
            }

            private void WriteCallback(IAsyncResult ar)
            {
                Context.Enqueue(delegate
                                    {
                                        if (!parent.disposed)
                                        {
                                            ResetWriteTimeout();

                                            System.Net.Sockets.SocketError err;
                                            parent.socket.EndSend(ar, out err);
                                            if (err != System.Net.Sockets.SocketError.Success)
                                            {
                                                RaiseError(new SocketException("Write failure",
                                                                               Errors.ErrorToSocketError(err)));
                                            }
                                            else
                                            {
                                                HandleWrite();
                                            }
                                        }
                                    });
            }
        }

        #endregion
    }
}