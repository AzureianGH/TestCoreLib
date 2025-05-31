using System.Runtime.CompilerServices;

namespace System;

[EagerStaticClassConstruction]
internal static class PreallocatedOutOfMemoryException
{
	public static readonly OutOfMemoryException Instance = new OutOfMemoryException();
}
