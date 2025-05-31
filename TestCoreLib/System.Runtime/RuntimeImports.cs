using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Internal.Runtime;

namespace System.Runtime;

public static class RuntimeImports
{
	private const string RuntimeLibrary = "*";

	[MethodImpl(MethodImplOptions.InternalCall)]
	[RuntimeImport("*", "RhpHandleAlloc")]
	private static extern IntPtr RhpHandleAlloc(object value, GCHandleType type);

	internal static IntPtr RhHandleAlloc(object value, GCHandleType type)
	{
		IntPtr intPtr = RhpHandleAlloc(value, type);
		if (intPtr == IntPtr.Zero)
		{
			throw new OutOfMemoryException();
		}
		return intPtr;
	}

	[DllImport("*")]
	internal unsafe static extern IntPtr RhRegisterFrozenSegment(void* pSegmentStart, nuint allocSize, nuint commitSize, nuint reservedSize);

	[DllImport("*")]
	internal unsafe static extern void RhUpdateFrozenSegment(IntPtr seg, void* allocated, void* committed);

	[DllImport("*")]
	internal static extern void RhUnregisterFrozenSegment(IntPtr pSegmentHandle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[RuntimeImport("*", "RhpGetModuleSection")]
	private static extern IntPtr RhGetModuleSection(ref TypeManagerHandle module, ReadyToRunSectionType section, out int length);

	internal static IntPtr RhGetModuleSection(TypeManagerHandle module, ReadyToRunSectionType section, out int length)
	{
		return RhGetModuleSection(ref module, section, out length);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[RuntimeImport("*", "RhpCreateTypeManager")]
	internal unsafe static extern TypeManagerHandle RhpCreateTypeManager(IntPtr osModule, IntPtr moduleHeader, IntPtr* pClasslibFunctions, int nClasslibFunctions);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[RuntimeImport("*", "RhpRegisterOsModule")]
	internal static extern IntPtr RhpRegisterOsModule(IntPtr osModule);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[RuntimeImport("*", "RhNewObject")]
	private unsafe static extern object RhNewObject(MethodTable* pEEType);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[RuntimeImport("*", "RhNewArray")]
	private unsafe static extern Array RhNewArray(MethodTable* pEEType, int length);

	[DllImport("*")]
	internal unsafe static extern void RhAllocateNewObject(IntPtr pEEType, uint flags, void* pResult);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[RuntimeImport("*", "RhpFallbackFailFast")]
	internal static extern void RhpFallbackFailFast();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[RuntimeImport("*", "RhpLockCmpXchg32")]
	internal static extern int InterlockedCompareExchange(ref int location1, int value, int comparand);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[RuntimeImport("*", "RhpLockCmpXchg32")]
	internal unsafe static extern int InterlockedCompareExchange(int* location1, int value, int comparand);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[RuntimeImport("*", "RhpLockCmpXchg64")]
	internal static extern long InterlockedCompareExchange(ref long location1, long value, long comparand);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[RuntimeImport("*", "RhBulkMoveWithWriteBarrier")]
	internal static extern void RhBulkMoveWithWriteBarrier(ref byte dmem, ref byte smem, nuint size);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[RuntimeImport("*", "RhGetMaxGcGeneration")]
	internal static extern int RhGetMaxGcGeneration();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[RuntimeImport("*", "RhGetGcCollectionCount")]
	internal static extern int RhGetGcCollectionCount(int generation, bool getSpecialGCCount);
}
