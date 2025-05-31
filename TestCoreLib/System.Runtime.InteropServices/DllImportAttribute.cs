namespace System.Runtime.InteropServices;

[AttributeUsage(AttributeTargets.Method)]
public sealed class DllImportAttribute : Attribute
{
	public CallingConvention CallingConvention;

	public string EntryPoint;

	public bool ExactSpelling;

	public DllImportAttribute(string dllName)
	{
	}
}
