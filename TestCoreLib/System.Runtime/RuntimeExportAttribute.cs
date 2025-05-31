namespace System.Runtime;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal sealed class RuntimeExportAttribute : Attribute
{
	public string EntryPoint;

	public RuntimeExportAttribute(string entry)
	{
		EntryPoint = entry;
	}
}
