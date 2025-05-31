namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
public sealed class InlineArrayAttribute : Attribute
{
	private readonly int _003CLength_003Ek__BackingField;

	public int Length => _003CLength_003Ek__BackingField;

	public InlineArrayAttribute(int length)
	{
		_003CLength_003Ek__BackingField = length;
	}
}
