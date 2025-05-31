namespace System.Runtime.CompilerServices;

public static class Unsafe
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Intrinsic]
	public static IntPtr ByteOffset<T>(ref readonly T origin, ref readonly T target)
	{
		throw new PlatformNotSupportedException();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Intrinsic]
	public unsafe static void* AsPointer<T>(ref readonly T value)
	{
		throw new PlatformNotSupportedException();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Intrinsic]
	public static int SizeOf<T>()
	{
		throw new PlatformNotSupportedException();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Intrinsic]
	public static T As<T>(object value) where T : class
	{
		throw new PlatformNotSupportedException();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Intrinsic]
	public static ref TTo As<TFrom, TTo>(ref TFrom source)
	{
		throw new PlatformNotSupportedException();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Intrinsic]
	public static ref T AddByteOffset<T>(ref T source, IntPtr byteOffset)
	{
		throw new PlatformNotSupportedException();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref T Add<T>(ref T source, int elementOffset)
	{
		return ref AddByteOffset(ref source, (nint)elementOffset * (nint)SizeOf<T>());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref T Add<T>(ref T source, IntPtr elementOffset)
	{
		return ref AddByteOffset(ref source, (nint)elementOffset * SizeOf<T>());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Intrinsic]
	public static ref T AsRef<T>(in T source)
	{
		throw new PlatformNotSupportedException();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Intrinsic]
	public unsafe static T ReadUnaligned<T>(void* source)
	{
		throw new PlatformNotSupportedException();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Intrinsic]
	public static T ReadUnaligned<T>(ref readonly byte source)
	{
		throw new PlatformNotSupportedException();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Intrinsic]
	public unsafe static void CopyBlock(void* destination, void* source, uint byteCount)
	{
		throw new PlatformNotSupportedException();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Intrinsic]
	public static void CopyBlock(ref byte destination, ref readonly byte source, uint byteCount)
	{
		throw new PlatformNotSupportedException();
	}

	//Read like Unsafe.Read<object>((void*)intPtr);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static T Read<T>(void* source)
	{
		throw new PlatformNotSupportedException();
	}

	//AreSame
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool AreSame<T>(ref T left, ref T right)
	{
		if (Unsafe.IsAddressLessThan(ref left, ref right))
		{
			return false;
		}
		else if (Unsafe.IsAddressLessThan(ref right, ref left))
		{
			return false;
		}
		return true;
	}

	//IsAddressLessThan
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsAddressLessThan<T>(ref T left, ref T right)
	{
		return (int)(ByteOffset(ref left, ref right)) < 0;
	}
	
}
