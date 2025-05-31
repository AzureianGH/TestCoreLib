#define DEBUG
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Internal.Runtime;

namespace System.Runtime;

internal static class CachedInterfaceDispatch
{
	[RuntimeExport("RhpCidResolve")]
	private unsafe static IntPtr RhpCidResolve(IntPtr callerTransitionBlockParam, IntPtr pCell)
	{
		IntPtr intPtr = callerTransitionBlockParam + TransitionBlock.GetThisOffset();
		object pObject = Unsafe.Read<object>((void*)intPtr);
		return RhpCidResolve_Worker(pObject, pCell);
	}

	private unsafe static IntPtr RhpCidResolve_Worker(object pObject, IntPtr pCell)
	{
		InternalCalls.RhpGetDispatchCellInfo(pCell, out var newCellInfo);
		IntPtr intPtr = RhResolveDispatchWorker(pObject, (void*)pCell, ref newCellInfo);
		if (intPtr != IntPtr.Zero)
		{
			if (!pObject.GetMethodTable()->IsIDynamicInterfaceCastable)
			{
				return InternalCalls.RhpUpdateDispatchCellCache(pCell, intPtr, pObject.GetMethodTable(), ref newCellInfo);
			}
			return intPtr;
		}
		EH.FallbackFailFast(RhFailFastReason.InternalError, null);
		return IntPtr.Zero;
	}

	[RuntimeExport("RhpResolveInterfaceMethod")]
	private unsafe static IntPtr RhpResolveInterfaceMethod(object pObject, IntPtr pCell)
	{
		if (pObject == null)
		{
			return IntPtr.Zero;
		}
		MethodTable* methodTable = pObject.GetMethodTable();
		IntPtr intPtr = InternalCalls.RhpSearchDispatchCellCache(pCell, methodTable);
		if (intPtr == IntPtr.Zero)
		{
			intPtr = RhpCidResolve_Worker(pObject, pCell);
		}
		return intPtr;
	}

	[RuntimeExport("RhResolveDispatch")]
	private unsafe static IntPtr RhResolveDispatch(object pObject, MethodTable* interfaceType, ushort slot)
	{
		DispatchCellInfo cellInfo = new DispatchCellInfo
		{
			CellType = DispatchCellType.InterfaceAndSlot,
			InterfaceType = interfaceType,
			InterfaceSlot = slot
		};
		return RhResolveDispatchWorker(pObject, null, ref cellInfo);
	}

	[RuntimeExport("RhResolveDispatchOnType")]
	private unsafe static IntPtr RhResolveDispatchOnType(MethodTable* pInstanceType, MethodTable* pInterfaceType, ushort slot)
	{
		return DispatchResolve.FindInterfaceMethodImplementationTarget(pInstanceType, pInterfaceType, slot, (DispatchResolve.ResolveFlags)0, null);
	}

	[RuntimeExport("RhResolveStaticDispatchOnType")]
	private unsafe static IntPtr RhResolveStaticDispatchOnType(MethodTable* pInstanceType, MethodTable* pInterfaceType, ushort slot, MethodTable** ppGenericContext)
	{
		return DispatchResolve.FindInterfaceMethodImplementationTarget(pInstanceType, pInterfaceType, slot, DispatchResolve.ResolveFlags.Static, ppGenericContext);
	}

	[RuntimeExport("RhResolveDynamicInterfaceCastableDispatchOnType")]
	private unsafe static IntPtr RhResolveDynamicInterfaceCastableDispatchOnType(MethodTable* pInstanceType, MethodTable* pInterfaceType, ushort slot, MethodTable** ppGenericContext)
	{
		nint num = DispatchResolve.FindInterfaceMethodImplementationTarget(pInstanceType, pInterfaceType, slot, DispatchResolve.ResolveFlags.IDynamicInterfaceCastable, ppGenericContext);
		if ((num & 2) != 0)
		{
			num &= ~(nint)2;
		}
		else
		{
			*ppGenericContext = null;
		}
		return num;
	}

	private unsafe static IntPtr RhResolveDispatchWorker(object pObject, void* cell, ref DispatchCellInfo cellInfo)
	{
		MethodTable* methodTable = pObject.GetMethodTable();
		if (cellInfo.CellType == DispatchCellType.InterfaceAndSlot)
		{
			IntPtr intPtr = DispatchResolve.FindInterfaceMethodImplementationTarget(methodTable, cellInfo.InterfaceType, cellInfo.InterfaceSlot, (DispatchResolve.ResolveFlags)0, null);
			if (intPtr == IntPtr.Zero && methodTable->IsIDynamicInterfaceCastable)
			{
				intPtr = ((delegate*<object, MethodTable*, ushort, IntPtr>)(void*)methodTable->GetClasslibFunction(ClassLibFunctionId.IDynamicCastableGetInterfaceImplementation))(pObject, cellInfo.InterfaceType, cellInfo.InterfaceSlot);
				Debug.Assert(intPtr != IntPtr.Zero);
			}
			return intPtr;
		}
		if (cellInfo.CellType == DispatchCellType.VTableOffset)
		{
			return *(IntPtr*)((byte*)methodTable + cellInfo.VTableOffset);
		}
		EH.FallbackFailFast(RhFailFastReason.InternalError, null);
		return IntPtr.Zero;
	}
}
