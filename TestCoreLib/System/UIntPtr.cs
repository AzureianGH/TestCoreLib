using System.Runtime.CompilerServices;

namespace System;

public struct UIntPtr
{
	private unsafe void* _value;

	[Intrinsic]
	public static readonly UIntPtr Zero;

	[Intrinsic]
	public unsafe UIntPtr(uint value)
	{
		_value = (void*)value;
	}

	[Intrinsic]
	public unsafe UIntPtr(ulong value)
	{
		_value = (void*)value;
	}

	[Intrinsic]
	public unsafe UIntPtr(void* value)
	{
		_value = value;
	}

	[Intrinsic]
	public unsafe static explicit operator UIntPtr(void* value)
	{
		return new UIntPtr(value);
	}

	[Intrinsic]
	public unsafe static explicit operator void*(UIntPtr value)
	{
		return value._value;
	}

	[Intrinsic]
	public unsafe static explicit operator uint(UIntPtr value)
	{
		return checked((uint)value._value);
	}

	[Intrinsic]
	public unsafe static explicit operator ulong(UIntPtr value)
	{
		return (ulong)value._value;
	}

	[Intrinsic]
	public unsafe static bool operator ==(UIntPtr value1, UIntPtr value2)
	{
		return value1._value == value2._value;
	}

	[Intrinsic]
	public unsafe static bool operator !=(UIntPtr value1, UIntPtr value2)
	{
		return value1._value != value2._value;
	}
}
