namespace Internal.Runtime;

internal static class StaticVirtualMethodContextSource
{
	public const ushort None = 0;

	public const ushort ContextFromThisClass = 1;

	public const ushort ContextFromFirstInterface = 2;
}
