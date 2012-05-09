namespace Manos.Spdy
{
    public class SynStreamFrame : ControlFrame
    {
        public SynStreamFrame()
        {
            Type = ControlFrameType.SYN_STREAM;
        }

        public SynStreamFrame(byte[] data, int offset, int length, InflatingZlibContext inflate)
        {
            Type = ControlFrameType.SYN_STREAM;
            base.Parse(data, offset, length);
            StreamID = Util.BuildInt(data, offset + 8, 4);
            AssociatedToStreamID = Util.BuildInt(data, offset + 12, 4);
            Priority = data[16] >> 5;
            Headers = NameValueHeaderBlock.Parse(data, 18, Length - 10, inflate);
        }

        public int StreamID { get; set; }

        public int AssociatedToStreamID { get; set; }

        public int Priority { get; set; }

        public NameValueHeaderBlock Headers { get; set; }

        public byte[] Serialize(DeflatingZlibContext deflate)
        {
            byte[] nvblock = Headers.Serialize(deflate);
            Length = nvblock.Length + 10;
            byte[] header = base.Serialize();
            var middle = new byte[10];
            Util.IntToBytes(StreamID, ref middle, 0, 4);
            Util.IntToBytes(AssociatedToStreamID, ref middle, 4, 4);
            middle[8] = (byte) (Priority << 5);
            return Util.Combine(header, middle, nvblock);
        }
    }
}