using System;
using System.Collections;
using System.Collections.Generic;

namespace Manos.IO
{
    internal class StreamCopySequencer : IEnumerable<ByteBuffer>
    {
        private readonly bool ownsSource;
        private bool active;
        private ByteBuffer currentBuffer;
        private IByteStream source, target;

        public StreamCopySequencer(IByteStream source, IByteStream target, bool ownsSource)
        {
            this.source = source;
            this.target = target;
            this.ownsSource = ownsSource;
        }

        #region IEnumerable<ByteBuffer> Members

        public IEnumerator<ByteBuffer> GetEnumerator()
        {
            return CopySequencer().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private IEnumerable<ByteBuffer> CopySequencer()
        {
            active = true;
            IDisposable reader = source.Read(OnSourceData, OnSourceError, OnSourceClose);
            target.PauseWriting();
            yield return new ByteBuffer(new byte[0], 0, 0);
            while (active)
            {
                ByteBuffer buffer = currentBuffer;
                target.PauseWriting();
                source.ResumeReading();
                yield return buffer;
            }
            reader.Dispose();
            if (ownsSource)
            {
                source.Close();
            }
            source = null;
            target = null;
            currentBuffer = null;
        }

        private void OnSourceData(ByteBuffer buffer)
        {
            currentBuffer = buffer;
            source.PauseReading();
            target.ResumeWriting();
        }

        private void OnSourceClose()
        {
            active = false;
            target.ResumeWriting();
        }

        private void OnSourceError(Exception error)
        {
            active = false;
            target.ResumeWriting();
        }
    }
}