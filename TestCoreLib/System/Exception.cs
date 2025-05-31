namespace System;

public abstract class Exception
{
	private string _exceptionString = "Unknown Exception";
	public string Message => _exceptionString;

	public Exception()
	{
	}

	public Exception(string str)
	{
		_exceptionString = str;
	}
}

public class InvalidProgramException : Exception
{
	public InvalidProgramException()
	{
	}

	public InvalidProgramException(string str) : base(str)
	{
	}
}