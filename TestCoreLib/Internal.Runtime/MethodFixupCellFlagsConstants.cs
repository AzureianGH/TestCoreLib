namespace Internal.Runtime;

internal static class MethodFixupCellFlagsConstants
{
	public const int CharSetMask = 7;

	public const int IsObjectiveCMessageSendMask = 8;

	public const int ObjectiveCMessageSendFunctionMask = 112;

	public const int ObjectiveCMessageSendFunctionShift = 4;

	public const int IsStdcall = 8;
}
