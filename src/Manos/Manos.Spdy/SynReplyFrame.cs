namespace Manos.Spdy
{
    public class SynReplyFrame : ControlFrame
    {
        public SynReplyFrame()
        {
            Type = ControlFrameType.SYN_REPLY;
        }

        public SynReplyFrame(byte[] data, int offset, int length, InflatingZlibContext inflate)
        {
            Type = ControlFrameType.SYN_REPLY;
            base.Parse(data, offset, length);
            StreamID = Util.BuildInt(data, offset + 8, 4);
            Headers = NameValueHeaderBlock.Parse(data, offset + 14, length - 12, inflate);
                //14 because of 2 unused bytes
        }

        public int StreamID { get; set; }

        public NameValueHeaderBlock Headers { get; set; }

        public byte[] Serialize(DeflatingZlibContext deflate)
        {
            byte[] nvblock = Headers.Serialize(deflate);
            Length = nvblock.Length + 6;
            byte[] header = base.Serialize();
            var middle = new byte[6];
            Util.IntToBytes(StreamID, ref middle, 0, 4);
            return Util.Combine(header, middle, nvblock);
        }
    }
}