using System.Runtime.InteropServices;

namespace Internal.Runtime;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct ReadyToRunHeaderConstants
{
	public const uint Signature = 5395538u;

	public const ushort CurrentMajorVersion = 13;

	public const ushort CurrentMinorVersion = 0;
}
