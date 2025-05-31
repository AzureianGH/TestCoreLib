using System.Runtime.CompilerServices;

namespace System.Threading;

public static class Interlocked
{
	[Intrinsic]
	public static IntPtr CompareExchange(ref IntPtr location1, IntPtr value, IntPtr comparand)
	{
		return (IntPtr)CompareExchange(ref Unsafe.As<IntPtr, long>(ref location1), (long)value, (long)comparand);
	}

	[Intrinsic]
	public static int CompareExchange(ref int location1, int value, int comparand)
	{
		return CompareExchange(ref location1, value, comparand);
	}

	[Intrinsic]
	public static long CompareExchange(ref long location1, long value, long comparand)
	{
		return CompareExchange(ref location1, value, comparand);
	}

	[Intrinsic]
	public static void MemoryBarrier()
	{
		MemoryBarrier();
	}
}
