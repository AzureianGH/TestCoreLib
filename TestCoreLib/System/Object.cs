using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Internal.Runtime;

namespace System;

public class Object
{
	[StructLayout(LayoutKind.Sequential)]
	private class RawData
	{
		public byte Data;
	}

	private unsafe MethodTable* m_pEEType;

	~Object()
	{
	}

	public virtual bool Equals(object o)
	{
		return false;
	}

	public static bool ReferenceEquals(object? objA, object? objB)
	{
		return objA == objB;
	}

	public virtual int GetHashCode()
	{
		return 0;
	}

	internal unsafe MethodTable* GetMethodTable()
	{
		return m_pEEType;
	}

	public ref byte GetRawData()
	{
		return ref Unsafe.As<RawData>(this).Data;
	}

	internal unsafe uint GetRawObjectDataSize()
	{
		return (uint)((int)GetMethodTable()->BaseSize - sizeof(ObjHeader) - sizeof(MethodTable*));
	}
}
