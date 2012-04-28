using System;
using Libev;

namespace Manos.IO.Libev
{
	class Stdin : IStdin
	{
		IOWatcher readWatcher;

		public Stdin (Context context)
		{
			readWatcher = new IOWatcher (new IntPtr(0), EventTypes.Read, context.Loop, (iow, ev) => {
				OnReady();
			});
		}

		void OnReady ()
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
			readWatcher.Start();
		}

		public void ResumeWriting ()
		{
			throw new NotImplementedException ();
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
			readWatcher.Stop();
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
			readWatcher.Dispose();
		}
		#endregion
	}
}

