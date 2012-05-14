namespace Manos.IO.Libev
{
    internal struct ManosIPEndpoint
    {
        private readonly byte a1;
        private readonly byte a10;
        private readonly byte a11;
        private readonly byte a12;
        private readonly byte a13;
        private readonly byte a14;
        private readonly byte a15;
        private readonly byte a16;
        private readonly byte a2;
        private readonly byte a3;
        private readonly byte a4;
        private readonly byte a5;
        private readonly byte a6;
        private readonly byte a7;
        private readonly byte a8;
        private readonly byte a9;
        private readonly int is_ipv4;
        private readonly int port;

        public ManosIPEndpoint(IPEndPoint ep)
        {
            port = ep.Port;
            is_ipv4 = ep.AddressFamily == AddressFamily.InterNetwork ? 1 : 0;

            byte[] b = ep.Address.GetAddressBytes();

            a1 = ValueOrZero(b, 0);
            a2 = ValueOrZero(b, 1);
            a3 = ValueOrZero(b, 2);
            a4 = ValueOrZero(b, 3);
            a5 = ValueOrZero(b, 4);
            a6 = ValueOrZero(b, 5);
            a7 = ValueOrZero(b, 6);
            a8 = ValueOrZero(b, 7);
            a9 = ValueOrZero(b, 8);
            a10 = ValueOrZero(b, 9);
            a11 = ValueOrZero(b, 10);
            a12 = ValueOrZero(b, 11);
            a13 = ValueOrZero(b, 12);
            a14 = ValueOrZero(b, 13);
            a15 = ValueOrZero(b, 14);
            a16 = ValueOrZero(b, 15);
        }

        public IPAddress Address
        {
            get
            {
                if (is_ipv4 != 0)
                {
                    return new IPAddress(new[] {a1, a2, a3, a4});
                }
                else
                {
                    return new IPAddress(new[] {a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16});
                }
            }
        }

        public int Port
        {
            get { return port; }
        }

        private static byte ValueOrZero(byte[] b, int index)
        {
            if (index >= b.Length)
            {
                return 0;
            }
            else
            {
                return b[index];
            }
        }

        public static implicit operator ManosIPEndpoint(IPEndPoint ep)
        {
            return new ManosIPEndpoint(ep);
        }

        public static implicit operator IPEndPoint(ManosIPEndpoint ep)
        {
            return new IPEndPoint(ep.Address, ep.Port);
        }
    }
}