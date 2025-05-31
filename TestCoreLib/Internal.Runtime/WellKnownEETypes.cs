using System.Runtime.CompilerServices;

namespace Internal.Runtime;

internal static class WellKnownEETypes
{
	internal unsafe static bool IsSystemObject(MethodTable* pEEType)
	{
		if (pEEType->IsArray)
		{
			return false;
		}
		return pEEType->NonArrayBaseType == null && !pEEType->IsInterface;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe static bool IsValidArrayBaseType(MethodTable* pEEType)
	{
		return pEEType->ElementType switch
		{
			EETypeElementType.Class => pEEType->NonArrayBaseType == null, 
			EETypeElementType.SystemArray => true, 
			_ => false, 
		};
	}
}
