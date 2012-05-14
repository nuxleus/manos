namespace Manos.IO.Managed
{
    internal abstract class ManagedByteStream : ManagedStream<ByteBuffer>, IByteStream
    {
        protected ManagedByteStream(Context ctx, int bufferSize)
            : base(ctx, bufferSize)
        {
        }

        #region IByteStream Members

        public void Write(byte[] data)
        {
            Write(new ByteBuffer(data));
        }

        #endregion

        protected override long FragmentSize(ByteBuffer fragment)
        {
            return fragment.Length;
        }
    }
}