using System.Runtime;
using System.Runtime.CompilerServices;
using Internal.Runtime;

namespace System;

public class Array
{
	public int _numComponents;

	public int Length => (int)Unsafe.As<RawArrayData>(this).Length;

	[RuntimeExport("GetSystemArrayEEType")]
	private unsafe static MethodTable* GetSystemArrayEEType()
	{
		return MethodTable.Of<Array>();
	}

	public static bool operator ==(Array left, Array right)
	{
		return ReferenceEquals(left, right);
	}

	public static bool operator !=(Array left, Array right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		return ReferenceEquals(this, obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}

internal class Array<T> : Array
{
	public static bool operator ==(Array<T> left, Array<T> right)
	{
		return ReferenceEquals(left, right);
	}

	public static bool operator !=(Array<T> left, Array<T> right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		return ReferenceEquals(this, obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
