using System;
using Manos.IO;

namespace Manos.IO.Poll
{
	abstract class Watcher : IBaseWatcher
	{
		Action cb;

		protected Loop Loop { get; set; }

		public Watcher (Loop loop, Action callback)
		{
			Loop = loop;
			cb = callback;
		}

		public void Call ()
		{
			if (cb != null) {
				cb ();
			}
		}

		bool disposed = false;

		public void Start ()
		{
			if (disposed) {
				return;
			}
			if (!IsRunning) {
				IsRunning = true;
				StartImpl();
			}
		}

		public abstract void StartImpl ();

		public void Stop ()
		{
			if (disposed) {
				return;
			}
			if (IsRunning) {
				IsRunning = false;
				StopImpl();
			}
		}
		public abstract void StopImpl ();

		public bool IsRunning {
			get;
			protected set;
		}

		public void Dispose ()
		{
			disposed = true;
		}
	}
}

