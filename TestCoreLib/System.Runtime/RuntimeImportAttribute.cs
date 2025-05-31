namespace System.Runtime;

[AttributeUsage((AttributeTargets)96, Inherited = false)]
internal sealed class RuntimeImportAttribute : Attribute
{
	private readonly string _003CDllName_003Ek__BackingField;

	private readonly string _003CEntryPoint_003Ek__BackingField;

	public string DllName => _003CDllName_003Ek__BackingField;

	public string EntryPoint => _003CEntryPoint_003Ek__BackingField;

	public RuntimeImportAttribute(string entry)
	{
		_003CEntryPoint_003Ek__BackingField = entry;
	}

	public RuntimeImportAttribute(string dllName, string entry)
	{
		_003CEntryPoint_003Ek__BackingField = entry;
		_003CDllName_003Ek__BackingField = dllName;
	}
}
