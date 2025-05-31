#define DEBUG
using System.Diagnostics;
using Internal.Runtime;

namespace System.Runtime;

internal static class DispatchResolve
{
	public enum ResolveFlags
	{
		Variant = 1,
		DefaultInterfaceImplementation = 2,
		Static = 4,
		IDynamicInterfaceCastable = 8
	}

	public unsafe static IntPtr FindInterfaceMethodImplementationTarget(MethodTable* pTgtType, MethodTable* pItfType, ushort itfSlotNumber, ResolveFlags flags, MethodTable** ppGenericContext)
	{
		Debug.Assert((flags & ResolveFlags.DefaultInterfaceImplementation) == 0);
		MethodTable* ptr = pTgtType;
		ushort num = default(ushort);
		while (true)
		{
			if (ptr != null)
			{
				if (FindImplSlotForCurrentType(ptr, pItfType, itfSlotNumber, flags, &num, ppGenericContext))
				{
					IntPtr result;
					if (num < ptr->NumVtableSlots)
					{
						result = pTgtType->GetVTableStartAddress()[(int)num];
					}
					else
					{
						switch (num)
						{
						case ushort.MaxValue:
							throw pTgtType->GetClasslibException(ExceptionIDs.EntrypointNotFound);
						case 65534:
							throw pTgtType->GetClasslibException(ExceptionIDs.AmbiguousImplementation);
						}
						result = ptr->GetSealedVirtualSlot((ushort)(num - ptr->NumVtableSlots));
					}
					return result;
				}
				ptr = ((!ptr->IsArray) ? ptr->NonArrayBaseType : ptr->GetArrayEEType());
			}
			else
			{
				if ((flags & ResolveFlags.DefaultInterfaceImplementation) != 0)
				{
					break;
				}
				flags |= ResolveFlags.DefaultInterfaceImplementation;
				ptr = pTgtType;
			}
		}
		return IntPtr.Zero;
	}

	private unsafe static bool FindImplSlotForCurrentType(MethodTable* pTgtType, MethodTable* pItfType, ushort itfSlotNumber, ResolveFlags flags, ushort* pImplSlotNumber, MethodTable** ppGenericContext)
	{
		Debug.Assert((flags & ResolveFlags.Variant) == 0);
		bool flag = false;
		if (!pItfType->IsInterface)
		{
			*pImplSlotNumber = itfSlotNumber;
			return pTgtType == pItfType;
		}
		if (pTgtType->HasDispatchMap)
		{
			flag = FindImplSlotInSimpleMap(pTgtType, pItfType, itfSlotNumber, pImplSlotNumber, ppGenericContext, flags);
			if (!flag)
			{
				flags |= ResolveFlags.Variant;
				flag = FindImplSlotInSimpleMap(pTgtType, pItfType, itfSlotNumber, pImplSlotNumber, ppGenericContext, flags);
			}
		}
		return flag;
	}

