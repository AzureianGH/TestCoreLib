#define DEBUG
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Internal.Runtime;

namespace System.Runtime;

[StackTraceHidden]
[DebuggerStepThrough]
[EagerStaticClassConstruction]
internal static class TypeCast
{
	[Flags]
	internal enum AssignmentVariation
	{
		BoxedSource = 0,
		Unboxed = 1,
		AllowSizeEquivalence = 2
	}

	internal struct EETypePairList
	{
		private unsafe MethodTable* _eetype1;

		private unsafe MethodTable* _eetype2;

		private unsafe EETypePairList* _next;

		public unsafe EETypePairList(MethodTable* pEEType1, MethodTable* pEEType2, EETypePairList* pNext)
		{
			_eetype1 = pEEType1;
			_eetype2 = pEEType2;
			_next = pNext;
		}

		public unsafe static bool Exists(EETypePairList* pList, MethodTable* pEEType1, MethodTable* pEEType2)
		{
			while (pList != null)
			{
				if (pList->_eetype1 == pEEType1 && pList->_eetype2 == pEEType2)
				{
					return true;
				}
				if (pList->_eetype1 == pEEType2 && pList->_eetype2 == pEEType1)
				{
					return true;
				}
				pList = pList->_next;
			}
			return false;
		}
	}

	private const int InitialCacheSize = 8;

	private const int MaximumCacheSize = 512;

	private static CastCache s_castCache = new CastCache(8, 512);

	public unsafe static object IsInstanceOfAny(MethodTable* pTargetType, object obj)
	{
		if (obj != null)
		{
			MethodTable* methodTable = obj.GetMethodTable();
			if (methodTable != pTargetType)
			{
				CastResult castResult = s_castCache.TryGet((nuint)((byte*)methodTable + 0), (nuint)pTargetType);
				if (castResult != CastResult.CanCast)
				{
					if (castResult != CastResult.CannotCast)
					{
						return IsInstanceOfAny_NoCacheLookup(pTargetType, obj);
					}
					obj = null;
				}
			}
		}
		return obj;
	}

	public unsafe static object IsInstanceOfInterface(MethodTable* pTargetType, object obj)
	{
		Debug.Assert(pTargetType->IsInterface);
		Debug.Assert(!pTargetType->HasGenericVariance);
		MethodTable* methodTable;
		nint num;
		MethodTable** ptr;
		if (obj != null)
		{
			methodTable = obj.GetMethodTable();
			num = methodTable->NumInterfaces;
			if (num == 0)
			{
				goto IL_00f7;
			}
			ptr = methodTable->InterfaceMap;
			if (num < 4)
			{
				goto IL_00cb;
			}
			while (*ptr != pTargetType && ptr[1] != pTargetType && ptr[2] != pTargetType && ptr[3] != pTargetType)
			{
				ptr += 4;
				num -= 4;
				if (num >= 4)
				{
					continue;
				}
				goto IL_00bc;
			}
		}
		goto IL_010b;
		IL_00cb:
		while (*ptr != pTargetType)
		{
			ptr++;
			num--;
			if (num > 0)
			{
				continue;
			}
			goto IL_00f7;
		}
		goto IL_010b;
		IL_00f7:
		if (!methodTable->IsIDynamicInterfaceCastable)
		{
			obj = null;
			goto IL_010b;
		}
		return IsInstanceOfInterface_Helper(pTargetType, obj);
		IL_010b:
		return obj;
		IL_00bc:
		if (num != 0)
		{
			goto IL_00cb;
		}
		goto IL_00f7;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private unsafe static object IsInstanceOfInterface_Helper(MethodTable* pTargetType, object obj)
	{
		if (!IsInstanceOfInterfaceViaIDynamicInterfaceCastable(pTargetType, obj, throwing: false))
		{
			obj = null;
		}
		return obj;
	}

