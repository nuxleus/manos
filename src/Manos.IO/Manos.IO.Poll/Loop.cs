using System;
using System.Collections.Generic;
using Mono.Unix.Native;

namespace Manos.IO.Poll
{
	class Loop
	{
		List<PrepareWatcher> prepares;
		List<CheckWatcher> checks;
		List<IdleWatcher> idles;
		List<IOWatcher> iowatchers;
		SortedList<long, TimerWatcher> timeouts;


		public Loop ()
		{
			prepares = new List<PrepareWatcher> ();
			checks = new List<CheckWatcher>();
			idles = new List<IdleWatcher>();
			iowatchers = new List<IOWatcher> ();
			timeouts = new SortedList<long, TimerWatcher> ();
		}

		internal void Add (IOWatcher watcher)
		{
			iowatchers.Add (watcher);
		}
		internal void Remove (IOWatcher watcher)
		{
			iowatchers.Remove (watcher);
		}

		internal void Add (TimerWatcher watcher)
		{
			long ticks = (DateTime.UtcNow + watcher.After).Ticks;
			timeouts.Add(ticks, watcher);
		}
		internal void Remove (TimerWatcher watcher)
		{
			int idx = timeouts.IndexOfValue (watcher);
			if (idx == -1) {
				return;
			}
			timeouts.RemoveAt (idx);
		}

		internal void Add (PrepareWatcher watcher)
		{
			prepares.Add (watcher);
		}
		internal void Remove (PrepareWatcher watcher)
		{
			prepares.Remove (watcher);
		}

		internal void Add (CheckWatcher watcher)
		{
			checks.Add (watcher);
		}
		internal void Remove (CheckWatcher watcher)
		{
			checks.Add (watcher);
		}

		internal void Add (IdleWatcher watcher)
		{
			idles.Add (watcher);
		}
		internal void Remove (IdleWatcher watcher)
		{
			idles.Remove (watcher);
		}


		public void Dispatch (int timeout)
		{
			var now = DateTime.UtcNow;

			int count = iowatchers.Count;
			Pollfd[] pollmap = new Pollfd[count];
			for (int i = 0; i < count; i++) {
				pollmap[i] = iowatchers[i].pollfd;
			}

			var io = iowatchers.ToArray();

			if (timeout != 0 && timeouts.Count > 0) {
				int t = (int)((timeouts.Keys[0] - now.Ticks) / TimeSpan.TicksPerMillisecond);
				if (t < 0) {
					timeout = 0;
				} else if (timeout == -1 || t < timeout) {
					timeout = t;
				}
			}

			foreach (var prepare in prepares) {
				prepare.Call();
			}

			Syscall.poll(pollmap, timeout);

			foreach (var check in checks) {
				check.Call();
			}

			now = DateTime.UtcNow;

			List<TimerWatcher> next = new List<TimerWatcher>();

			foreach (var kvp in timeouts) {
				long key = kvp.Key / TimeSpan.TicksPerMillisecond;
				// that +1 cost me 3 hours of debugging
				// apparently the second value has to be by one bigger
				// otherwise it is too small, and the first entrance is not removed
				// from the queue and the next poll is called with 0 delay
				// so we are iterating once through the entire loop
				// without actually doing anything useful
				long nowt = now.Ticks / TimeSpan.TicksPerMillisecond + 1;
				if (key > nowt) {
					break;
				}

				var timer = kvp.Value;
				next.Add(timer);
			}

			foreach (var timer in next) {
				Remove (timer);
				if (timer.Repeat != TimeSpan.Zero) {
					timeouts.Add ((now + timer.Repeat).Ticks , timer);
				}
			}

			foreach (var timer in next) {
				timer.Call();
			}

			for (int i = 0; i < pollmap.Length; i++) {
				io[i].Dispatch(pollmap[i].revents);
			}

			foreach (var idle in idles) {
				idle.Call();
			}

		}
	}
}

