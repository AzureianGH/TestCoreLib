using System;

namespace Internal.Runtime.CompilerHelpers;

internal static class ThrowHelpers
{
	private static void ThrowOverflowException()
	{
		throw new OverflowException();
	}

	private static void ThrowIndexOutOfRangeException()
	{
		throw new IndexOutOfRangeException();
	}

	private static void ThrowNullReferenceException()
	{
		throw new NullReferenceException();
	}

	private static void ThrowDivideByZeroException()
	{
		throw new DivideByZeroException();
	}

	private static void ThrowArrayTypeMismatchException()
	{
		throw new ArrayTypeMismatchException();
	}

	private static void ThrowPlatformNotSupportedException()
	{
		throw new PlatformNotSupportedException();
	}

	private static void ThrowTypeLoadException()
	{
		throw new PlatformNotSupportedException();
	}

	private static void ThrowArgumentException()
	{
		throw new PlatformNotSupportedException();
	}

	private static void ThrowArgumentOutOfRangeException()
	{
		throw new PlatformNotSupportedException();
	}

	private static void ThrowInvalidProgramException()
	{
		throw new InvalidProgramException();
	}

	private static void ThrowInvalidProgramExceptionWithArgument(string argument)
	{
		throw new InvalidProgramException(argument);
	}
}