namespace System.Runtime.CompilerServices;

[AttributeUsage((AttributeTargets)96, Inherited = false)]
public sealed class MethodImplAttribute : Attribute
{
	internal MethodImplOptions _val;

	public MethodImplOptions Value => _val;

	public MethodImplAttribute(MethodImplOptions methodImplOptions)
	{
		_val = methodImplOptions;
	}

	public MethodImplAttribute(short value)
	{
		_val = (MethodImplOptions)value;
	}

	public MethodImplAttribute()
	{
	}
}
