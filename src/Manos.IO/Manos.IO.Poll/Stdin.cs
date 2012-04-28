using System;
using Mono.Unix.Native;

namespace Manos.IO.Poll
{
	class Stdin : IStdin
	{
		IOWatcher reader;

		public Stdin (Loop loop)
		{
			reader = new IOWatcher (0, PollEvents.POLLIN, loop, (iow, ev) => {
				if ((ev & PollEvents.POLLIN) > 0) {
					OnReadyEvent ();
				}
			});
		}

		protected void OnReadyEvent ()
		{
			if (ReadyEvent != null) {
				ReadyEvent ();
			}
		}

		event Action ReadyEvent;

		#region IStdin implementation
		public void Ready (Action callback)
		{
			ReadyEvent += callback;
			ResumeReading ();
		}
		#endregion

		#region IByteStream implementation
		public void Write (byte[] data)
		{
			throw new NotImplementedException ();
		}
		#endregion

		#region IStream[Manos.IO.ByteBuffer] implementation
		public IDisposable Read (Action<ByteBuffer> onData, Action<Exception> onError, Action onEndOfStream)
		{
			throw new NotImplementedException ();
		}

		void IStream<ByteBuffer>.Write (System.Collections.Generic.IEnumerable<ByteBuffer> data)
		{
			throw new NotImplementedException ();
		}

		void IStream<ByteBuffer>.Write (ByteBuffer data)
		{
			throw new NotImplementedException ();
		}

		public void ResumeReading ()
		{
			reader.Start();
		}

		public void ResumeWriting ()
		{
			reader.Stop();
		}

		public void PauseReading ()
		{
			throw new NotImplementedException ();
		}

		public void PauseWriting ()
		{
			throw new NotImplementedException ();
		}

		public void SeekBy (long delta)
		{
			throw new NotImplementedException ();
		}

		public void SeekTo (long position)
		{
			throw new NotImplementedException ();
		}

		public void Flush ()
		{
			throw new NotImplementedException ();
		}

		public void Close ()
		{
			throw new NotImplementedException ();
		}

		public Manos.IO.Context Context {
			get {
				throw new NotImplementedException ();
			}
		}

		public long Position {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public bool CanRead {
			get {
				throw new NotImplementedException ();
			}
		}

		public bool CanWrite {
			get {
				throw new NotImplementedException ();
			}
		}

		public bool CanSeek {
			get {
				throw new NotImplementedException ();
			}
		}

		public bool CanTimeout {
			get {
				throw new NotImplementedException ();
			}
		}

		public TimeSpan ReadTimeout {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public TimeSpan WriteTimeout {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		#endregion

		#region IDisposable implementation
		public void Dispose ()
		{
			reader.Stop();
		}
		#endregion
	}
}

