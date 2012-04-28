using System;
using System.Runtime.InteropServices;
using Mono.Unix.Native;
using Manos.IO;

namespace Manos.IO.Poll
{
	class AsyncWatcher : IAsyncWatcher
	{
		Pipe pipe;
		IntPtr data;
		IOWatcher iowatcher;

		public AsyncWatcher (Loop loop, Action callback)
		{
			pipe = new Pipe ();
			data = Marshal.AllocHGlobal(1);
			iowatcher = new IOWatcher (pipe.Out.ToInt32(), PollEvents.POLLIN, loop, (io, events) => {
				if ((events & PollEvents.POLLIN) > 0) {
					pipe.Read (data, 1);
					if (callback != null) {
						callback ();
					}
				}
			});
		}

		public void Send ()
		{
			pipe.Write (data, 1);
		}

		public void Start ()
		{
			iowatcher.Start ();
		}

		public void Stop ()
		{
			iowatcher.Stop ();
		}

		public bool IsRunning {
			get {
				return iowatcher.IsRunning;
			}
		}

		~AsyncWatcher()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			GC.SuppressFinalize(this);
			Dispose (true);
		}

		protected void Dispose (bool disposing)
		{
			if (data != IntPtr.Zero) {
				Marshal.FreeHGlobal(data);
				data = IntPtr.Zero;
			}

			pipe.Dispose();
		}
	}
}

