namespace System;

internal struct Nullable<T> where T : struct
{
	private readonly bool _hasValue;

	private T _value;
}
