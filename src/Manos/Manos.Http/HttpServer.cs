//
// Copyright (C) 2010 Jackson Harper (jackson@manosdemono.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//


using System;
using System.Reflection;
using Manos.IO;

namespace Manos.Http
{
    public delegate void HttpConnectionCallback(IHttpTransaction transaction);

    public class HttpServer : IDisposable
    {
        public static readonly string ServerVersion;

        private readonly HttpConnectionCallback callback;
        private readonly bool closeOnEnd;
        private ITcpServerSocket socket;

        static HttpServer()
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            ServerVersion = "Manos/" + v;
        }

        public HttpServer(Context context, HttpConnectionCallback callback, ITcpServerSocket socket,
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
            socket.Bind(new IPEndPoint(IPAddress.Parse(host), port));
            socket.Listen(128, ConnectionAccepted);
        }

        public void RunTransaction(HttpTransaction trans)
        {
            trans.Run();
        }

        private void ConnectionAccepted(ITcpSocket socket)
        {
            HttpTransaction t = HttpTransaction.BeginTransaction(this, socket, callback, closeOnEnd);
        }
    }
}