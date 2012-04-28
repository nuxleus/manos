using System;
using Manos.IO;

namespace Manos.IO.Poll
{
	class Context : Manos.IO.Context
	{
		private bool running;
		private IAsyncWatcher runningWatcher;

		public Loop Loop { get; protected set; }

		public Context ()
		{
			Loop = new Loop ();
			runningWatcher = CreateAsyncWatcher(() => { });
		}

		protected override void Dispose (bool disposing)
		{
			Loop = null;
			Stop ();
		}

		public override void Start ()
		{
			running = true;
			while (running) {
				RunOnce ();
			}
		}

		public override void RunOnce ()
		{
			Loop.Dispatch (-1);
		}

		public override void RunOnceNonblocking ()
		{
			Loop.Dispatch (0);
		}

		public override void Stop ()
		{
			running = false;
			runningWatcher.Send ();
		}

		public override ITimerWatcher CreateTimerWatcher (TimeSpan timeout, TimeSpan repeat, Action cb)
		{
			return new TimerWatcher (Loop, timeout, repeat, cb);
		}

		public override ITimerWatcher CreateTimerWatcher (TimeSpan timeout, Action cb)
		{
			return CreateTimerWatcher (timeout, TimeSpan.Zero, cb);
		}

		public override IAsyncWatcher CreateAsyncWatcher (Action cb)
		{
			return new AsyncWatcher (Loop, cb);
		}

		public override IPrepareWatcher CreatePrepareWatcher (Action cb)
		{
			return new PrepareWatcher (Loop, cb);
		}

		public override ICheckWatcher CreateCheckWatcher (Action cb)
		{
			return new CheckWatcher (Loop, cb);
		}

		public override IIdleWatcher CreateIdleWatcher (Action cb)
		{
			return new IdleWatcher (Loop, cb);
		}

		public override ITcpSocket CreateTcpSocket (AddressFamily addressFamily)
		{
			throw new NotImplementedException ();
		}

		public override ITcpServerSocket CreateTcpServerSocket (AddressFamily addressFamily)
		{
			throw new NotImplementedException ();
		}

		public override ITcpSocket CreateSecureSocket (string certFile, string keyFile)
		{
			throw new NotImplementedException ();
		}

		public override IByteStream OpenFile (string fileName, OpenMode openMode, int blockSize)
		{
			throw new NotImplementedException ();
		}

		public override IByteStream CreateFile (string fileName, int blockSize)
		{
			throw new NotImplementedException ();
		}

		public override IUdpSocket CreateUdpSocket (AddressFamily family)
		{
			throw new NotImplementedException ();
		}

		public override IStdin OpenStdin ()
		{
			return new Stdin (Loop);
		}
	}
}

