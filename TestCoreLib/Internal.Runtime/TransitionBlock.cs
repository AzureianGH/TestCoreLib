using System;

namespace Internal.Runtime;

internal struct TransitionBlock
{
	public ReturnBlock m_returnBlock;

	public ArgumentRegisters m_argumentRegisters;

	private IntPtr m_alignmentPadding;

	private IntPtr m_ReturnAddress;

	public const int InvalidOffset = -1;

	public static int GetOffsetOfReturnValuesBlock()
	{
		return 0;
	}

	public unsafe static int GetOffsetOfArgumentRegisters()
	{
		return sizeof(ReturnBlock);
	}

	public unsafe static byte GetOffsetOfArgs()
	{
		return (byte)sizeof(TransitionBlock);
	}

	public static bool IsStackArgumentOffset(int offset)
	{
		int offsetOfArgumentRegisters = GetOffsetOfArgumentRegisters();
		return offset >= offsetOfArgumentRegisters + 48;
	}

	public static bool IsArgumentRegisterOffset(int offset)
	{
		int offsetOfArgumentRegisters = GetOffsetOfArgumentRegisters();
		return offset >= offsetOfArgumentRegisters && offset < offsetOfArgumentRegisters + 48;
	}

	public static int GetArgumentIndexFromOffset(int offset)
	{
		return (offset - GetOffsetOfArgumentRegisters()) / IntPtr.Size;
	}

	public static int GetStackArgumentIndexFromOffset(int offset)
	{
		return (offset - GetOffsetOfArgs()) / 8;
	}

	public static bool IsFloatArgumentRegisterOffset(int offset)
	{
		return offset < 0;
	}

	public static int GetOffsetOfFloatArgumentRegisters()
	{
		return -GetNegSpaceSize();
	}

	public unsafe static int GetNegSpaceSize()
	{
		int num = 0;
		return num + sizeof(FloatArgumentRegisters);
	}

	public static int GetThisOffset()
	{
		return GetOffsetOfArgumentRegisters();
	}
}
