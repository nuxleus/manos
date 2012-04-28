using System;
using Manos.IO;

namespace Manos.IO.Poll
{
	class PrepareWatcher : Watcher, IPrepareWatcher
	{
		public PrepareWatcher (Loop loop, Action callback)
			: base (loop, callback)
		{
		}

		public override void StartImpl ()
		{
			Loop.Add (this);
		}

		public override void StopImpl ()
		{
			Loop.Remove (this);
		}
	}
}

