using System;
using Mono.Unix.Native;

namespace Manos.IO.Poll
{
	class IOWatcher
	{
		internal Pollfd pollfd;
		private Action<IOWatcher, PollEvents> callback;
		private Loop loop;

		public IOWatcher (int fd, PollEvents events, Loop loop, Action<IOWatcher, PollEvents> callback)
		{
			pollfd = new Pollfd();
			pollfd.fd = fd;
			pollfd.events = events;
			this.callback = callback;
			this.loop = loop;
		}

		public void Start ()
		{
			IsRunning = true;
			loop.Add (this);
		}

		public void Stop ()
		{
			IsRunning = false;
			loop.Remove (this);
		}
		public bool IsRunning { get; protected set; }

		internal void Dispatch (PollEvents events)
		{
			callback (this, events);
		}
	}
}

