namespace System;

internal sealed class ArgumentOutOfRangeException : Exception
{
    public ArgumentOutOfRangeException(string? paramName)
        : base($"Specified argument was out of the range of valid values.")
    {
        ParamName = paramName;
    }

    public string? ParamName { get; }
}
