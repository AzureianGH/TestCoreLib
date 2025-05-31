namespace System;

internal sealed class DivideByZeroException : Exception
{
    public DivideByZeroException()
        : base("Attempted to divide by zero.")
    {
    }
}
