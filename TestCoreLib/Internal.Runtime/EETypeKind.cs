namespace Internal.Runtime;

internal enum EETypeKind : uint
{
	CanonicalEEType = 0u,
	FunctionPointerEEType = 65536u,
	ParameterizedEEType = 131072u,
	GenericTypeDefEEType = 196608u
}
