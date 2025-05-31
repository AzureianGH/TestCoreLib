using System.Runtime.CompilerServices;

namespace System;

public class Type
{
	private readonly RuntimeTypeHandle _typeHandle;

	public RuntimeTypeHandle TypeHandle => _typeHandle;

	private Type(RuntimeTypeHandle typeHandle)
	{
		_typeHandle = typeHandle;
	}

	public static Type GetTypeFromHandle(RuntimeTypeHandle rh)
	{
		return new Type(rh);
	}

	[Intrinsic]
	public static bool operator ==(Type left, Type right)
	{
		return RuntimeTypeHandle.ToIntPtr(left._typeHandle) == RuntimeTypeHandle.ToIntPtr(right._typeHandle);
	}

	[Intrinsic]
	public static bool operator !=(Type left, Type right)
	{
		return !(left == right);
	}

	public override bool Equals(object o)
	{
		return o is Type && this == (Type)o;
	}

	public override int GetHashCode()
	{
		return 0;
	}
}
