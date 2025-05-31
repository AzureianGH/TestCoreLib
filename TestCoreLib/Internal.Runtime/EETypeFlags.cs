using System;

namespace Internal.Runtime;

[Flags]
internal enum EETypeFlags : uint
{
	EETypeKindMask = 0x30000u,
	HasDispatchMap = 0x40000u,
	IsDynamicTypeFlag = 0x80000u,
	HasFinalizerFlag = 0x100000u,
	HasSealedVTableEntriesFlag = 0x400000u,
	GenericVarianceFlag = 0x800000u,
	HasPointersFlag = 0x1000000u,
	IsGenericFlag = 0x2000000u,
	ElementTypeMask = 0x7C000000u,
	ElementTypeShift = 0x1Au,
	HasComponentSizeFlag = 0x80000000u
}
