#define DEBUG
using System.Runtime;
using System.Runtime.CompilerServices;

namespace System.Diagnostics;

internal static class Debug
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[Conditional("DEBUG")]
	internal static void Assert(bool condition, string message)
	{
		if (!condition)
		{
			RuntimeImports.RhpFallbackFailFast();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Conditional("DEBUG")]
	internal static void Assert(bool condition)
	{
		if (!condition)
		{
			RuntimeImports.RhpFallbackFailFast();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Conditional("DEBUG")]
	internal static void Fail(string message)
	{
		Assert(condition: false, message);
	}
}