	public unsafe static object IsInstanceOfClass(MethodTable* pTargetType, object obj)
	{
		Debug.Assert(!pTargetType->IsParameterizedType, "IsInstanceOfClass called with parameterized MethodTable");
		Debug.Assert(!pTargetType->IsFunctionPointer, "IsInstanceOfClass called with function pointer MethodTable");
		Debug.Assert(!pTargetType->IsInterface, "IsInstanceOfClass called with interface MethodTable");
		Debug.Assert(!pTargetType->HasGenericVariance, "IsInstanceOfClass with variant MethodTable");
		if (obj == null || obj.GetMethodTable() == pTargetType)
		{
			return obj;
		}
		if (!obj.GetMethodTable()->IsCanonical)
		{
			Debug.Assert(obj.GetMethodTable()->IsArray);
			if (!WellKnownEETypes.IsValidArrayBaseType(pTargetType))
			{
				goto IL_013d;
			}
		}
		else
		{
			MethodTable* nonArrayBaseType = obj.GetMethodTable()->NonArrayBaseType;
			while (nonArrayBaseType != pTargetType)
			{
				if (nonArrayBaseType != null)
				{
					nonArrayBaseType = nonArrayBaseType->NonArrayBaseType;
					if (nonArrayBaseType == pTargetType)
					{
						break;
					}
					if (nonArrayBaseType != null)
					{
						nonArrayBaseType = nonArrayBaseType->NonArrayBaseType;
						if (nonArrayBaseType == pTargetType)
						{
							break;
						}
						if (nonArrayBaseType != null)
						{
							nonArrayBaseType = nonArrayBaseType->NonArrayBaseType;
							if (nonArrayBaseType == pTargetType)
							{
								break;
							}
							if (nonArrayBaseType != null)
							{
								nonArrayBaseType = nonArrayBaseType->NonArrayBaseType;
								continue;
							}
						}
					}
				}
				goto IL_013d;
			}
		}
		goto IL_0141;
		IL_0141:
		return obj;
		IL_013d:
		obj = null;
		goto IL_0141;
	}

	public unsafe static bool IsInstanceOfException(MethodTable* pTargetType, object obj)
	{
		if (obj == null)
		{
			return false;
		}
		MethodTable* ptr = obj.GetMethodTable();
		if (ptr == pTargetType)
		{
			return true;
		}
		if (ptr->IsArray)
		{
			return WellKnownEETypes.IsValidArrayBaseType(pTargetType);
		}
		do
		{
			ptr = ptr->NonArrayBaseType;
			if (ptr == null)
			{
				return false;
			}
		}
		while (ptr != pTargetType);
		return true;
	}

	public unsafe static object CheckCastAny(MethodTable* pTargetType, object obj)
	{
		if (obj != null)
		{
			MethodTable* methodTable = obj.GetMethodTable();
			if (methodTable != pTargetType)
			{
				CastResult castResult = s_castCache.TryGet((nuint)methodTable, (nuint)pTargetType);
				if (castResult != CastResult.CanCast)
				{
					object result = CheckCastAny_NoCacheLookup(pTargetType, obj);
					Debug.Assert(castResult != CastResult.CannotCast);
					return result;
				}
			}
		}
		return obj;
	}

	public unsafe static object CheckCastInterface(MethodTable* pTargetType, object obj)
	{
		Debug.Assert(pTargetType->IsInterface);
		Debug.Assert(!pTargetType->HasGenericVariance);
		nint num;
		MethodTable** ptr;
		if (obj != null)
		{
			MethodTable* methodTable = obj.GetMethodTable();
			num = methodTable->NumInterfaces;
			if (num == 0)
			{
				goto IL_00f8;
			}
			ptr = methodTable->InterfaceMap;
			if (num < 4)
			{
				goto IL_00c8;
			}
			while (*ptr != pTargetType && ptr[1] != pTargetType && ptr[2] != pTargetType && ptr[3] != pTargetType)
			{
				ptr += 4;
				num -= 4;
				if (num >= 4)
				{
					continue;
				}
				goto IL_00b9;
			}
		}
		goto IL_00f2;
		IL_00f8:
		return CheckCastInterface_Helper(pTargetType, obj);
		IL_00c8:
		while (*ptr != pTargetType)
		{
			ptr++;
			num--;
			if (num > 0)
			{
				continue;
			}
			goto IL_00f8;
		}
		goto IL_00f2;
		IL_00b9:
		if (num != 0)
		{
			goto IL_00c8;
		}
		goto IL_00f8;
		IL_00f2:
		return obj;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private unsafe static object CheckCastInterface_Helper(MethodTable* pTargetType, object obj)
	{
		if (obj.GetMethodTable()->IsIDynamicInterfaceCastable && IsInstanceOfInterfaceViaIDynamicInterfaceCastable(pTargetType, obj, throwing: true))
		{
			return obj;
		}
		return ThrowInvalidCastException(pTargetType);
	}

