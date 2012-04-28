using System;
using Manos.IO;

namespace Manos.IO.Poll
{
	class TimerWatcher : Watcher, ITimerWatcher
	{
		public TimerWatcher (Loop loop, TimeSpan after, TimeSpan repeat, Action cb)
			: base(loop, cb)
		{
			After = after;
			Repeat = repeat;
		}

		public void Again ()
		{
			throw new NotImplementedException ();
		}

		public TimeSpan After {
			get;
			set;
		}

		public TimeSpan Repeat {
			get;
			set;
		}

		public override void StartImpl ()
		{
			Loop.Add (this);
		}

		public override void StopImpl ()
		{
			Loop.Add (this);
		}
	}
}

