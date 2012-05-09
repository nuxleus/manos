using System.Collections.Specialized;
using System.Text;
using Manos.Http;

namespace Manos.Spdy
{
    public class NameValueHeaderBlock : NameValueCollection
    {
        public static NameValueHeaderBlock Parse(byte[] data, int offset, int length, InflatingZlibContext inflate)
        {
            int bytelength = 2; //for version 2, changes to 4 in version 3
            var ret = new NameValueHeaderBlock();
            byte[] def = inflate.Inflate(data, offset, length);
            int NumberPairs = Util.BuildInt(def, 0, bytelength);
            int index = bytelength;
            while (NumberPairs-- > 0)
            {
                int namelength = Util.BuildInt(def, index, bytelength);
                index += bytelength;
                string name = Encoding.UTF8.GetString(def, index, namelength);
                index += namelength;
                int vallength = Util.BuildInt(def, index, bytelength);
                index += bytelength;
                string vals = Encoding.UTF8.GetString(def, index, vallength);
                index += vallength;
                string[] splitvals = vals.Split(char.MinValue);
                foreach (string str in splitvals)
                {
                    ret.Add(name, str);
                }
            }
            return ret;
        }

        public byte[] UncompressedSerialize()
        {
            int bytelength = 2;
            int arrlen = bytelength;
            // Shouldn't iterate twice?
            // If I have to resize the array at the end, then that'll be O(n) in itself
            // So either way, its O(2n) (I think)
            foreach (string key in AllKeys)
            {
                arrlen += bytelength;
                arrlen += key.Length;
                string val = this[key];
                arrlen += bytelength;
                arrlen += val.Length;
            }
            var ret = new byte[arrlen];
            Util.IntToBytes(Count, ref ret, 0, bytelength);
            int index = bytelength;
            foreach (string key in AllKeys)
            {
                Util.IntToBytes(key.Length, ref ret, index, bytelength);
                index += bytelength;
                foreach (char c in key)
                {
                    ret[index] = (byte) c;
                    index++;
                }
                Util.IntToBytes(this[key].Length, ref ret, index, bytelength);
                index += bytelength;
                string vals = this[key].Replace(' ', char.MinValue);
                foreach (char c in vals)
                {
                    ret[index] = (byte) c;
                    index ++;
                }
            }
            return ret;
        }

        public byte[] Serialize(DeflatingZlibContext deflate)
        {
            byte[] inarr = UncompressedSerialize();
            return deflate.Deflate(inarr, 0, inarr.Length);
            ;
        }

        public HttpHeaders ToHttpHeaders(string[] exclude)
        {
            var h = new HttpHeaders();
            foreach (string key in AllKeys)
            {
                foreach (string str in exclude)
                {
                    if (str == key)
                    {
                        continue;
                    }
                }
                if (!string.IsNullOrEmpty(key))
                {
                    h.SetHeader(key, this[key]);
                }
            }
            return h;
        }
    }
}