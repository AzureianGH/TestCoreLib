using System.Runtime.InteropServices;

namespace System;

[StructLayout(LayoutKind.Sequential)]
internal class RawArrayData
{
	public uint Length;

	public uint Padding;

	public byte Data;
}
