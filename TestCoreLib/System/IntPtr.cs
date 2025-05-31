using System.Runtime.CompilerServices;

namespace System;

public struct IntPtr
{
	private unsafe void* _value;

	[Intrinsic]
	public static readonly IntPtr Zero;

	public static int Size
	{
		[Intrinsic]
		get
		{
			return 8;
		}
	}

	[Intrinsic]
	public unsafe IntPtr(void* value)
	{
		_value = value;
	}

	[Intrinsic]
	public unsafe IntPtr(int value)
	{
		_value = (void*)value;
	}

	[Intrinsic]
	public unsafe IntPtr(long value)
	{
		_value = (void*)value;
	}

	[Intrinsic]
	public unsafe long ToInt64()
	{
		return (long)_value;
	}

	[Intrinsic]
	public static explicit operator IntPtr(int value)
	{
		return new IntPtr(value);
	}

	[Intrinsic]
	public static explicit operator IntPtr(long value)
	{
		return new IntPtr(value);
	}

	[Intrinsic]
	public unsafe static explicit operator IntPtr(void* value)
	{
		return new IntPtr(value);
	}

	[Intrinsic]
	public unsafe static explicit operator void*(IntPtr value)
	{
		return value._value;
	}

	[Intrinsic]
	public unsafe static explicit operator int(IntPtr value)
	{
		return (int)value._value;
	}

	[Intrinsic]
	public unsafe static explicit operator long(IntPtr value)
	{
		return (long)value._value;
	}

	[Intrinsic]
	public unsafe static bool operator ==(IntPtr value1, IntPtr value2)
	{
		return value1._value == value2._value;
	}

	[Intrinsic]
	public unsafe static bool operator !=(IntPtr value1, IntPtr value2)
	{
		return value1._value != value2._value;
	}

	[Intrinsic]
	public unsafe static IntPtr operator +(IntPtr pointer, int offset)
	{
		return new IntPtr((long)pointer._value + (long)offset);
	}

    public unsafe void* ToPointer() => (void*)_value;
}
