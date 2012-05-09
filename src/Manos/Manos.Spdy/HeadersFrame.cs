namespace Manos.Spdy
{
    public class HeadersFrame : ControlFrame
    {
        public HeadersFrame()
        {
            Type = ControlFrameType.HEADERS;
        }

        public HeadersFrame(byte[] data, int offset, int length, InflatingZlibContext inflate)
        {
            Type = ControlFrameType.HEADERS;
            base.Parse(data, offset, length);
            StreamID = Util.BuildInt(data, offset + 8, 4);
            Headers = NameValueHeaderBlock.Parse(data, offset + 12, length - 12, inflate);
        }

        public int StreamID { get; set; }

        public NameValueHeaderBlock Headers { get; set; }

        public byte[] Serialize(DeflatingZlibContext deflate)
        {
            byte[] nvblock = Headers.Serialize(deflate);
            Length = nvblock.Length + 4;
            byte[] header = base.Serialize();
            var middle = new byte[4];
            Util.IntToBytes(StreamID, ref middle, 0, 4);
            return Util.Combine(header, middle, nvblock);
        }
    }
}