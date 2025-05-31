using System;

namespace Internal.Runtime.CompilerHelpers;

internal static class LdTokenHelpers
{
	private static RuntimeTypeHandle GetRuntimeTypeHandle(IntPtr pEEType)
	{
		return new RuntimeTypeHandle(pEEType);
	}

	private static Type GetRuntimeType(IntPtr pEEType)
	{
		return Type.GetTypeFromHandle(new RuntimeTypeHandle(pEEType));
	}
}
