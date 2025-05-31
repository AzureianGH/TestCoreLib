using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices;

public static class MemoryMarshal
{
	[Intrinsic]
	public static ref T GetArrayDataReference<T>(T[] array)
	{
		return ref GetArrayDataReference(array);
	}
}
