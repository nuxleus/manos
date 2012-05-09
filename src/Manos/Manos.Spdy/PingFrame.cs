using System;

namespace Manos.Spdy
{
    public class PingFrame : ControlFrame
    {
        public PingFrame()
        {
            Type = ControlFrameType.PING;
        }

        public PingFrame(byte[] data, int offset, int length)
        {
            Type = ControlFrameType.PING;
            base.Parse(data, offset, length);
            ID = Util.BuildInt(data, offset + 8, 4);
        }

        public int ID { get; set; }

        public new byte[] Serialize()
        {
            Length = 4;
            byte[] headers = base.Serialize();
            Array.Resize(ref headers, 12);
            Util.IntToBytes(ID, ref headers, 8, 4);
            return headers;
        }
    }
}