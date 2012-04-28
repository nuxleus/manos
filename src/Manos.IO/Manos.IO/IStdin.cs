using System;

namespace Manos.IO
{
	public interface IStdin : IByteStream
	{
		void Ready(Action callback);
	}
}

