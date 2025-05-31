using System;

namespace Internal.Runtime;

internal readonly struct Pointer
{
	private readonly IntPtr _value;

	public IntPtr Value => _value;
}
internal readonly struct Pointer<T> where T : unmanaged
{
	private unsafe readonly T* _value;

	public unsafe T* Value => _value;
}
