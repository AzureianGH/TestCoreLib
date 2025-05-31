#define DEBUG
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Internal.Runtime;

namespace System.Runtime;

internal static class RuntimeExports
{
	[RuntimeExport("RhNewObject")]
	public unsafe static object RhNewObject(MethodTable* pEEType)
	{
		if (pEEType->IsGenericTypeDefinition || pEEType->IsInterface || pEEType->IsArray || pEEType->IsString || pEEType->IsPointer || pEEType->IsFunctionPointer || pEEType->IsByRefLike)
		{
			Debug.Assert(condition: false);
		}
		if (pEEType->IsFinalizable)
		{
			return InternalCalls.RhpNewFinalizable(pEEType);
		}
		return InternalCalls.RhpNewFast(pEEType);
	}

	[RuntimeExport("RhNewArray")]
	public unsafe static object RhNewArray(MethodTable* pEEType, int length)
	{
		Debug.Assert(pEEType->IsArray || pEEType->IsString);
		return InternalCalls.RhpNewArray(pEEType, length);
	}

	public unsafe static object RhBox(MethodTable* pEEType, ref byte data)
	{
		Unsafe.ReadUnaligned<byte>(in data);
		ref byte reference = ref data;
		Debug.Assert(pEEType->IsValueType && !pEEType->IsByRefLike && !pEEType->IsFinalizable);
		if (pEEType->IsNullable)
		{
			if (data == 0)
			{
				return null;
			}
			reference = ref Unsafe.Add(ref data, pEEType->NullableValueOffset);
			pEEType = pEEType->NullableType;
		}
		object obj = InternalCalls.RhpNewFast(pEEType);
		if (pEEType->ContainsGCPointers)
		{
			InternalCalls.RhBulkMoveWithWriteBarrier(ref obj.GetRawData(), ref reference, pEEType->ValueTypeSize);
		}
		else
		{
			Unsafe.CopyBlock(ref obj.GetRawData(), in reference, pEEType->ValueTypeSize);
		}
		return obj;
	}

	[RuntimeExport("RhBoxAny")]
	public unsafe static object RhBoxAny(ref byte data, MethodTable* pEEType)
	{
		if (pEEType->IsValueType)
		{
			return RhBox(pEEType, ref data);
		}
		return Unsafe.As<byte, object>(ref data);
	}

	private unsafe static bool UnboxAnyTypeCompare(MethodTable* pEEType, MethodTable* ptrUnboxToEEType)
	{
		if (pEEType == ptrUnboxToEEType)
		{
			return true;
		}
		if (pEEType->ElementType == ptrUnboxToEEType->ElementType)
		{
			EETypeElementType elementType = ptrUnboxToEEType->ElementType;
			EETypeElementType eETypeElementType = elementType;
			if ((uint)(eETypeElementType - 4) <= 9u)
			{
				return true;
			}
		}
		return false;
	}

	[RuntimeExport("RhUnboxAny")]
	public unsafe static void RhUnboxAny(object o, ref byte data, MethodTable* pUnboxToEEType)
	{
		if (pUnboxToEEType->IsValueType)
		{
			bool flag = false;
			if (!((!pUnboxToEEType->IsNullable) ? (o != null && UnboxAnyTypeCompare(o.GetMethodTable(), pUnboxToEEType)) : (o == null || o.GetMethodTable() == pUnboxToEEType->NullableType)))
			{
				ExceptionIDs id = ((o == null) ? ExceptionIDs.NullReference : ExceptionIDs.InvalidCast);
				throw pUnboxToEEType->GetClasslibException(id);
			}
			RhUnbox(o, ref data, pUnboxToEEType);
		}
		else
		{
			if (o != null && TypeCast.IsInstanceOfAny(pUnboxToEEType, o) == null)
			{
				throw pUnboxToEEType->GetClasslibException(ExceptionIDs.InvalidCast);
			}
			Unsafe.As<byte, object>(ref data) = o;
		}
	}

	public unsafe static ref byte RhUnbox2(MethodTable* pUnboxToEEType, object obj)
	{
		if (obj == null || !UnboxAnyTypeCompare(obj.GetMethodTable(), pUnboxToEEType))
		{
			ExceptionIDs id = ((obj == null) ? ExceptionIDs.NullReference : ExceptionIDs.InvalidCast);
			throw pUnboxToEEType->GetClasslibException(id);
		}
		return ref obj.GetRawData();
	}

	public unsafe static void RhUnboxNullable(ref byte data, MethodTable* pUnboxToEEType, object obj)
	{
		if (obj != null && obj.GetMethodTable() != pUnboxToEEType->NullableType)
		{
			throw pUnboxToEEType->GetClasslibException(ExceptionIDs.InvalidCast);
		}
		RhUnbox(obj, ref data, pUnboxToEEType);
	}

	public unsafe static void RhUnboxTypeTest(MethodTable* pType, MethodTable* pBoxType)
	{
		Debug.Assert(pType->IsValueType);
		if (!UnboxAnyTypeCompare(pType, pBoxType))
		{
			throw pType->GetClasslibException(ExceptionIDs.InvalidCast);
		}
	}

