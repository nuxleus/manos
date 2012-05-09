using System;

namespace Manos.Spdy
{
    public class WindowUpdateFrame : ControlFrame
    {
        public WindowUpdateFrame()
        {
            Type = ControlFrameType.WINDOW_UPDATE;
        }

        public WindowUpdateFrame(byte[] data, int offset, int length)
        {
            Type = ControlFrameType.WINDOW_UPDATE;
            base.Parse(data, offset, length);
            StreamID = Util.BuildInt(data, offset + 8, 4);
            DeltaWindowSize = Util.BuildInt(data, offset + 12, 4);
        }

        public int StreamID { get; set; }

        public int DeltaWindowSize { get; set; }

        public new byte[] Serialize()
        {
            Length = 8;
            byte[] headers = base.Serialize();
            Array.Resize(ref headers, 16);
            Util.IntToBytes(StreamID, ref headers, 8, 4);
            Util.IntToBytes(DeltaWindowSize, ref headers, 12, 4);
            return headers;
        }
    }
}