	public unsafe static object CheckCastClass(MethodTable* pTargetType, object obj)
	{
		Debug.Assert(!pTargetType->IsParameterizedType, "CheckCastClass called with parameterized MethodTable");
		Debug.Assert(!pTargetType->IsFunctionPointer, "CheckCastClass called with function pointer MethodTable");
		Debug.Assert(!pTargetType->IsInterface, "CheckCastClass called with interface MethodTable");
		Debug.Assert(!pTargetType->HasGenericVariance, "CheckCastClass with variant MethodTable");
		if (obj == null || obj.GetMethodTable() == pTargetType)
		{
			return obj;
		}
		return CheckCastClassSpecial(pTargetType, obj);
	}

	private unsafe static object CheckCastClassSpecial(MethodTable* pTargetType, object obj)
	{
		Debug.Assert(!pTargetType->IsParameterizedType, "CheckCastClassSpecial called with parameterized MethodTable");
		Debug.Assert(!pTargetType->IsFunctionPointer, "CheckCastClassSpecial called with function pointer MethodTable");
		Debug.Assert(!pTargetType->IsInterface, "CheckCastClassSpecial called with interface MethodTable");
		Debug.Assert(!pTargetType->HasGenericVariance, "CheckCastClassSpecial with variant MethodTable");
		MethodTable* ptr = obj.GetMethodTable();
		Debug.Assert(ptr != pTargetType, "The check for the trivial cases should be inlined by the JIT");
		if (!ptr->IsCanonical)
		{
			Debug.Assert(ptr->IsArray);
			if (!WellKnownEETypes.IsValidArrayBaseType(pTargetType))
			{
				goto IL_012a;
			}
		}
		else
		{
			while (true)
			{
				ptr = ptr->NonArrayBaseType;
				if (ptr == pTargetType)
				{
					break;
				}
				if (ptr != null)
				{
					ptr = ptr->NonArrayBaseType;
					if (ptr == pTargetType)
					{
						break;
					}
					if (ptr != null)
					{
						ptr = ptr->NonArrayBaseType;
						if (ptr == pTargetType)
						{
							break;
						}
						if (ptr != null)
						{
							ptr = ptr->NonArrayBaseType;
							if (ptr == pTargetType)
							{
								break;
							}
							if (ptr != null)
							{
								continue;
							}
						}
					}
				}
				goto IL_012a;
			}
		}
		return obj;
		IL_012a:
		return ThrowInvalidCastException(pTargetType);
	}

	private unsafe static bool IsInstanceOfInterfaceViaIDynamicInterfaceCastable(MethodTable* pTargetType, object obj, bool throwing)
	{
		return ((delegate*<object, MethodTable*, bool, bool>)(void*)pTargetType->GetClasslibFunction(ClassLibFunctionId.IDynamicCastableIsInterfaceImplemented))(obj, pTargetType, throwing);
	}

	internal unsafe static bool IsDerived(MethodTable* pDerivedType, MethodTable* pBaseType)
	{
		Debug.Assert(!pDerivedType->IsArray, "did not expect array type");
		Debug.Assert(!pDerivedType->IsParameterizedType, "did not expect parameterType");
		Debug.Assert(!pDerivedType->IsFunctionPointer, "did not expect function pointer");
		Debug.Assert(!pBaseType->IsArray, "did not expect array type");
		Debug.Assert(!pBaseType->IsInterface, "did not expect interface type");
		Debug.Assert(!pBaseType->IsParameterizedType, "did not expect parameterType");
		Debug.Assert(!pBaseType->IsFunctionPointer, "did not expect function pointer");
		Debug.Assert(pBaseType->IsCanonical || pBaseType->IsGenericTypeDefinition, "unexpected MethodTable");
		Debug.Assert(pDerivedType->IsCanonical || pDerivedType->IsGenericTypeDefinition, "unexpected MethodTable");
		do
		{
			if (pDerivedType == pBaseType)
			{
				return true;
			}
			pDerivedType = pDerivedType->NonArrayBaseType;
		}
		while (pDerivedType != null);
		return false;
	}