	[RuntimeExport("RhUnbox")]
	public unsafe static void RhUnbox(object obj, ref byte data, MethodTable* pUnboxToEEType)
	{
		if (obj == null)
		{
			Debug.Assert(pUnboxToEEType != null && pUnboxToEEType->IsNullable);
			InternalCalls.RhpGcSafeZeroMemory(ref data, pUnboxToEEType->ValueTypeSize);
			return;
		}
		MethodTable* methodTable = obj.GetMethodTable();
		Debug.Assert(methodTable->IsValueType);
		Debug.Assert(pUnboxToEEType == null || UnboxAnyTypeCompare(methodTable, pUnboxToEEType) || pUnboxToEEType->IsNullable);
		if (pUnboxToEEType != null && pUnboxToEEType->IsNullable)
		{
			Debug.Assert(pUnboxToEEType->NullableType == methodTable);
			Unsafe.As<byte, bool>(ref data) = true;
			data = ref Unsafe.Add(ref data, pUnboxToEEType->NullableValueOffset);
		}
		ref byte rawData = ref obj.GetRawData();
		if (methodTable->ContainsGCPointers)
		{
			InternalCalls.RhBulkMoveWithWriteBarrier(ref data, ref rawData, methodTable->ValueTypeSize);
		}
		else
		{
			Unsafe.CopyBlock(ref data, in rawData, methodTable->ValueTypeSize);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[RuntimeExport("RhGetCurrentThreadStackTrace")]
	public unsafe static int RhGetCurrentThreadStackTrace(IntPtr[] outputBuffer)
	{
		fixed (IntPtr* ptr = outputBuffer)
		{
			IntPtr* pOutputBuffer = ptr;
			return RhpGetCurrentThreadStackTrace(pOutputBuffer, (outputBuffer != null) ? ((uint)outputBuffer.Length) : 0u, new UIntPtr(&pOutputBuffer));
		}
	}

	[DllImport("*")]
	private unsafe static extern int RhpGetCurrentThreadStackTrace(IntPtr* pOutputBuffer, uint outputBufferLength, UIntPtr addressInCurrentFrame);

	[UnmanagedCallersOnly(EntryPoint = "RhpCalculateStackTraceWorker")]
	private unsafe static int RhpCalculateStackTraceWorker(IntPtr* pOutputBuffer, uint outputBufferLength, UIntPtr addressInCurrentFrame)
	{
		uint num = 0u;
		bool flag = true;
		StackFrameIterator stackFrameIterator = default(StackFrameIterator);
		bool condition = stackFrameIterator.Init(null, instructionFault: false, null);
		Debug.Assert(condition, "Missing RhGetCurrentThreadStackTrace frame");
		while (stackFrameIterator.Next())
		{
			if ((void*)stackFrameIterator.SP >= (void*)addressInCurrentFrame)
			{
				if (num < outputBufferLength)
				{
					pOutputBuffer[num] = new IntPtr(stackFrameIterator.ControlPC);
				}
				else
				{
					flag = false;
				}
				num++;
			}
		}
		return (int)(flag ? num : (0 - num));
	}

	[RuntimeExport("RhGetRuntimeHelperForType")]
	internal unsafe static IntPtr RhGetRuntimeHelperForType(MethodTable* pEEType, RuntimeHelperKind kind)
	{
		switch (kind)
		{
		case RuntimeHelperKind.AllocateObject:
			if (pEEType->IsFinalizable)
			{
				return (IntPtr)(delegate*<MethodTable*, object>)(&InternalCalls.RhpNewFinalizable);
			}
			return (IntPtr)(delegate*<MethodTable*, object>)(&InternalCalls.RhpNewFast);
		case RuntimeHelperKind.IsInst:
			if (pEEType->HasGenericVariance || pEEType->IsParameterizedType || pEEType->IsFunctionPointer)
			{
				return (IntPtr)(delegate*<MethodTable*, object, object>)(&TypeCast.IsInstanceOfAny);
			}
			if (pEEType->IsInterface)
			{
				return (IntPtr)(delegate*<MethodTable*, object, object>)(&TypeCast.IsInstanceOfInterface);
			}
			return (IntPtr)(delegate*<MethodTable*, object, object>)(&TypeCast.IsInstanceOfClass);
		case RuntimeHelperKind.CastClass:
			if (pEEType->HasGenericVariance || pEEType->IsParameterizedType || pEEType->IsFunctionPointer)
			{
				return (IntPtr)(delegate*<MethodTable*, object, object>)(&TypeCast.CheckCastAny);
			}
			if (pEEType->IsInterface)
			{
				return (IntPtr)(delegate*<MethodTable*, object, object>)(&TypeCast.CheckCastInterface);
			}
			return (IntPtr)(delegate*<MethodTable*, object, object>)(&TypeCast.CheckCastClass);
		case RuntimeHelperKind.AllocateArray:
			return (IntPtr)(delegate*<MethodTable*, int, object>)(&InternalCalls.RhpNewArray);
		default:
			Debug.Fail("Unknown RuntimeHelperKind");
			return IntPtr.Zero;
		}
	}
}
