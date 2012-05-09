namespace Manos.Spdy
{
    public abstract class ControlFrame
    {
        public int Version { get; set; }

        public ControlFrameType Type { get; set; }

        public byte Flags { get; set; }

        public int Length { get; set; }

        private byte[] FrameData { get; set; }

        public void Parse(byte[] data, int offset, int length)
        {
            Version = data[offset + 1];
            Flags = data[offset + 4];
            Length = Util.BuildInt(data, offset + 5, 3);
        }

        public byte[] Serialize()
        {
            var header = new byte[8];
            header[0] = (byte) (0x80 | ((Version >> 8) & 0xff));
            header[1] = (byte) (Version & 0xff);
            header[2] = (byte) ((int) Type >> 8);
            header[3] = (byte) Type;
            header[4] = Flags;
            header[5] = (byte) ((Length >> 16) & 0xFF);
            header[6] = (byte) ((Length >> 8) & 0xFF);
            header[7] = (byte) (Length & 0xFF);
            return header;
        }
    }

    public enum ControlFrameType
    {
        SYN_STREAM = 1,
        SYN_REPLY = 2,
        RST_STREAM = 3,
        SETTINGS = 4,
        PING = 6,
        GOAWAY = 7,
        HEADERS = 8,
        WINDOW_UPDATE = 9,
        VERSION = 10
    }
}