using System;
using Manos.Http;
using Manos.IO;

namespace Manos.Spdy
{
    public class SpdyTransaction : IHttpTransaction
    {
        private static int bufferlength = 1000;
        private byte[] data;

        public SpdyTransaction(Context context, SynStreamFrame synstream, SPDYParser parser, SpdyStream writestream,
                               SpdyConnectionCallback callback)
        {
            Context = context;
            SynStream = synstream;
            Parser = parser;
            WriteStream = writestream;
            Callback = callback;
            if ((synstream.Flags & 0x01) == 1)
            {
                data = new byte[0];
                Request = new SpdyRequest(context, SynStream);
                Context.CreateTimerWatcher(new TimeSpan(1), OnRequestReady).Start();
            }
            else
            {
                if (synstream.Headers["Content-Length"] != null)
                {
                    data = new byte[int.Parse(synstream.Headers["Content-Length"])];
                }
                else
                {
                    data = new byte[bufferlength];
                }
                parser.OnData += delegate(DataFrame packet)
                                     {
                                         if (packet.Data.Length > 0)
                                         {
                                             if (packet.Data.Length + DataIndex > data.Length)
                                             {
                                                 Array.Resize(ref data, packet.Data.Length + DataIndex);
                                             }
                                             Array.Copy(packet.Data, 0, data, DataIndex, packet.Data.Length);
                                             DataIndex += packet.Data.Length;
                                         }
                                         if ((packet.Flags & 0x01) == 1)
                                         {
                                             Request = new SpdyRequest(context, SynStream, data);
                                             OnRequestReady();
                                         }
                                     };
            }
        }

        public SynStreamFrame SynStream { get; set; }

        public SPDYParser Parser { get; set; }

        public SpdyStream WriteStream { get; set; }

        public SpdyConnectionCallback Callback { get; set; }

        public byte[] DataArra
        {
            get { return data; }
        }

        private int DataIndex { get; set; }

        #region IHttpTransaction Members

        public Context Context { get; set; }

        public HttpServer Server
        {
            get { throw new NotImplementedException("Server"); }
            set { throw new NotImplementedException("Server"); }
        }

        public IHttpRequest Request { get; private set; }

        public IHttpResponse Response { get; private set; }

        public bool Aborted { get; private set; }

        public bool ResponseReady { get; set; }

        public void OnRequestReady()
        {
            try
            {
                Response = new SpdyResponse(Request as SpdyRequest, WriteStream, Context);
                ResponseReady = true;
                //if( closeOnEnd ) Response.OnEnd += () => Response.Complete( OnResponseFinished );
                Callback(this);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception while running transaction");
                Console.WriteLine(e);
            }
        }

        public void OnResponseFinished()
        {
            //Request.Read (Close);
            //Close();
        }

        public void Abort(int status, string message, params object[] p)
        {
            throw new NotImplementedException();
        }

        #endregion

        public void Close()
        {
            if (Request != null)
                Request.Dispose();

            if (Response != null)
                Response.Dispose();

            Request = null;
            Response = null;
        }
    }
}