namespace System.Runtime.InteropServices;

[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public sealed class FieldOffsetAttribute : Attribute
{
	public int Value => 0;

	public FieldOffsetAttribute(int offset)
	{
	}
}
