using System;
using System.IO;
using Manos.IO;

namespace Manos.Spdy
{
    public class SpdyStream
    {
        private readonly DeflatingZlibContext Deflate;
        private readonly ITcpSocket Socket;

        public SpdyStream(ITcpSocket socket, DeflatingZlibContext deflate)
        {
            Socket = socket;
            Deflate = deflate;
            ReplyWritten = false;
        }

        public bool ReplyWritten { get; set; }

        public int StreamID { get; set; }

        public void SendFile(string filename)
        {
            var info = new FileInfo(filename);
            //if (this.Socket.GetSocketStream () is ISendfileCapable) {
            //		DataFrame header = new DataFrame ();
            //		header.StreamID = this.StreamID;
            //		header.Length = (int) info.Length;
            //		this.Socket.GetSocketStream ().Write (header.SerializeHeader ());
            //		((ISendfileCapable) this.Socket.GetSocketStream ()).SendFile (filename);
            //	} else {
            IByteStream str = Socket.Context.OpenFile(filename, OpenMode.Read, 64*1024);
            str.Read((buf) =>
                         {
                             var d = new DataFrame();
                             d.Flags = 0x00;
                             d.StreamID = StreamID;
                             d.Length = buf.Length - buf.Position;
                             d.Data = new byte[d.Length];
                             Array.Copy(buf.Bytes, buf.Position, d.Data, 0, d.Length);
                             byte[] ret = d.Serialize();
                             Socket.GetSocketStream().Write(new ByteBuffer(ret, 0, ret.Length));
                         }, (e) => { }, () =>
                                            {
                                                var d = new DataFrame();
                                                d.Flags = 0x01;
                                                d.StreamID = StreamID;
                                                d.Length = 0;
                                                d.Data = new byte[d.Length];
                                                byte[] ret = d.Serialize();
                                                Socket.GetSocketStream().Write(new ByteBuffer(ret, 0, ret.Length));
                                            });
            //	}
        }

        public void WriteReply(SpdyResponse res, bool done = false)
        {
            var rep = new SynReplyFrame();
            StreamID = rep.StreamID = res.Request.StreamID;
            rep.Version = 2;
            if (done)
                rep.Flags = 0x01;
            else
                rep.Flags = 0x00;
            rep.Headers = new NameValueHeaderBlock();
            rep.Headers["version"] = "HTTP/" + res.Request.MajorVersion + "." + res.Request.MinorVersion;
            rep.Headers["status"] = res.StatusCode.ToString();
            foreach (string header in res.Headers.Keys)
            {
                rep.Headers[header] = res.Headers[header];
            }
            Socket.GetSocketStream().Write(rep.Serialize(Deflate));
            ReplyWritten = true;
        }

        public void Write(ByteBuffer buf)
        {
            Write(buf.Bytes, buf.Position, buf.Length);
        }

        public void Write(byte[] data, int offset, int length)
        {
            var d = new DataFrame();
            d.Flags = 0x00;
            d.StreamID = StreamID;
            d.Length = length - offset;
            d.Data = new byte[d.Length];
            Array.Copy(data, offset, d.Data, 0, length);
            byte[] ret = d.Serialize();
            Socket.GetSocketStream().Write(new ByteBuffer(ret, 0, ret.Length));
        }

        public void End()
        {
            var d = new DataFrame();
            d.Flags = 0x01;
            d.StreamID = StreamID;
            d.Length = 0;
            d.Data = new byte[0];
            byte[] ret = d.Serialize();
            Socket.GetSocketStream().Write(new ByteBuffer(ret, 0, ret.Length));
        }
    }
}