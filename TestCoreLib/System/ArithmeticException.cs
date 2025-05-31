namespace System;

internal sealed class ArithmeticException : Exception
{
    public ArithmeticException()
        : base("Arithmetic operation resulted in an overflow.")
    {
    }
}
