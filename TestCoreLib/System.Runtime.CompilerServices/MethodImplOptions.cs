namespace System.Runtime.CompilerServices;

public enum MethodImplOptions
{
	Unmanaged = 4,
	NoInlining = 8,
	NoOptimization = 0x40,
	AggressiveInlining = 0x100,
	AggressiveOptimization = 0x200,
	InternalCall = 0x1000
}
