namespace System;

internal sealed class ArrayTypeMismatchException : Exception
{
    public ArrayTypeMismatchException()
        : base("Attempted to access an element as a type incompatible with the array.")
    {
    }
}
