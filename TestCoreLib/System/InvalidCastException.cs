namespace System;

internal sealed class InvalidCastException : Exception
{
    public InvalidCastException()
        : base("Specified cast is not valid.")
    {
    }
}
