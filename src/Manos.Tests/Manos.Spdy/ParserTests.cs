using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace Manos.Spdy.Tests
{
    [TestFixture]
    public class ParserTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Init()
        {
            parser = new SPDYParser(new InflatingZlibContext());
            SynStreamPacket = genSynStream();
            SynReplyPacket = genSynReply();
            RstStreamPacket = genRstStream();
            SettingsPacket = genSettingsPacket();
            PingPacket = genPingPacket();
            GoawayPacket = genGoawayPacket();
            HeadersPacket = genHeadersPacket();
            WindowUpdatePacket = genWindowUpdatePacket();
            VersionPacket = genVersionPacket();
            DataPacket = genDataPacket();
        }

        #endregion

        private SPDYParser parser;
        private byte[] SynStreamPacket;
        private byte[] SynReplyPacket;
        private byte[] RstStreamPacket;
        private byte[] SettingsPacket;
        private byte[] PingPacket;
        private byte[] GoawayPacket;
        private byte[] HeadersPacket;
        private byte[] WindowUpdatePacket;
        private byte[] VersionPacket;
        private byte[] DataPacket;

        private static byte[] combine(params byte[][] all)
        {
            int totallength = all.Select(x => x.Length).Sum();
            var ret = new byte[totallength];
            int index = 0;
            foreach (var b in all)
            {
                b.CopyTo(ret, index);
                index += b.Length;
            }
            return ret;
        }

        private static byte[] inttonbytes(int val, int n)
        {
            var ret = new byte[n];
            for (int i = 0; i < n; i++)
            {
                ret[i] = Convert.ToByte((val >> ((n - (i + 1))*8)) & 0xFF); //kinda gross
            }
            //ret[0] = Convert.ToByte(val >> 24);
            //ret[1] = Convert.ToByte((val >> 16) & 0xFF);
            //ret[2] = Convert.ToByte((val >> 8) & 0xFF);
            //ret[3] = Convert.ToByte(val & 0xFF);
            return ret;
        }

        private static byte[] buildnamevalue(string name, params string[] vals)
        {
            int namelength = name.Length;
            string valstr = String.Join("\0", vals);
            int valslength = valstr.Length;
            int length = namelength + valslength + 8;
            var ret = new byte[length];
            ret[0] = Convert.ToByte(namelength >> 24);
            ret[1] = Convert.ToByte((namelength >> 16) & 0xFF);
            ret[2] = Convert.ToByte((namelength >> 8) & 0xFF);
            ret[3] = Convert.ToByte(namelength & 0xFF);
            int index = 4;
            for (int i = 0; i < namelength; i++)
            {
                ret[index + i] = (byte) name[i];
            }
            index += namelength;
            ret[index] = Convert.ToByte(valslength >> 24);
            ret[index + 1] = Convert.ToByte((valslength >> 16) & 0xFF);
            ret[index + 2] = Convert.ToByte((valslength >> 8) & 0xFF);
            ret[index + 3] = Convert.ToByte((valslength) & 0xFF);
            index += 4;
            for (int i = 0; i < valslength; i++)
            {
                ret[index + i] = (byte) valstr[i];
            }
            index += valslength;
            //Console.WriteLine(BitConverter.ToString(ret));
            return ret;
        }

        private static byte[] genSynStream()
        {
            byte[] method = buildnamevalue("method", "GET");
            byte[] path = buildnamevalue("path", "/test");
            byte[] version = buildnamevalue("version", "HTTP/1.1");
            byte[] host = buildnamevalue("host", "www.google.com:1234");
            byte[] scheme = buildnamevalue("scheme", "https");
            byte[] length = inttonbytes(5, 4);
            byte[] nvblock = combine(length, method, path, version, host, scheme);
            byte[] deflated = (new DeflatingZlibContext()).Deflate(nvblock, 0, nvblock.Length);
            int deflen = deflated.Length;
            //Console.WriteLine(BitConverter.ToString(deflated, 0, deflen));
            var packet = new byte[]
                             {
                                 0x80, // 10000000 Control Frame bit +  empty version bits
                                 0x02, // Version
                                 0x00,
                                 (byte) ControlFrameType.SYN_STREAM, // Enum for type bit, will be 1
                                 0x00, // No Flags
                                 // Length bytes, add to 24 bit integer
                                 0x00,
                                 0x00,
                                 0x00,
                                 // Stream-ID
                                 0x00,
                                 0x00,
                                 0x00,
                                 0x01,
                                 // Associated to Stream ID
                                 0x00,
                                 0x00,
                                 0x00,
                                 0x00,
                                 0x20, // Priority 00100000 (3 bits) 5 unused bits
                                 0x00 // Unused
                             };
            int len = packet.Length;
            Array.Resize(ref packet, packet.Length + deflen);
            Array.Copy(deflated, 0, packet, len, deflen);
            byte[] lenbytes = inttonbytes(10 + deflen, 3); //10 is length of everything else in SYN_STREAM
            //Console.WriteLine(deflen);
            //Console.WriteLine(BitConverter.ToString(packet));
            Array.Copy(lenbytes, 0, packet, 5, 3);
            //Console.WriteLine(BitConverter.ToString(packet));
            return packet;
        }

        private static byte[] genSynReply()
        {
            byte[] status = buildnamevalue("status", "200");
            byte[] version = buildnamevalue("version", "HTTP/1.1");
            byte[] length = inttonbytes(2, 4);
            byte[] nvblock = combine(length, version, status);
            byte[] deflated = (new DeflatingZlibContext()).Deflate(nvblock, 0, nvblock.Length);
            int deflen = deflated.Length;
            var packet = new byte[]
                             {
                                 0x80, // 10000000 Control Frame bit +  empty version bits
                                 0x02, // Version
                                 0x00,
                                 (byte) ControlFrameType.SYN_REPLY, // Enum for type bit, will be 1
                                 0x00, // No Flags
                                 // Length bytes, add to 24 bit integer
                                 0x00,
                                 0x00,
                                 0x00,
                                 // Stream-ID
                                 0x00,
                                 0x00,
                                 0x00,
                                 0x01
                             };
            int len = packet.Length;
            Array.Resize(ref packet, packet.Length + deflen);
            Array.Copy(deflated, 0, packet, len, deflen);
            byte[] lenbytes = inttonbytes(4 + deflen, 3); //10 is length of everything else in SYN_STREAM
            //Console.WriteLine(deflen);
            //Console.WriteLine(BitConverter.ToString(packet));
            Array.Copy(lenbytes, 0, packet, 4, 3);
            //Console.WriteLine(BitConverter.ToString(packet));
            return packet;
        }

        private byte[] genRstStream()
        {
            return new byte[]
                       {
                           0x80, // 10000000 Control Frame bit +  empty version bits
                           0x02, // Version
                           0x00,
                           (byte) ControlFrameType.RST_STREAM, // Enum for type bit, will be 1
                           0x00, // No Flags
                           // Length bytes, add to 24 bit integer - always 8 for this one
                           0x00,
                           0x00,
                           0x08,
                           // Stream-ID
                           0x00,
                           0x00,
                           0x00,
                           0x01,
                           // Status Code
                           0x00,
                           0x00,
                           0x00,
                           0x05
                       };
        }

        public byte[] genSettingsPacket()
        {
            return new byte[]
                       {
                           0x80,
                           0x02,
                           0x00,
                           (byte) ControlFrameType.SETTINGS,
                           0x00, // No Flags
                           // Length
                           0x00,
                           0x00,
                           0x0C,
                           // Number of entries
                           0x00,
                           0x00,
                           0x00,
                           0x01,
                           // ID
                           0x01, //Persist This
                           0x00,
                           0x00,
                           0x01,
                           // Value (3041 kb/s)
                           0x00,
                           0x00,
                           0x0B,
                           0xE1
                       };
        }

        public byte[] genPingPacket()
        {
            return new byte[]
                       {
                           0x80,
                           0x02,
                           0x00,
                           (byte) ControlFrameType.PING,
                           0x00, // No Flags
                           // Length
                           0x00,
                           0x00,
                           0x04,
                           // Number of entries
                           0x00,
                           0x00,
                           0x00,
                           0x01,
                       };
        }

        public byte[] genGoawayPacket()
        {
            return new byte[]
                       {
                           0x80,
                           0x02,
                           0x00,
                           (byte) ControlFrameType.GOAWAY,
                           0x00, // No Flags
                           // Length
                           0x00,
                           0x00,
                           0x08,
                           // Last Good Stream ID
                           0x00,
                           0x00,
                           0x00,
                           0x01,
                           // Status Code (0 - OK)
                           0x00,
                           0x00,
                           0x00,
                           0x00,
                       };
        }

        public byte[] genHeadersPacket()
        {
            byte[] status = buildnamevalue("header1", "value1");
            byte[] version = buildnamevalue("header2", "value2");
            byte[] length = inttonbytes(2, 4);
            byte[] nvblock = combine(length, version, status);
            byte[] deflated = (new DeflatingZlibContext()).Deflate(nvblock, 0, nvblock.Length);
            int deflen = deflated.Length;
            var packet = new byte[]
                             {
                                 0x80, // 10000000 Control Frame bit +  empty version bits
                                 0x02, // Version
                                 0x00,
                                 (byte) ControlFrameType.HEADERS, // Enum for type bit, will be 1
                                 0x00, // No Flags
                                 // Length bytes, add to 24 bit integer
                                 0x00,
                                 0x00,
                                 0x04,
                                 // Stream-ID
                                 0x00,
                                 0x00,
                                 0x00,
                                 0x01
                             };
            int len = packet.Length;
            Array.Resize(ref packet, packet.Length + deflen);
            Array.Copy(deflated, 0, packet, len, deflen);
            byte[] lenbytes = inttonbytes(4 + deflen, 3);
            //Console.WriteLine(deflen);
            //Console.WriteLine(BitConverter.ToString(packet));
            Array.Copy(lenbytes, 0, packet, 4, 3);
            //Console.WriteL
            return packet;
        }

        public byte[] genWindowUpdatePacket()
        {
            return new byte[]
                       {
                           0x80, // 10000000 Control Frame bit +  empty version bits
                           0x02, // Version
                           0x00,
                           (byte) ControlFrameType.WINDOW_UPDATE,
                           0x00, // No Flags
                           // Length bytes, add to 24 bit integer - always 8 for this one
                           0x00,
                           0x00,
                           0x08,
                           // Stream-ID
                           0x00,
                           0x00,
                           0x00,
                           0x01,
                           // Delta Window Size
                           0x00,
                           0x00,
                           0x00,
                           0x05
                       };
        }

        public byte[] genVersionPacket()
        {
            return new byte[]
                       {
                           0x80, // 10000000 Control Frame bit +  empty version bits
                           0x02, // Version
                           0x00,
                           (byte) ControlFrameType.VERSION,
                           0x00, // No Flags
                           // Length bytes, add to 24 bit integer - always 8 for this one
                           0x00,
                           0x00,
                           0x08,
                           // Number of Versions
                           0x00,
                           0x00,
                           0x00,
                           0x02,
                           // Version 1
                           0x00,
                           0x01,
                           // Version 2
                           0x00,
                           0x02
                       };
        }

        public byte[] genDataPacket()
        {
            return new byte[]
                       {
                           // Stream-ID
                           0x00,
                           0x00,
                           0x00,
                           0x01,
                           0x00, // No Flags (0x01 - last frame, 0x02 - Compressed)
                           // Length bytes, add to 24 bit integer - always 8 for this one
                           0x00,
                           0x00,
                           0x08,
                           // Data 
                           0x0A,
                           0x02,
                           0x44,
                           0x05,
                           0x0B,
                           0xD0,
                           0x0F,
                           0x05
                       };
        }

        public void AsyncTest(Action<Action<Action>> cb)
        {
            var wait = new ManualResetEvent(false);
            bool ran = false;
            Action<Action> done = (afterdone) =>
                                      {
                                          ran = true;
                                          wait.Set();
                                          afterdone();
                                      };
            cb(done);
            wait.WaitOne();
            Assert.IsTrue(ran, "Callback Fired");
        }

        [Test]
        public void ParseDataFrame()
        {
            AsyncTest(done =>
                          {
                              SPDYParser.DataHandler handle = (parsed_packet) =>
                                                                  {
                                                                      Assert.AreEqual(1, parsed_packet.StreamID,
                                                                                      "Stream ID");
                                                                      Assert.AreEqual(0x00, parsed_packet.Flags, "Flags");
                                                                      Assert.AreEqual(8, parsed_packet.Length, "Length");
                                                                      CollectionAssert.AreEqual(
                                                                          new byte[]
                                                                              {
                                                                                  0x0A, 0x02, 0x44, 0x05, 0x0B, 0xD0, 0x0F,
                                                                                  0x05
                                                                              }, parsed_packet.Data, "Data");
                                                                  };
                              parser.OnData += handle;
                              parser.Parse(DataPacket, 0, DataPacket.Length);
                              done(() => { parser.OnData -= handle; });
                          });
        }

        [Test]
        public void ParseGoaway()
        {
            AsyncTest(done =>
                          {
                              SPDYParser.GoawayHandler handle = (parsed_packet) =>
                                                                    {
                                                                        Assert.AreEqual(2, parsed_packet.Version,
                                                                                        "Version");
                                                                        Assert.AreEqual(ControlFrameType.GOAWAY,
                                                                                        parsed_packet.Type, "Type");
                                                                        Assert.AreEqual(0x00, parsed_packet.Flags,
                                                                                        "Flags");
                                                                        Assert.AreEqual(8, parsed_packet.Length,
                                                                                        "Length");
                                                                        Assert.AreEqual(1,
                                                                                        parsed_packet.LastGoodStreamID,
                                                                                        "Last Good Stream ID");
                                                                        Assert.AreEqual(0, parsed_packet.StatusCode,
                                                                                        "Status Code");
                                                                    };
                              parser.OnGoaway += handle;
                              parser.Parse(GoawayPacket, 0, GoawayPacket.Length);
                              done(() => { parser.OnGoaway -= handle; });
                          });
        }

        [Test]
        public void ParseHeaders()
        {
            AsyncTest(done =>
                          {
                              SPDYParser.HeadersHandler handle = (parsed_packet) =>
                                                                     {
                                                                         Assert.AreEqual(2, parsed_packet.Version,
                                                                                         "Version");
                                                                         Assert.AreEqual(ControlFrameType.HEADERS,
                                                                                         parsed_packet.Type, "Type");
                                                                         Assert.AreEqual(0x00, parsed_packet.Flags,
                                                                                         "Flags");
                                                                         //Assert.AreEqual(0x05, parsed_packet.Length, "Length");
                                                                         Assert.AreEqual(1, parsed_packet.StreamID,
                                                                                         "Stream ID");
                                                                         Assert.AreEqual("value1",
                                                                                         parsed_packet.Headers["header1"
                                                                                             ], "Header 1");
                                                                         Assert.AreEqual("value2",
                                                                                         parsed_packet.Headers["header2"
                                                                                             ], "Header 2");
                                                                     };
                              parser.OnHeaders += handle;
                              parser.Parse(HeadersPacket, 0, HeadersPacket.Length);
                              done(() => { parser.OnHeaders -= handle; });
                          });
        }

        [Test]
        public void ParsePing()
        {
            AsyncTest(done =>
                          {
                              SPDYParser.PingHandler handle = (parsed_packet) =>
                                                                  {
                                                                      Assert.AreEqual(2, parsed_packet.Version,
                                                                                      "Version");
                                                                      Assert.AreEqual(ControlFrameType.PING,
                                                                                      parsed_packet.Type, "Type");
                                                                      Assert.AreEqual(0x00, parsed_packet.Flags, "Flags");
                                                                      Assert.AreEqual(4, parsed_packet.Length, "Length");
                                                                      Assert.AreEqual(1, parsed_packet.ID, "ID");
                                                                  };
                              parser.OnPing += handle;
                              parser.Parse(PingPacket, 0, PingPacket.Length);
                              done(() => { parser.OnPing -= handle; });
                          });
        }

        [Test]
        public void ParseRstStream()
        {
            AsyncTest(done =>
                          {
                              SPDYParser.RstStreamHandler handle = (parsed_packet) =>
                                                                       {
                                                                           Assert.AreEqual(2, parsed_packet.Version,
                                                                                           "Version");
                                                                           Assert.AreEqual(ControlFrameType.RST_STREAM,
                                                                                           parsed_packet.Type, "Type");
                                                                           Assert.AreEqual(0x00, parsed_packet.Flags,
                                                                                           "Flags");
                                                                           Assert.AreEqual(8, parsed_packet.Length,
                                                                                           "Length");
                                                                           Assert.AreEqual(1, parsed_packet.StreamID,
                                                                                           "Stream ID");
                                                                           Assert.AreEqual(RstStreamStatusCode.CANCEL,
                                                                                           parsed_packet.StatusCode,
                                                                                           "Status Code");
                                                                       };
                              parser.OnRstStream += handle;
                              parser.Parse(RstStreamPacket, 0, RstStreamPacket.Length);
                              done(() => { parser.OnRstStream -= handle; });
                          });
        }

        [Test]
        public void ParseSettings()
        {
            AsyncTest(done =>
                          {
                              SPDYParser.SettingsHandler handle = (parsed_packet) =>
                                                                      {
                                                                          Assert.AreEqual(2, parsed_packet.Version,
                                                                                          "Version");
                                                                          Assert.AreEqual(ControlFrameType.SETTINGS,
                                                                                          parsed_packet.Type, "Type");
                                                                          Assert.AreEqual(0x00, parsed_packet.Flags,
                                                                                          "Flags");
                                                                          Assert.AreEqual(12, parsed_packet.Length,
                                                                                          "Length");
                                                                          Assert.AreEqual(3041,
                                                                                          parsed_packet.UploadBandwidth,
                                                                                          "Upload Bandwidth");
                                                                      };
                              parser.OnSettings += handle;
                              parser.Parse(SettingsPacket, 0, SettingsPacket.Length);
                              done(() => { parser.OnSettings -= handle; });
                          });
        }

        [Test]
        public void ParseSynReply()
        {
            AsyncTest(done =>
                          {
                              SPDYParser.SynReplyHandler handle = (parsed_packet) =>
                                                                      {
                                                                          Assert.AreEqual(2, parsed_packet.Version,
                                                                                          "Version");
                                                                          Assert.AreEqual(ControlFrameType.SYN_REPLY,
                                                                                          parsed_packet.Type, "Type");
                                                                          Assert.AreEqual(0x00, parsed_packet.Flags,
                                                                                          "Flags");
                                                                          Assert.AreEqual(1, parsed_packet.StreamID,
                                                                                          "Stream ID");
                                                                          Assert.AreEqual("200",
                                                                                          parsed_packet.Headers["status"
                                                                                              ], "Status");
                                                                          Assert.AreEqual("HTTP/1.1",
                                                                                          parsed_packet.Headers[
                                                                                              "version"], "HTTP Version");
                                                                      };
                              parser.OnSynReply += handle;
                              parser.Parse(SynReplyPacket, 0, SynReplyPacket.Length);
                              done(() => parser.OnSynReply -= handle);
                          });
        }

        [Test]
        public void ParseSynStream()
        {
            AsyncTest(done =>
                          {
                              SPDYParser.SynStreamHandler handle = (parsed_packet) =>
                                                                       {
                                                                           Assert.AreEqual(2, parsed_packet.Version,
                                                                                           "Version");
                                                                           Assert.AreEqual(ControlFrameType.SYN_STREAM,
                                                                                           parsed_packet.Type, "Type");
                                                                           Assert.AreEqual(0x00, parsed_packet.Flags,
                                                                                           "Flags");
                                                                           Assert.AreEqual(1, parsed_packet.StreamID,
                                                                                           "Stream ID");
                                                                           Assert.AreEqual(0,
                                                                                           parsed_packet.
                                                                                               AssociatedToStreamID,
                                                                                           "Associated to Stream ID");
                                                                           Assert.AreEqual(1, parsed_packet.Priority,
                                                                                           "Priority");
                                                                           Assert.AreEqual("GET",
                                                                                           parsed_packet.Headers[
                                                                                               "method"], "Method");
                                                                           Assert.AreEqual("/test",
                                                                                           parsed_packet.Headers["path"],
                                                                                           "Path");
                                                                           Assert.AreEqual("HTTP/1.1",
                                                                                           parsed_packet.Headers[
                                                                                               "version"],
                                                                                           " HTTP Version");
                                                                           Assert.AreEqual("www.google.com:1234",
                                                                                           parsed_packet.Headers["host"],
                                                                                           "Host");
                                                                           Assert.AreEqual("https",
                                                                                           parsed_packet.Headers[
                                                                                               "scheme"], "Scheme");
                                                                       };
                              parser.OnSynStream += handle;
                              parser.Parse(SynStreamPacket, 0, SynStreamPacket.Length);
                              done(() => parser.OnSynStream -= handle);
                          });
        }

        [Test]
        public void ParseVersion()
        {
            AsyncTest(done =>
                          {
                              SPDYParser.VersionHandler handle = (parsed_packet) =>
                                                                     {
                                                                         Assert.AreEqual(2, parsed_packet.Version,
                                                                                         "Version");
                                                                         Assert.AreEqual(ControlFrameType.VERSION,
                                                                                         parsed_packet.Type, "Type");
                                                                         Assert.AreEqual(0x00, parsed_packet.Flags,
                                                                                         "Flags");
                                                                         Assert.AreEqual(8, parsed_packet.Length,
                                                                                         "Length");
                                                                         Assert.AreEqual(1,
                                                                                         parsed_packet.SupportedVersions
                                                                                             [0],
                                                                                         "First Supported Version");
                                                                         Assert.AreEqual(2,
                                                                                         parsed_packet.SupportedVersions
                                                                                             [1],
                                                                                         "Second Supported Version");
                                                                     };
                              parser.OnVersion += handle;
                              parser.Parse(VersionPacket, 0, VersionPacket.Length);
                              done(() => { parser.OnVersion -= handle; });
                          });
        }

        [Test]
        public void ParseWindowUpdate()
        {
            AsyncTest(done =>
                          {
                              SPDYParser.WindowUpdateHandler handle = (parsed_packet) =>
                                                                          {
                                                                              Assert.AreEqual(2, parsed_packet.Version,
                                                                                              "Version");
                                                                              Assert.AreEqual(
                                                                                  ControlFrameType.WINDOW_UPDATE,
                                                                                  parsed_packet.Type, "Type");
                                                                              Assert.AreEqual(0x00, parsed_packet.Flags,
                                                                                              "Flags");
                                                                              Assert.AreEqual(8, parsed_packet.Length,
                                                                                              "Length");
                                                                              Assert.AreEqual(1, parsed_packet.StreamID,
                                                                                              "Stream ID");
                                                                              Assert.AreEqual(5,
                                                                                              parsed_packet.
                                                                                                  DeltaWindowSize,
                                                                                              "Delta Window Size");
                                                                          };
                              parser.OnWindowUpdate += handle;
                              parser.Parse(WindowUpdatePacket, 0, WindowUpdatePacket.Length);
                              done(() => { parser.OnWindowUpdate -= handle; });
                          });
        }
    }
}