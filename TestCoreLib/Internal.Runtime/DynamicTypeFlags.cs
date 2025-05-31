using System;

namespace Internal.Runtime;

[Flags]
internal enum DynamicTypeFlags
{
	HasLazyCctor = 1,
	HasGCStatics = 2,
	HasNonGCStatics = 4,
	HasThreadStatics = 8
}
