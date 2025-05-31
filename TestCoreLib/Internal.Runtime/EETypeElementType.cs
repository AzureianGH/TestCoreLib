namespace Internal.Runtime;

internal enum EETypeElementType
{
	Unknown = 0,
	Void = 1,
	Boolean = 2,
	Char = 3,
	SByte = 4,
	Byte = 5,
	Int16 = 6,
	UInt16 = 7,
	Int32 = 8,
	UInt32 = 9,
	Int64 = 10,
	UInt64 = 11,
	IntPtr = 12,
	UIntPtr = 13,
	Single = 14,
	Double = 15,
	ValueType = 16,
	Nullable = 18,
	Class = 20,
	Interface = 21,
	SystemArray = 22,
	Array = 23,
	SzArray = 24,
	ByRef = 25,
	Pointer = 26,
	FunctionPointer = 27
}
