namespace System.Diagnostics.CodeAnalysis;

public sealed class DoesNotReturnIfAttribute : Attribute
{
	private readonly bool _003CParameterValue_003Ek__BackingField;

	public bool ParameterValue => _003CParameterValue_003Ek__BackingField;

	public DoesNotReturnIfAttribute(bool parameterValue)
	{
		_003CParameterValue_003Ek__BackingField = parameterValue;
	}
}
