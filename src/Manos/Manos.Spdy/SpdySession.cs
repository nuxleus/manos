using System;
using Manos.IO;

namespace Manos.Spdy
{
    public class SpdySession
    {
        private readonly SpdyConnectionCallback callback;
        private readonly SPDYParser parser;
        private readonly ITcpSocket socket;
        public DeflatingZlibContext Deflate;
        public InflatingZlibContext Inflate;
        private int laststreamid;

        public SpdySession(Context context, ITcpSocket sock, SpdyConnectionCallback cb)
        {
            socket = sock;
            callback = cb;
            Inflate = new InflatingZlibContext();
            Deflate = new DeflatingZlibContext();
            Context = context;
            parser = new SPDYParser(Inflate);
            parser.OnSynStream += HandleSynStream;
            parser.OnRstStream += HandleRstStream;
            parser.OnPing += HandlePing;
            socket.GetSocketStream().Read(onData, onError, onEndOfStream);
        }

        public Context Context { get; set; }

        private void HandlePing(PingFrame packet)
        {
            socket.GetSocketStream().Write(packet.Serialize());
        }

        private void HandleRstStream(RstStreamFrame packet)
        {
            socket.Close();
        }

        private void HandleSynStream(SynStreamFrame packet)
        {
            if (packet.StreamID < laststreamid)
            {
                var rst = new RstStreamFrame();
                rst.StreamID = packet.StreamID;
                rst.StatusCode = RstStreamStatusCode.PROTOCOL_ERROR;
                socket.GetSocketStream().Write(rst.Serialize());
                socket.Close();
                return;
            }
            laststreamid = packet.StreamID;
            var t = new SpdyTransaction(Context, packet, parser, new SpdyStream(socket, Deflate), callback);
        }

        private void onData(ByteBuffer data)
        {
            parser.Parse(data.Bytes, data.Position, data.Length);
        }

        private void onError(Exception error)
        {
        }

        private void onEndOfStream()
        {
        }
    }
}