using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Internal.Runtime.CompilerHelpers;

internal static class StartupDebug
{
	[Conditional("DEBUG")]
	public unsafe static void Assert([DoesNotReturnIf(false)] bool condition)
	{
		if (!condition)
		{
			*(int*)null = 0;
		}
	}
}
