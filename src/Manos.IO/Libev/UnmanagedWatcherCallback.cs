using System;
using System.Runtime.InteropServices;

namespace Libev
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void UnmanagedWatcherCallback(IntPtr watcher, EventTypes revents);
}