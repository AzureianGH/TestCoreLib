using System.Runtime.InteropServices;

namespace Internal.Runtime;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct ArchitectureConstants
{
	public const int MAX_ARG_SIZE = 16777215;

	public const int NUM_ARGUMENT_REGISTERS = 6;

	public const int ARGUMENTREGISTERS_SIZE = 48;

	public const int ENREGISTERED_RETURNTYPE_MAXSIZE = 8;

	public const int ENREGISTERED_RETURNTYPE_INTEGER_MAXSIZE = 8;

	public const int ENREGISTERED_RETURNTYPE_INTEGER_MAXSIZE_PRIMITIVE = 8;

	public const int ENREGISTERED_PARAMTYPE_MAXSIZE = 8;

	public const int STACK_ELEM_SIZE = 8;

	public static int StackElemSize(int size)
	{
		return (size + 8 - 1) & -8;
	}
}
