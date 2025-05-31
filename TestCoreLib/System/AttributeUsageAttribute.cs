namespace System;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class AttributeUsageAttribute : Attribute
{
	public bool AllowMultiple
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool Inherited
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public AttributeUsageAttribute(AttributeTargets validOn)
	{
	}

	public AttributeUsageAttribute(AttributeTargets validOn, bool allowMultiple, bool inherited)
	{
	}
}
