using System;

namespace Internal.Runtime;

[Flags]
internal enum EETypeFlagsEx : ushort
{
	HasEagerFinalizerFlag = 1,
	HasCriticalFinalizerFlag = 2,
	IsTrackedReferenceWithFinalizerFlag = 4,
	IDynamicInterfaceCastableFlag = 8,
	IsByRefLikeFlag = 0x10,
	ValueTypeFieldPaddingMask = 0xE0,
	NullableValueOffsetMask = 0x700,
	RequiresAlign8Flag = 0x1000
}