	private unsafe static bool ImplementsInterface(MethodTable* pObjType, MethodTable* pTargetType, EETypePairList* pVisited)
	{
		Debug.Assert(!pTargetType->IsParameterizedType, "did not expect parameterized type");
		Debug.Assert(!pTargetType->IsFunctionPointer, "did not expect function pointer type");
		Debug.Assert(pTargetType->IsInterface, "IsInstanceOfInterface called with non-interface MethodTable");
		int numInterfaces = pObjType->NumInterfaces;
		MethodTable** interfaceMap = pObjType->InterfaceMap;
		for (int i = 0; i < numInterfaces; i++)
		{
			MethodTable* ptr = interfaceMap[i];
			if (ptr == pTargetType)
			{
				return true;
			}
		}
		bool isArray = pObjType->IsArray;
		if (pTargetType->HasGenericVariance)
		{
			MethodTable* genericDefinition = pTargetType->GenericDefinition;
			MethodTableList genericArguments = pTargetType->GenericArguments;
			int genericArity = (int)pTargetType->GenericArity;
			GenericVariance* genericVariance = pTargetType->GenericVariance;
			Debug.Assert(genericVariance != null, "did not expect empty variance info");
			for (int j = 0; j < numInterfaces; j++)
			{
				MethodTable* ptr2 = interfaceMap[j];
				if (!ptr2->HasGenericVariance)
				{
					continue;
				}
				MethodTable* genericDefinition2 = ptr2->GenericDefinition;
				if (genericDefinition2 == genericDefinition)
				{
					MethodTableList genericArguments2 = ptr2->GenericArguments;
					int genericArity2 = (int)ptr2->GenericArity;
					GenericVariance* genericVariance2 = ptr2->GenericVariance;
					Debug.Assert(genericVariance2 != null, "did not expect empty variance info");
					Debug.Assert(genericArity == genericArity2, "arity mismatch between generic instantiations");
					if (TypeParametersAreCompatible(genericArity, genericArguments2, genericArguments, genericVariance, isArray, pVisited))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private unsafe static bool TypesAreCompatibleViaGenericVariance(MethodTable* pSourceType, MethodTable* pTargetType, EETypePairList* pVisited)
	{
		MethodTable* genericDefinition = pTargetType->GenericDefinition;
		MethodTable* genericDefinition2 = pSourceType->GenericDefinition;
		if (genericDefinition2 == genericDefinition)
		{
			MethodTableList genericArguments = pTargetType->GenericArguments;
			int genericArity = (int)pTargetType->GenericArity;
			GenericVariance* genericVariance = pTargetType->GenericVariance;
			Debug.Assert(genericVariance != null, "did not expect empty variance info");
			MethodTableList genericArguments2 = pSourceType->GenericArguments;
			int genericArity2 = (int)pSourceType->GenericArity;
			GenericVariance* genericVariance2 = pSourceType->GenericVariance;
			Debug.Assert(genericVariance2 != null, "did not expect empty variance info");
			Debug.Assert(genericArity == genericArity2, "arity mismatch between generic instantiations");
			if (TypeParametersAreCompatible(genericArity, genericArguments2, genericArguments, genericVariance, fForceCovariance: false, pVisited))
			{
				return true;
			}
		}
		return false;
	}

	internal unsafe static bool TypeParametersAreCompatible(int arity, MethodTableList sourceInstantiation, MethodTableList targetInstantiation, GenericVariance* pVarianceInfo, bool fForceCovariance, EETypePairList* pVisited)
	{
		for (int i = 0; i < arity; i++)
		{
			MethodTable* ptr = targetInstantiation[i];
			MethodTable* ptr2 = sourceInstantiation[i];
			switch ((!fForceCovariance) ? pVarianceInfo[i] : GenericVariance.ArrayCovariant)
			{
			case GenericVariance.NonVariant:
				if (ptr2 != ptr)
				{
					return false;
				}
				break;
			case GenericVariance.Covariant:
				if (!AreTypesAssignableInternal(ptr2, ptr, AssignmentVariation.Unboxed, pVisited))
				{
					return false;
				}
				break;
			case GenericVariance.ArrayCovariant:
				if (!AreTypesAssignableInternal(ptr2, ptr, AssignmentVariation.AllowSizeEquivalence, pVisited))
				{
					return false;
				}
				break;
			case GenericVariance.Contravariant:
				if (!AreTypesAssignableInternal(ptr, ptr2, AssignmentVariation.Unboxed, pVisited))
				{
					return false;
				}
				break;
			default:
				Debug.Fail("unknown generic variance type");
				break;
			}
		}
		return true;
	}

	[RuntimeExport("RhTypeCast_CheckArrayStore")]
	public unsafe static void CheckArrayStore(object array, object obj)
	{
		if (array != null && obj != null)
		{
			Debug.Assert(array.GetMethodTable()->IsArray, "first argument must be an array");
			MethodTable* relatedParameterType = array.GetMethodTable()->RelatedParameterType;
			if (!AreTypesAssignableInternal(obj.GetMethodTable(), relatedParameterType, AssignmentVariation.BoxedSource, null) && (!obj.GetMethodTable()->IsIDynamicInterfaceCastable || !IsInstanceOfInterfaceViaIDynamicInterfaceCastable(relatedParameterType, obj, throwing: false)))
			{
				throw array.GetMethodTable()->GetClasslibException(ExceptionIDs.ArrayTypeMismatch);
			}
		}
	}

	private unsafe static void ThrowIndexOutOfRangeException(object[] array)
	{
		throw array.GetMethodTable()->GetClasslibException(ExceptionIDs.IndexOutOfRange);
	}

	private unsafe static void ThrowArrayMismatchException(object[] array)
	{
		throw array.GetMethodTable()->GetClasslibException(ExceptionIDs.ArrayTypeMismatch);
	}

	public unsafe static ref object LdelemaRef(object[] array, nint index, MethodTable* elementType)
	{
		Debug.Assert(array == null || array.GetMethodTable()->IsArray, "first argument must be an array");
		if ((nuint)index >= (nuint)(uint)array.Length)
		{
			ThrowIndexOutOfRangeException(array);
		}
		Debug.Assert(index >= 0);
		ref object result = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), index);
		MethodTable* relatedParameterType = array.GetMethodTable()->RelatedParameterType;
		if (elementType != relatedParameterType)
		{
			ThrowArrayMismatchException(array);
		}
		return ref result;
	}

	public unsafe static void StelemRef(object[] array, nint index, object obj)
	{
		Debug.Assert(array == null || array.GetMethodTable()->IsArray, "first argument must be an array");
		if ((nuint)index >= (nuint)(uint)array.Length)
		{
			ThrowIndexOutOfRangeException(array);
		}
		Debug.Assert(index >= 0);
		ref object reference = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), index);
		MethodTable* relatedParameterType = array.GetMethodTable()->RelatedParameterType;
		if (obj != null)
		{
			if (relatedParameterType == obj.GetMethodTable() || array.GetMethodTable() == MethodTable.Of<object[]>())
			{
				InternalCalls.RhpAssignRef(ref reference, obj);
			}
			else
			{
				StelemRef_Helper(ref reference, relatedParameterType, obj);
			}
		}
		else
		{
			reference = null;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private unsafe static void StelemRef_Helper(ref object element, MethodTable* elementType, object obj)
	{
		CastResult castResult = s_castCache.TryGet((nuint)((byte*)obj.GetMethodTable() + 0), (nuint)elementType);
		if (castResult == CastResult.CanCast)
		{
			InternalCalls.RhpAssignRef(ref element, obj);
		}
		else
		{
			StelemRef_Helper_NoCacheLookup(ref element, elementType, obj);
		}
	}

	private unsafe static void StelemRef_Helper_NoCacheLookup(ref object element, MethodTable* elementType, object obj)
	{
		object obj2 = IsInstanceOfAny_NoCacheLookup(elementType, obj);
		if (obj2 == null)
		{
			throw elementType->GetClasslibException(ExceptionIDs.ArrayTypeMismatch);
		}
		InternalCalls.RhpAssignRef(ref element, obj);
	}

	private unsafe static object IsInstanceOfArray(MethodTable* pTargetType, object obj)
	{
		MethodTable* methodTable = obj.GetMethodTable();
		Debug.Assert(pTargetType->IsArray, "IsInstanceOfArray called with non-array MethodTable");
		if (methodTable == pTargetType)
		{
			return obj;
		}
		if (!methodTable->IsArray)
		{
			return null;
		}
		if (methodTable->ParameterizedTypeShape != pTargetType->ParameterizedTypeShape && (!methodTable->IsSzArray || pTargetType->ArrayRank != 1))
		{
			return null;
		}
		if (AreTypesAssignableInternal(methodTable->RelatedParameterType, pTargetType->RelatedParameterType, AssignmentVariation.AllowSizeEquivalence, null))
		{
			return obj;
		}
		return null;
	}

	private unsafe static object CheckCastArray(MethodTable* pTargetEEType, object obj)
	{
		object obj2 = IsInstanceOfArray(pTargetEEType, obj);
		if (obj2 == null)
		{
			return ThrowInvalidCastException(pTargetEEType);
		}
		return obj2;
	}

	private unsafe static object IsInstanceOfVariantType(MethodTable* pTargetType, object obj)
	{
		if (!AreTypesAssignableInternal(obj.GetMethodTable(), pTargetType, AssignmentVariation.BoxedSource, null) && (!obj.GetMethodTable()->IsIDynamicInterfaceCastable || !IsInstanceOfInterfaceViaIDynamicInterfaceCastable(pTargetType, obj, throwing: false)))
		{
			return null;
		}
		return obj;
	}

	private unsafe static object CheckCastVariantType(MethodTable* pTargetType, object obj)
	{
		if (!AreTypesAssignableInternal(obj.GetMethodTable(), pTargetType, AssignmentVariation.BoxedSource, null) && (!obj.GetMethodTable()->IsIDynamicInterfaceCastable || !IsInstanceOfInterfaceViaIDynamicInterfaceCastable(pTargetType, obj, throwing: true)))
		{
			return ThrowInvalidCastException(pTargetType);
		}
		return obj;
	}

	private unsafe static EETypeElementType GetNormalizedIntegralArrayElementType(MethodTable* type)
	{
		EETypeElementType elementType = type->ElementType;
		switch (elementType)
		{
		case EETypeElementType.Byte:
		case EETypeElementType.UInt16:
		case EETypeElementType.UInt32:
		case EETypeElementType.UInt64:
		case EETypeElementType.UIntPtr:
			return elementType - 1;
		default:
			return elementType;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private unsafe static object ThrowInvalidCastException(MethodTable* pMT)
	{
		throw pMT->GetClasslibException(ExceptionIDs.InvalidCast);
	}

	[RuntimeExport("RhTypeCast_AreTypesAssignable")]
	public unsafe static bool AreTypesAssignable(MethodTable* pSourceType, MethodTable* pTargetType)
	{
		if (pTargetType->IsGenericTypeDefinition || pSourceType->IsGenericTypeDefinition)
		{
			return false;
		}
		if (pTargetType->IsNullable && pSourceType->IsValueType && !pSourceType->IsNullable)
		{
			MethodTable* nullableType = pTargetType->NullableType;
			return pSourceType == nullableType;
		}
		return AreTypesAssignableInternal(pSourceType, pTargetType, AssignmentVariation.BoxedSource, null);
	}

	public unsafe static bool AreTypesAssignableInternal(MethodTable* pSourceType, MethodTable* pTargetType, AssignmentVariation variation, EETypePairList* pVisited)
	{
		if (pSourceType == pTargetType)
		{
			return true;
		}
		nuint source = (nuint)((byte*)pSourceType + (uint)variation);
		CastResult castResult = s_castCache.TryGet(source, (nuint)pTargetType);
		if (castResult != CastResult.MaybeCast)
		{
			return castResult == CastResult.CanCast;
		}
		return CacheMiss(pSourceType, pTargetType, variation, pVisited);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private unsafe static bool CacheMiss(MethodTable* pSourceType, MethodTable* pTargetType, AssignmentVariation variation, EETypePairList* pVisited)
	{
		if (EETypePairList.Exists(pVisited, pSourceType, pTargetType))
		{
			return false;
		}
		EETypePairList eETypePairList = new EETypePairList(pSourceType, pTargetType, pVisited);
		bool flag = AreTypesAssignableInternalUncached(pSourceType, pTargetType, variation, &eETypePairList);
		if (flag || !pTargetType->IsInterface || !pSourceType->IsIDynamicInterfaceCastable)
		{
			nuint source = (nuint)((byte*)pSourceType + (uint)variation);
			s_castCache.TrySet(source, (nuint)pTargetType, flag);
		}
		return flag;
	}

	internal unsafe static bool AreTypesAssignableInternalUncached(MethodTable* pSourceType, MethodTable* pTargetType, AssignmentVariation variation, EETypePairList* pVisited)
	{
		bool flag = variation == AssignmentVariation.BoxedSource;
		bool flag2 = (variation & AssignmentVariation.AllowSizeEquivalence) == AssignmentVariation.AllowSizeEquivalence;
		if (pSourceType == pTargetType)
		{
			return true;
		}
		if (pTargetType->IsInterface)
		{
			if (!flag && pSourceType->IsValueType)
			{
				return false;
			}
			if (ImplementsInterface(pSourceType, pTargetType, pVisited))
			{
				return true;
			}
			if (pTargetType->HasGenericVariance && pSourceType->HasGenericVariance)
			{
				return TypesAreCompatibleViaGenericVariance(pSourceType, pTargetType, pVisited);
			}
			return false;
		}
		if (pSourceType->IsInterface)
		{
			return WellKnownEETypes.IsSystemObject(pTargetType);
		}
		if (pTargetType->IsParameterizedType)
		{
			if (pSourceType->IsParameterizedType && pTargetType->ParameterizedTypeShape == pSourceType->ParameterizedTypeShape)
			{
				MethodTable* relatedParameterType = pSourceType->RelatedParameterType;
				if (relatedParameterType->IsPointer)
				{
					return false;
				}
				if (relatedParameterType->IsByRef)
				{
					return false;
				}
				if (relatedParameterType->IsFunctionPointer)
				{
					return false;
				}
				return AreTypesAssignableInternal(pSourceType->RelatedParameterType, pTargetType->RelatedParameterType, AssignmentVariation.AllowSizeEquivalence, pVisited);
			}
			return false;
		}
		if (pTargetType->IsFunctionPointer)
		{
			return false;
		}
		if (pSourceType->IsArray)
		{
			return WellKnownEETypes.IsValidArrayBaseType(pTargetType);
		}
		if (pSourceType->IsParameterizedType)
		{
			return false;
		}
		if (pSourceType->IsFunctionPointer)
		{
			return false;
		}
		if (pSourceType->IsValueType)
		{
			if (flag2 && pTargetType->IsPrimitive)
			{
				if (GetNormalizedIntegralArrayElementType(pSourceType) == GetNormalizedIntegralArrayElementType(pTargetType))
				{
					return true;
				}
				return false;
			}
			if (!flag)
			{
				return false;
			}
		}
		if (pTargetType->HasGenericVariance && pSourceType->HasGenericVariance)
		{
			return TypesAreCompatibleViaGenericVariance(pSourceType, pTargetType, pVisited);
		}
		if (IsDerived(pSourceType, pTargetType))
		{
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private unsafe static object IsInstanceOfAny_NoCacheLookup(MethodTable* pTargetType, object obj)
	{
		MethodTable* methodTable = obj.GetMethodTable();
		object obj2 = (pTargetType->IsArray ? IsInstanceOfArray(pTargetType, obj) : (pTargetType->HasGenericVariance ? IsInstanceOfVariantType(pTargetType, obj) : (pTargetType->IsInterface ? IsInstanceOfInterface(pTargetType, obj) : ((!pTargetType->IsParameterizedType && !pTargetType->IsFunctionPointer) ? IsInstanceOfClass(pTargetType, obj) : null))));
		if (obj2 != null || !pTargetType->IsInterface || !methodTable->IsIDynamicInterfaceCastable)
		{
			nuint source = (nuint)((byte*)methodTable + 0);
			s_castCache.TrySet(source, (nuint)pTargetType, obj2 != null);
		}
		return obj2;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private unsafe static object CheckCastAny_NoCacheLookup(MethodTable* pTargetType, object obj)
	{
		MethodTable* methodTable = obj.GetMethodTable();
		if (pTargetType->IsArray)
		{
			obj = CheckCastArray(pTargetType, obj);
		}
		else if (pTargetType->HasGenericVariance)
		{
			obj = CheckCastVariantType(pTargetType, obj);
		}
		else if (pTargetType->IsInterface)
		{
			obj = CheckCastInterface(pTargetType, obj);
		}
		else
		{
			if (pTargetType->IsParameterizedType || pTargetType->IsFunctionPointer)
			{
				return ThrowInvalidCastException(pTargetType);
			}
			obj = CheckCastClass(pTargetType, obj);
		}
		nuint source = (nuint)((byte*)methodTable + 0);
		s_castCache.TrySet(source, (nuint)pTargetType, result: true);
		return obj;
	}
}
