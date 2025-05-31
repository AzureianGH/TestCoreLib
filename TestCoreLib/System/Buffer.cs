using System.Runtime;
using System.Runtime.CompilerServices;

namespace System;

public unsafe static class Buffer
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void BulkMoveWithWriteBarrier(ref byte destination, ref byte source, nuint byteCount)
	{
		RuntimeImports.RhBulkMoveWithWriteBarrier(ref destination, ref source, byteCount);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Memmove(byte* destination, byte* source, nuint elementCount)
	{
		if (elementCount == 0 || destination == source)
		{
			return;
		}

		if (destination < source)
		{
			for (nuint i = 0; i < elementCount; i++)
			{
				destination[i] = source[i];
			}
		}
		else
		{
			for (nuint i = elementCount; i > 0; i--)
			{
				destination[i - 1] = source[i - 1];
			}
		}
	}

	public static void Memmove(ref byte destination, ref byte source, nuint elementCount)
	{
		if (elementCount == 0 || Unsafe.AreSame(ref destination, ref source))
		{
			return;
		}

		if (Unsafe.IsAddressLessThan(ref destination, ref source))
		{
			for (nuint i = 0; i < elementCount; i++)
			{
				destination = source;
				destination++;
				source++;
			}
		}
		else
		{
			for (nuint i = elementCount; i > 0; i--)
			{
				destination--;
				source--;
				destination = source;
			}
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static unsafe void Memmove<T>(ref T destination, ref T source, nuint elementCount)
	{
		if (elementCount == 0 || Unsafe.AreSame(ref destination, ref source))
		{
			return;
		}

		nuint byteCount = elementCount * (nuint)Unsafe.SizeOf<T>();

		if (Unsafe.IsAddressLessThan(ref destination, ref source))
		{
			for (nuint i = 0; i < byteCount; i++)
			{
				Unsafe.AddByteOffset(ref Unsafe.As<T, byte>(ref destination), (nint)i) =
					Unsafe.AddByteOffset(ref Unsafe.As<T, byte>(ref source), (nint)i);
			}
		}
		else
		{
			for (nuint i = byteCount; i > 0; i--)
			{
				Unsafe.AddByteOffset(ref Unsafe.As<T, byte>(ref destination), (nint)i - 1) =
					Unsafe.AddByteOffset(ref Unsafe.As<T, byte>(ref source), (nint)i - 1);
			}
		}
	}
}
