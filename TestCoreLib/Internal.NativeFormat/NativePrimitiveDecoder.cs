#define DEBUG
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Internal.NativeFormat;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct NativePrimitiveDecoder
{
	public unsafe static byte ReadUInt8(ref byte* stream)
	{
		byte result = *stream;
		stream++;
		return result;
	}

	public unsafe static ushort ReadUInt16(ref byte* stream)
	{
		ushort result = Unsafe.ReadUnaligned<ushort>(stream);
		stream += 2;
		return result;
	}

	public unsafe static uint ReadUInt32(ref byte* stream)
	{
		uint result = Unsafe.ReadUnaligned<uint>(stream);
		stream += 4;
		return result;
	}

	public unsafe static ulong ReadUInt64(ref byte* stream)
	{
		ulong result = Unsafe.ReadUnaligned<ulong>(stream);
		stream += 8;
		return result;
	}

	public unsafe static float ReadFloat(ref byte* stream)
	{
		uint num = ReadUInt32(ref stream);
		return *(float*)(&num);
	}

	public unsafe static double ReadDouble(ref byte* stream)
	{
		ulong num = ReadUInt64(ref stream);
		return *(double*)(&num);
	}

	public static uint GetUnsignedEncodingSize(uint value)
	{
		if (value < 128)
		{
			return 1u;
		}
		if (value < 16384)
		{
			return 2u;
		}
		if (value < 2097152)
		{
			return 3u;
		}
		if (value < 268435456)
		{
			return 4u;
		}
		return 5u;
	}

	public unsafe static uint DecodeUnsigned(ref byte* stream)
	{
		uint num = 0u;
		uint num2 = *stream;
		if ((num2 & 1) == 0)
		{
			num = num2 >> 1;
			stream++;
		}
		else if ((num2 & 2) == 0)
		{
			num = (num2 >> 2) | (uint)(stream[1] << 6);
			stream += 2;
		}
		else if ((num2 & 4) == 0)
		{
			num = (num2 >> 3) | (uint)(stream[1] << 5) | (uint)(stream[2] << 13);
			stream += 3;
		}
		else if ((num2 & 8) == 0)
		{
			num = (num2 >> 4) | (uint)(stream[1] << 4) | (uint)(stream[2] << 12) | (uint)(stream[3] << 20);
			stream += 4;
		}
		else
		{
			if ((num2 & 0x10) != 0)
			{
				Debug.Assert(condition: false);
				return 0u;
			}
			stream++;
			num = ReadUInt32(ref stream);
		}
		return num;
	}

	public unsafe static int DecodeSigned(ref byte* stream)
	{
		int num = 0;
		int num2 = *stream;
		if ((num2 & 1) == 0)
		{
			num = (sbyte)num2 >> 1;
			stream++;
		}
		else if ((num2 & 2) == 0)
		{
			num = (num2 >> 2) | (stream[1] << 6);
			stream += 2;
		}
		else if ((num2 & 4) == 0)
		{
			num = (num2 >> 3) | (stream[1] << 5) | (stream[2] << 13);
			stream += 3;
		}
		else if ((num2 & 8) == 0)
		{
			num = (num2 >> 4) | (stream[1] << 4) | (stream[2] << 12) | (stream[3] << 20);
			stream += 4;
		}
		else
		{
			if ((num2 & 0x10) != 0)
			{
				Debug.Assert(condition: false);
				return 0;
			}
			stream++;
			num = (int)ReadUInt32(ref stream);
		}
		return num;
	}

	public unsafe static ulong DecodeUnsignedLong(ref byte* stream)
	{
		ulong num = 0uL;
		byte b = *stream;
		if ((b & 0x1F) != 31)
		{
			num = DecodeUnsigned(ref stream);
		}
		else
		{
			if ((b & 0x20) != 0)
			{
				Debug.Assert(condition: false);
				return 0uL;
			}
			stream++;
			num = ReadUInt64(ref stream);
		}
		return num;
	}

	public unsafe static long DecodeSignedLong(ref byte* stream)
	{
		long num = 0L;
		byte b = *stream;
		if ((b & 0x1F) != 31)
		{
			num = DecodeSigned(ref stream);
		}
		else
		{
			if ((b & 0x20) != 0)
			{
				Debug.Assert(condition: false);
				return 0L;
			}
			stream++;
			num = (long)ReadUInt64(ref stream);
		}
		return num;
	}
}
