using System;

namespace Manos.Spdy
{
    public class VersionFrame : ControlFrame
    {
        public VersionFrame()
        {
            Type = ControlFrameType.VERSION;
        }

        public VersionFrame(byte[] data, int offset, int length)
        {
            Type = ControlFrameType.VERSION;
            base.Parse(data, offset, length);
            int versionscount = Util.BuildInt(data, offset + 8, 4);
            int index = 12;
            SupportedVersions = new int[versionscount];
            for (int i = 0; i < versionscount; i++)
            {
                SupportedVersions[i] = Util.BuildInt(data, index, 2);
                index += 2;
            }
        }

        public int[] SupportedVersions { get; set; }

        public new byte[] Serialize()
        {
            Length = 4 + SupportedVersions.Length*2;
            byte[] headers = base.Serialize();
            Array.Resize(ref headers, Length + 8);
            Util.IntToBytes(SupportedVersions.Length, ref headers, 8, 4);
            for (int i = 0; i < SupportedVersions.Length; i++)
            {
                Util.IntToBytes(SupportedVersions[i], ref headers, 12 + i*2, 2);
            }
            Console.WriteLine(BitConverter.ToString(headers));
            return headers;
        }
    }
}