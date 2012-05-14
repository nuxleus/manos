using System;

namespace Manos.IO.Libev
{
    internal abstract class EventedByteStream : EventedStream<ByteBuffer>, IByteStream
    {
        internal EventedByteStream(Context context, IntPtr handle)
            : base(context, handle)
        {
        }

        #region IByteStream Members

        public void Write(byte[] data)
        {
            Write(new ByteBuffer(data));
        }

        #endregion

        protected override long FragmentSize(ByteBuffer data)
        {
            return data.Length;
        }
    }
}