#define DEBUG
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Internal.Runtime;

internal static class DehydratedDataCommand
{
	public const byte Copy = 0;

	public const byte ZeroFill = 1;

	public const byte RelPtr32Reloc = 2;

	public const byte PtrReloc = 3;

	public const byte InlineRelPtr32Reloc = 4;

	public const byte InlinePtrReloc = 5;

	private const byte DehydratedDataCommandMask = 7;

	private const int DehydratedDataCommandPayloadShift = 3;

	private const int MaxRawShortPayload = 31;

	private const int MaxExtraPayloadBytes = 3;

	public const int MaxShortPayload = 28;

	public static byte EncodeShort(int command, int commandData)
	{
		Debug.Assert((command & 7) == command);
		Debug.Assert(commandData <= 28);
		return (byte)(command | (commandData << 3));
	}

	public static int Encode(int command, int commandData, byte[] buffer)
	{
		Debug.Assert((command & 7) == command);
		int num = commandData - 28;
		if (num <= 0)
		{
			buffer[0] = EncodeShort(command, commandData);
			return 1;
		}
		int num2 = 0;
		while (num != 0)
		{
			buffer[++num2] = (byte)num;
			num >>= 8;
		}
		if (num2 > 3)
		{
			throw new InvalidOperationException();
		}
		buffer[0] = (byte)(command | (28 + num2 << 3));
		return 1 + num2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static byte* Decode(byte* pB, out int command, out int payload)
	{
		byte b = *pB;
		command = b & 7;
		payload = b >> 3;
		int num = payload - 28;
		if (num > 0)
		{
			payload = *(++pB);
			if (num > 1)
			{
				payload += *(++pB) << 8;
				if (num > 2)
				{
					payload += *(++pB) << 16;
				}
			}
			payload += 28;
		}
		return pB + 1;
	}
}
