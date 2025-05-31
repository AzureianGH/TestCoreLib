namespace System;

internal sealed class IndexOutOfRangeException : Exception
{
    public IndexOutOfRangeException()
        : base("Index was outside the bounds of the array.")
    {
    }
}
