using System;
using System.Runtime.InteropServices;
using Mono.Unix;
using Mono.Unix.Native;

namespace Manos.IO.Poll
{
	public class Pipe : IDisposable
	{
		int[] pipe = new int[2];

		public Pipe()
		{
			Syscall.pipe(pipe);
		}

		public IntPtr Out {
			get {
				return new IntPtr(pipe[0]);
			}
		}

		public IntPtr In {
			get {
				return new IntPtr(pipe[1]);
			}
		}

		public void Write(IntPtr buf, ulong count)
		{
			Syscall.write(pipe[1], buf, count);
		}

		public void Read(IntPtr buf, ulong count)
		{
			Syscall.read(pipe[0], buf, count);
		}

		void Close()
		{
			for (int i = 0; i < pipe.Length; i++) {
				Syscall.close(pipe[i]);
			}
		}

		~Pipe()
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
			Close ();
		}
	}
}
