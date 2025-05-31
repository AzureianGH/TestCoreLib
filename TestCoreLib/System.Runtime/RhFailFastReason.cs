namespace System.Runtime;

internal enum RhFailFastReason
{
	Unknown,
	InternalError,
	UnhandledException,
	UnhandledExceptionFromPInvoke,
	EnvironmentFailFast,
	AssertionFailure
}
