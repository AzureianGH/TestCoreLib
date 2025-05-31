using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct CastCache
{
	public CastCache(int initialCacheSize, int maxCacheSize)
	{
	}

	internal CastResult TryGet(nuint source, nuint target)
	{
		return CastResult.MaybeCast;
	}

	internal void TrySet(nuint source, nuint target, bool result)
	{
	}
}
