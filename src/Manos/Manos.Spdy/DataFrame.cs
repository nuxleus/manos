using System;

namespace Manos.Spdy
{
    public class DataFrame
    {
        public DataFrame()
        {
        }

        public DataFrame(byte[] data, int offset, int length)
        {
            StreamID = Util.BuildInt(data, offset, 4);
            Flags = data[offset + 4];
            Length = Util.BuildInt(data, offset + 5, 3);
            Data = new byte[Length];
            Array.Copy(data, offset + 8, Data, 0, Length);
        }

        public int StreamID { get; set; }

        public byte Flags { get; set; }

        public int Length { get; set; }

        public byte[] Data { get; set; }

        public byte[] Serialize()
        {
            var ret = new byte[8 + Data.Length];
            Util.IntToBytes(StreamID, ref ret, 0, 4);
            ret[4] = Flags;
            Util.IntToBytes(Data.Length, ref ret, 5, 3);
            Array.Copy(Data, 0, ret, 8, Data.Length);
            return ret;
        }

        public byte[] SerializeHeader()
        {
            var ret = new byte[8];
            Util.IntToBytes(StreamID, ref ret, 0, 4);
            ret[4] = Flags;
            Util.IntToBytes(Length, ref ret, 5, 3);
            return ret;
        }
    }
}