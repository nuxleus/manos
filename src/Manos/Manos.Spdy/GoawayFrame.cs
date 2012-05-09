using System;

namespace Manos.Spdy
{
    public class GoawayFrame : ControlFrame
    {
        public GoawayFrame()
        {
            Type = ControlFrameType.GOAWAY;
        }

        public GoawayFrame(byte[] data, int offset, int length)
        {
            Type = ControlFrameType.GOAWAY;
            base.Parse(data, offset, length);
            LastGoodStreamID = Util.BuildInt(data, offset + 8, 4);
            StatusCode = Util.BuildInt(data, offset + 12, 4);
        }

        public int LastGoodStreamID { get; set; }

        public int StatusCode { get; set; }

        public new byte[] Serialize()
        {
            Length = 8;
            byte[] head = base.Serialize();
            Array.Resize(ref head, 16);
            Util.IntToBytes(LastGoodStreamID, ref head, 8, 4);
            Util.IntToBytes(StatusCode, ref head, 12, 4);
            return head;
        }
    }
}