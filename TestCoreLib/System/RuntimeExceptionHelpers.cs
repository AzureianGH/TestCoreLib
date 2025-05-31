#define DEBUG
using System.Diagnostics;
using System.Runtime;

namespace System;

internal static class RuntimeExceptionHelpers
{
	[RuntimeExport("GetRuntimeException")]
	public static Exception GetRuntimeException(ExceptionIDs id)
	{
		try
		{
			switch (id)
			{
			case ExceptionIDs.OutOfMemory:
				return PreallocatedOutOfMemoryException.Instance;
			case ExceptionIDs.Arithmetic:
				return new ArithmeticException();
			case ExceptionIDs.ArrayTypeMismatch:
				return new ArrayTypeMismatchException();
			case ExceptionIDs.DivideByZero:
				return new DivideByZeroException();
			case ExceptionIDs.IndexOutOfRange:
				return new IndexOutOfRangeException();
			case ExceptionIDs.InvalidCast:
				return new InvalidCastException();
			case ExceptionIDs.Overflow:
				return new OverflowException();
			case ExceptionIDs.NullReference:
				return new NullReferenceException();
			case ExceptionIDs.DataMisaligned:
				return new PlatformNotSupportedException();
			default:
				Debug.Fail("unexpected ExceptionID");
				RuntimeImports.RhpFallbackFailFast();
				return null;
			}
		}
		catch
		{
			return null;
		}
	}

	[RuntimeExport("RuntimeFailFast")]
	internal static void RuntimeFailFast(RhFailFastReason reason, Exception exception, IntPtr pExAddress, IntPtr pExContext)
	{
		RuntimeImports.RhpFallbackFailFast();
	}

	public static void FailFast(string message)
	{
		RuntimeImports.RhpFallbackFailFast();
	}

	[RuntimeExport("AppendExceptionStackFrame")]
	private static void AppendExceptionStackFrame(object exceptionObj, IntPtr IP, int flags)
	{
		Exception ex = exceptionObj as Exception;
		if (ex == null)
		{
			FailFast("Exceptions must derive from the System.Exception class");
		}
	}

	[RuntimeExport("OnFirstChanceException")]
	internal static void OnFirstChanceException(object e)
	{
	}

	[RuntimeExport("OnUnhandledException")]
	internal static void OnUnhandledException(object e)
	{
	}
}
