namespace System;

internal sealed class ArgumentNullException : Exception
{
    public ArgumentNullException(string? paramName)
        : base($"Value cannot be null.")
    {
        ParamName = paramName;
    }

    public string? ParamName { get; }
}
