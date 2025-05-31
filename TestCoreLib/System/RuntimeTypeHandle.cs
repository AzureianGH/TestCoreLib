using System.Runtime.CompilerServices;

namespace System;

public struct RuntimeTypeHandle
{
	private IntPtr _value;

	internal RuntimeTypeHandle(IntPtr value)
	{
		_value = value;
	}

	[Intrinsic]
	internal static IntPtr ToIntPtr(RuntimeTypeHandle handle)
	{
		return handle._value;
	}
}
