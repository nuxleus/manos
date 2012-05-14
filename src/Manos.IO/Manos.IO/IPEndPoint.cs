using System;

namespace Manos.IO
{
    public class IPEndPoint : EndPoint
    {
        public IPEndPoint(IPAddress address, int port)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (port < 0 || port > 65535)
                throw new ArgumentOutOfRangeException("port");

            Address = address;
            Port = port;
        }

        public override AddressFamily AddressFamily
        {
            get { return Address.AddressFamily; }
        }

        public IPAddress Address { get; private set; }

        public int Port { get; private set; }

        public override string ToString()
        {
            return Address + ":" + Port;
        }

        public override bool Equals(object obj)
        {
            var other = obj as IPEndPoint;
            return other != null && other.Address.Equals(Address) && other.Port == Port;
        }

        public override int GetHashCode()
        {
            return Address.GetHashCode() + Port;
        }
    }
}