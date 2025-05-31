using System.Runtime.CompilerServices;

namespace System;

internal static class SpanHelpers
{
	[Intrinsic]
	public static void ClearWithoutReferences(ref byte dest, nuint len)
	{
		Fill(ref dest, 0, len);
	}

	[Intrinsic]
	internal static void Memmove(ref byte dest, ref byte src, nuint len)
	{
		if ((nuint)(nint)Unsafe.ByteOffset(in src, in dest) >= len)
		{
			for (nuint num = 0u; num < len; num++)
			{
				Unsafe.Add(ref dest, (nint)num) = Unsafe.Add(ref src, (nint)num);
			}
			return;
		}
		for (nuint num2 = len; num2 != 0; num2--)
		{
			Unsafe.Add(ref dest, (nint)(num2 - 1)) = Unsafe.Add(ref src, (nint)(num2 - 1));
		}
	}

	internal static void Fill(ref byte dest, byte value, nuint len)
	{
		for (nuint num = 0u; num < len; num++)
		{
			Unsafe.Add(ref dest, (nint)num) = value;
		}
	}
}
