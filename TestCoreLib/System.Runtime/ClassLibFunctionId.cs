namespace System.Runtime;

internal enum ClassLibFunctionId
{
	GetRuntimeException = 0,
	FailFast = 1,
	AppendExceptionStackFrame = 3,
	GetSystemArrayEEType = 5,
	OnFirstChance = 6,
	OnUnhandledException = 7,
	IDynamicCastableIsInterfaceImplemented = 8,
	IDynamicCastableGetInterfaceImplementation = 9,
	ObjectiveCMarshalTryGetTaggedMemory = 10,
	ObjectiveCMarshalGetIsTrackedReferenceCallback = 11,
	ObjectiveCMarshalGetOnEnteredFinalizerQueueCallback = 12,
	ObjectiveCMarshalGetUnhandledExceptionPropagationHandler = 13
}