	private unsafe static bool FindImplSlotInSimpleMap(MethodTable* pTgtType, MethodTable* pItfType, uint itfSlotNumber, ushort* pImplSlotNumber, MethodTable** ppGenericContext, ResolveFlags flags)
	{
		Debug.Assert(pTgtType->HasDispatchMap, "Missing dispatch map");
		MethodTable* ptr = null;
		MethodTableList targetInstantiation = default(MethodTableList);
		int num = 0;
		GenericVariance* pVarianceInfo = null;
		bool flag = false;
		bool flag2 = false;
		if ((flags & ResolveFlags.Variant) != 0)
		{
			flag = pItfType->HasGenericVariance;
			flag2 = pTgtType->IsArray;
			if (!flag2 && pTgtType->HasGenericVariance)
			{
				int genericArity = (int)pTgtType->GenericArity;
				GenericVariance* genericVariance = pTgtType->GenericVariance;
				if (genericArity == 1 && *genericVariance == GenericVariance.ArrayCovariant)
				{
					flag2 = true;
				}
			}
			if (flag2 && pItfType->IsGeneric)
			{
				flag = true;
			}
			if (!flag)
			{
				return false;
			}
		}
		bool flag3 = (flags & ResolveFlags.Static) != 0;
		bool flag4 = (flags & ResolveFlags.DefaultInterfaceImplementation) != 0;
		DispatchMap* dispatchMap = pTgtType->DispatchMap;
		DispatchMap.DispatchMapEntry* ptr2 = (flag3 ? dispatchMap->GetStaticEntry((int)(flag4 ? dispatchMap->NumStandardStaticEntries : 0)) : dispatchMap->GetEntry((int)(flag4 ? dispatchMap->NumStandardEntries : 0)));
		DispatchMap.DispatchMapEntry* ptr3 = (flag3 ? dispatchMap->GetStaticEntry((int)(flag4 ? (dispatchMap->NumStandardStaticEntries + dispatchMap->NumDefaultStaticEntries) : dispatchMap->NumStandardStaticEntries)) : dispatchMap->GetEntry((int)(flag4 ? (dispatchMap->NumStandardEntries + dispatchMap->NumDefaultEntries) : dispatchMap->NumStandardEntries)));
		while (ptr2 != ptr3)
		{
			if (ptr2->_usInterfaceMethodSlot == itfSlotNumber)
			{
				MethodTable* ptr4 = pTgtType->InterfaceMap[(int)ptr2->_usInterfaceIndex];
				if (ptr4 == pItfType)
				{
					*pImplSlotNumber = ptr2->_usImplMethodSlot;
					if (flag3)
					{
						*ppGenericContext = GetGenericContextSource(pTgtType, ptr2);
					}
					else if ((flags & ResolveFlags.IDynamicInterfaceCastable) != 0)
					{
						*ppGenericContext = pTgtType;
					}
					return true;
				}
				if (flag && ((flag2 && ptr4->IsGeneric) || ptr4->HasGenericVariance))
				{
					if (ptr == null)
					{
						ptr = pItfType->GenericDefinition;
						num = (int)pItfType->GenericArity;
						targetInstantiation = pItfType->GenericArguments;
						pVarianceInfo = pItfType->GenericVariance;
					}
					MethodTable* genericDefinition = ptr4->GenericDefinition;
					if (ptr == genericDefinition)
					{
						MethodTableList genericArguments = ptr4->GenericArguments;
						Debug.Assert(num == (int)ptr4->GenericArity, "arity mismatch between generic instantiations");
						if (TypeCast.TypeParametersAreCompatible(num, genericArguments, targetInstantiation, pVarianceInfo, flag2, null))
						{
							*pImplSlotNumber = ptr2->_usImplMethodSlot;
							if (flag3)
							{
								*ppGenericContext = GetGenericContextSource(pTgtType, ptr2);
							}
							else if ((flags & ResolveFlags.IDynamicInterfaceCastable) != 0)
							{
								*ppGenericContext = pTgtType;
							}
							return true;
						}
					}
				}
			}
			ptr2 = (flag3 ? ((DispatchMap.DispatchMapEntry*)((byte*)ptr2 + sizeof(DispatchMap.StaticDispatchMapEntry))) : (ptr2 + 1));
		}
		return false;
	}

	private unsafe static MethodTable* GetGenericContextSource(MethodTable* pTgtType, DispatchMap.DispatchMapEntry* pEntry)
	{
		ushort usContextMapSource = ((DispatchMap.StaticDispatchMapEntry*)pEntry)->_usContextMapSource;
		if (1 == 0)
		{
		}
		MethodTable* result = usContextMapSource switch
		{
			0 => null, 
			1 => pTgtType, 
			_ => pTgtType->InterfaceMap[usContextMapSource - 2], 
		};
		if (1 == 0)
		{
		}
		return result;
	}
}
