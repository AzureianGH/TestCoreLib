namespace System.Runtime.InteropServices;

[AttributeUsage((AttributeTargets)12, Inherited = false)]
public sealed class StructLayoutAttribute : Attribute
{
	public LayoutKind Value;

	public int Pack;

	public int Size;

	public CharSet CharSet;

	public StructLayoutAttribute(LayoutKind layoutKind)
	{
	}
}
