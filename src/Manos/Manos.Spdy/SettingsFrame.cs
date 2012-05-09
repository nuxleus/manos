using System;

namespace Manos.Spdy
{
    public class SettingsFrame : ControlFrame
    {
        private int _CWND;
        private bool _CWNDChanged;
        private int _DownloadBandwidth;
        private bool _DownloadBandwidthChanged;
        private int _MaxConcurrentStreams;
        private bool _MaxConcurrentStreamsChanged;
        private int _RoundTripTime;
        private bool _RoundTripTimeChanged;
        private int _UploadBandwidth;
        private bool _UploadBandwidthChanged;

        public SettingsFrame()
        {
            Type = ControlFrameType.SETTINGS;
        }

        public SettingsFrame(byte[] data, int offset, int length)
        {
            Type = ControlFrameType.SETTINGS;
            base.Parse(data, offset, length);
            int numentries = Util.BuildInt(data, offset + 8, 4);
            int index = offset + 12;
            for (int i = 0; i < numentries; i++)
            {
                byte IDFlags = data[index];
                index++;
                int ID = Util.BuildInt(data, index, 3);
                index += 3;
                int val = Util.BuildInt(data, index, 4);
                switch (ID)
                {
                    case 1:
                        UploadBandwidth = val;
                        break;
                    case 2:
                        DownloadBandwidth = val;
                        break;
                    case 3:
                        RoundTripTime = val;
                        break;
                    case 4:
                        MaxConcurrentStreams = val;
                        break;
                    case 5:
                        CWND = val;
                        break;
                }
            }
        }

        public int UploadBandwidth
        {
            get { return _UploadBandwidth; }
            set
            {
                _UploadBandwidth = value;
                _UploadBandwidthChanged = true;
            }
        }

        public int DownloadBandwidth
        {
            get { return _DownloadBandwidth; }
            set
            {
                _DownloadBandwidth = value;
                _DownloadBandwidthChanged = true;
            }
        }

        public int RoundTripTime
        {
            get { return _RoundTripTime; }
            set
            {
                _RoundTripTime = value;
                _RoundTripTimeChanged = true;
            }
        }

        public int MaxConcurrentStreams
        {
            get { return _MaxConcurrentStreams; }
            set
            {
                _MaxConcurrentStreams = value;
                _MaxConcurrentStreamsChanged = true;
            }
        }

        public int CWND
        {
            get { return _CWND; }
            set
            {
                _CWND = value;
                _CWNDChanged = true;
            }
        }

        // I don't know the best way to do this
        // This whole class needs something better
        // But I don't know what that is yet
        // Maybe using arrays as the backing, which would allow iteration
        public new byte[] Serialize()
        {
            int numchanged = 0;
            Length = 4;
            if (_UploadBandwidthChanged)
            {
                Length += 8;
                numchanged++;
            }
            if (_DownloadBandwidthChanged)
            {
                Length += 8;
                numchanged++;
            }
            if (_RoundTripTimeChanged)
            {
                Length += 8;
                numchanged++;
            }
            if (_MaxConcurrentStreamsChanged)
            {
                Length += 8;
                numchanged++;
            }
            if (_CWNDChanged)
            {
                Length += 8;
                numchanged++;
            }

            byte[] header = base.Serialize();
            Array.Resize(ref header, 8 + Length);
            Util.IntToBytes(numchanged, ref header, 8, 4);
            int index = 12;
            if (_UploadBandwidthChanged)
            {
                Util.IntToBytes(1, ref header, index, 4);
                header[index] = 0x01;
                index += 4;
                Util.IntToBytes(UploadBandwidth, ref header, index, 4);
                index += 4;
            }
            if (_DownloadBandwidthChanged)
            {
                Util.IntToBytes(2, ref header, index, 4);
                header[index] = 0x01;
                index += 4;
                Util.IntToBytes(DownloadBandwidth, ref header, index, 4);
                index += 4;
            }
            if (_RoundTripTimeChanged)
            {
                Util.IntToBytes(3, ref header, index, 4);
                header[index] = 0x01;
                index += 4;
                Util.IntToBytes(RoundTripTime, ref header, index, 4);
                index += 4;
            }
            if (_MaxConcurrentStreamsChanged)
            {
                Util.IntToBytes(4, ref header, index, 4);
                header[index] = 0x01;
                index += 4;
                Util.IntToBytes(MaxConcurrentStreams, ref header, index, 4);
                index += 4;
            }
            if (_CWNDChanged)
            {
                Util.IntToBytes(5, ref header, index, 4);
                header[index] = 0x01;
                index += 4;
                Util.IntToBytes(CWND, ref header, index, 4);
                index += 4;
            }
            Console.WriteLine(BitConverter.ToString(header));
            return header;
        }
    }
}