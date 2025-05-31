#define DEBUG
#define FEATURE_GC_STRESS
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Internal.Runtime;

namespace System.Runtime;

internal static class EH
{
	private enum RhEHClauseKind
	{
		RH_EH_CLAUSE_TYPED,
		RH_EH_CLAUSE_FAULT,
		RH_EH_CLAUSE_FILTER,
		RH_EH_CLAUSE_UNUSED
	}

	private struct RhEHClause
	{
		internal RhEHClauseKind _clauseKind;

		internal uint _tryStartOffset;

		internal uint _tryEndOffset;

		internal unsafe byte* _filterAddress;

		internal unsafe byte* _handlerAddress;

		internal unsafe void* _pTargetType;

		public bool ContainsCodeOffset(uint codeOffset)
		{
			return codeOffset >= _tryStartOffset && codeOffset < _tryEndOffset;
		}
	}

	[StructLayout(LayoutKind.Explicit, Size = 32)]
	private struct EHEnum
	{
		[FieldOffset(0)]
		private IntPtr _dummy;
	}

	internal struct MethodRegionInfo
	{
		internal unsafe byte* _hotStartAddress;

		internal nuint _hotSize;

		internal unsafe byte* _coldStartAddress;

		internal nuint _coldSize;
	}

	private enum RhEHFrameType
	{
		RH_EH_FIRST_FRAME = 1,
		RH_EH_FIRST_RETHROW_FRAME
	}

	private enum HwExceptionCode : uint
	{
		STATUS_REDHAWK_NULL_REFERENCE = 0u,
		STATUS_REDHAWK_UNMANAGED_HELPER_NULL_REFERENCE = 66u,
		STATUS_REDHAWK_THREAD_ABORT = 67u,
		STATUS_DATATYPE_MISALIGNMENT = 2147483650u,
		STATUS_ACCESS_VIOLATION = 3221225477u,
		STATUS_IN_PAGE_ERROR = 3221225478u,
		STATUS_ILLEGAL_INSTRUCTION = 3221225501u,
		STATUS_INTEGER_DIVIDE_BY_ZERO = 3221225620u,
		STATUS_INTEGER_OVERFLOW = 3221225621u,
		STATUS_PRIVILEGED_INSTRUCTION = 3221225622u
	}

	[StructLayout(LayoutKind.Explicit, Size = 80)]
	public struct PAL_LIMITED_CONTEXT
	{
		[FieldOffset(0)]
		internal IntPtr IP;
	}

	[Flags]
	internal enum ExKind : byte
	{
		None = 0,
		Throw = 1,
		HardwareFault = 2,
		KindMask = 3,
		RethrowFlag = 4,
		SupersededFlag = 8,
		InstructionFaultFlag = 0x10
	}

	[StructLayout(LayoutKind.Explicit)]
	public ref struct ExInfo
	{
		[FieldOffset(0)]
		internal unsafe void* _pPrevExInfo;

		[FieldOffset(8)]
		internal unsafe PAL_LIMITED_CONTEXT* _pExContext;

		[FieldOffset(16)]
		private object _exception;

		[FieldOffset(24)]
		internal ExKind _kind;

		[FieldOffset(25)]
		internal byte _passNumber;

		[FieldOffset(28)]
		internal uint _idxCurClause;

		[FieldOffset(32)]
		internal StackFrameIterator _frameIter;

		[FieldOffset(392)]
		internal volatile UIntPtr _notifyDebuggerSP;

		internal object ThrownException => _exception;

		internal void Init(object exceptionObj, bool instructionFault = false)
		{
			_exception = exceptionObj;
			if (instructionFault)
			{
				_kind |= ExKind.InstructionFaultFlag;
			}
			_notifyDebuggerSP = UIntPtr.Zero;
		}

		internal void Init(object exceptionObj, ref ExInfo rethrownExInfo)
		{
			_exception = exceptionObj;
			_kind = rethrownExInfo._kind | ExKind.RethrowFlag;
			_notifyDebuggerSP = UIntPtr.Zero;
		}
	}

	private const uint MaxTryRegionIdx = uint.MaxValue;

	internal unsafe static UIntPtr MaxSP => (UIntPtr)(void*)(-1);

	internal static void FallbackFailFast(RhFailFastReason reason, object unhandledException)
	{
		InternalCalls.RhpFallbackFailFast();
	}

	internal unsafe static void FailFastViaClasslib(RhFailFastReason reason, object unhandledException, IntPtr classlibAddress)
	{
		IntPtr intPtr = (IntPtr)InternalCalls.RhpGetClasslibFunctionFromCodeAddress(classlibAddress, ClassLibFunctionId.FailFast);
		if (intPtr == IntPtr.Zero)
		{
			FallbackFailFast(reason, unhandledException);
		}
		try
		{
			((delegate*<RhFailFastReason, object, IntPtr, IntPtr, void>)(void*)intPtr)(reason, unhandledException, IntPtr.Zero, IntPtr.Zero);
		}
		catch when (1 != 0)
		{
		}
		FallbackFailFast(reason, unhandledException);
	}

	private unsafe static void OnFirstChanceExceptionViaClassLib(object exception)
	{
		IntPtr intPtr = (IntPtr)InternalCalls.RhpGetClasslibFunctionFromEEType(exception.GetMethodTable(), ClassLibFunctionId.OnFirstChance);
		if (intPtr == IntPtr.Zero)
		{
			return;
		}
		try
		{
			((delegate*<object, void>)(void*)intPtr)(exception);
		}
		catch when (1 != 0)
		{
		}
	}

	private unsafe static void OnUnhandledExceptionViaClassLib(object exception)
	{
		IntPtr intPtr = (IntPtr)InternalCalls.RhpGetClasslibFunctionFromEEType(exception.GetMethodTable(), ClassLibFunctionId.OnUnhandledException);
		if (intPtr == IntPtr.Zero)
		{
			return;
		}
		try
		{
			((delegate*<object, void>)(void*)intPtr)(exception);
		}
		catch when (1 != 0)
		{
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal unsafe static void UnhandledExceptionFailFastViaClasslib(RhFailFastReason reason, object unhandledException, IntPtr classlibAddress, ref ExInfo exInfo)
	{
		IntPtr intPtr = (IntPtr)InternalCalls.RhpGetClasslibFunctionFromCodeAddress(classlibAddress, ClassLibFunctionId.FailFast);
		if (intPtr == IntPtr.Zero)
		{
			FailFastViaClasslib(reason, unhandledException, classlibAddress);
		}
		void* ptr = null;
		try
		{
			((delegate*<RhFailFastReason, object, IntPtr, void*, void>)(void*)intPtr)(reason, unhandledException, exInfo._pExContext->IP, ptr);
		}
		catch when (1 != 0)
		{
		}
		FallbackFailFast(reason, unhandledException);
	}

	private unsafe static void AppendExceptionStackFrameViaClasslib(object exception, IntPtr ip, UIntPtr sp, ref ExInfo exInfo, ref bool isFirstRethrowFrame, ref bool isFirstFrame)
	{
		int num = (isFirstFrame ? 1 : 0) | (isFirstRethrowFrame ? 2 : 0);
		IntPtr intPtr = (IntPtr)InternalCalls.RhpGetClasslibFunctionFromCodeAddress(ip, ClassLibFunctionId.AppendExceptionStackFrame);
		if (intPtr != IntPtr.Zero)
		{
			try
			{
				((delegate*<object, IntPtr, int, void>)(void*)intPtr)(exception, ip, num);
			}
			catch when (1 != 0)
			{
			}
			isFirstRethrowFrame = false;
			isFirstFrame = false;
		}
	}

	internal unsafe static Exception GetClasslibException(ExceptionIDs id, IntPtr address)
	{
		IntPtr intPtr = (IntPtr)InternalCalls.RhpGetClasslibFunctionFromCodeAddress(address, ClassLibFunctionId.GetRuntimeException);
		Exception ex = null;
		try
		{
			ex = ((delegate*<ExceptionIDs, Exception>)(void*)intPtr)(id);
		}
		catch when (1 != 0)
		{
		}
		if (ex == null)
		{
			FailFastViaClasslib(RhFailFastReason.InternalError, null, address);
		}
		return ex;
	}

	internal unsafe static Exception GetClasslibExceptionFromEEType(ExceptionIDs id, MethodTable* pEEType)
	{
		IntPtr intPtr = IntPtr.Zero;
		if (pEEType != null)
		{
			intPtr = (IntPtr)InternalCalls.RhpGetClasslibFunctionFromEEType(pEEType, ClassLibFunctionId.GetRuntimeException);
		}
		Exception ex = null;
		try
		{
			ex = ((delegate*<ExceptionIDs, Exception>)(void*)intPtr)(id);
		}
		catch when (1 != 0)
		{
		}
		if (ex == null)
		{
			FailFastViaClasslib(RhFailFastReason.InternalError, null, (IntPtr)pEEType);
		}
		return ex;
	}

	[StackTraceHidden]
	[RuntimeExport("RhExceptionHandling_ThrowClasslibOverflowException")]
	public static void ThrowClasslibOverflowException(IntPtr address)
	{
		throw GetClasslibException(ExceptionIDs.Overflow, address);
	}

	[StackTraceHidden]
	[RuntimeExport("RhExceptionHandling_ThrowClasslibDivideByZeroException")]
	public static void ThrowClasslibDivideByZeroException(IntPtr address)
	{
		throw GetClasslibException(ExceptionIDs.DivideByZero, address);
	}

	[StackTraceHidden]
	[RuntimeExport("RhExceptionHandling_FailedAllocation")]
	public unsafe static void FailedAllocation(MethodTable* pEEType, bool fIsOverflow)
	{
		ExceptionIDs id = ((!fIsOverflow) ? ExceptionIDs.OutOfMemory : ExceptionIDs.Overflow);
		throw pEEType->GetClasslibException(id);
	}

	[RuntimeExport("RhThrowHwEx")]
	[StackTraceHidden]
	public unsafe static void RhThrowHwEx(uint exceptionCode, ref ExInfo exInfo)
	{
		GCStress.TriggerGC();
		InternalCalls.RhpValidateExInfoStack();
		IntPtr iP = exInfo._pExContext->IP;
		bool instructionFault = true;
		ExceptionIDs exceptionIDs = (ExceptionIDs)0;
		Exception exceptionObj = null;
		switch (exceptionCode)
		{
		case 0u:
			exceptionIDs = ExceptionIDs.NullReference;
			break;
		case 66u:
			instructionFault = false;
			exceptionIDs = ExceptionIDs.NullReference;
			break;
		case 67u:
			exceptionObj = InternalCalls.RhpGetThreadAbortException();
			break;
		case 2147483650u:
			exceptionIDs = ExceptionIDs.DataMisaligned;
			break;
		case 3221225477u:
			exceptionIDs = ExceptionIDs.AccessViolation;
			break;
		case 3221225620u:
			exceptionIDs = ExceptionIDs.DivideByZero;
			break;
		case 3221225621u:
			exceptionIDs = ExceptionIDs.Overflow;
			break;
		case 3221225501u:
			exceptionIDs = ExceptionIDs.IllegalInstruction;
			break;
		case 3221225478u:
			exceptionIDs = ExceptionIDs.InPageError;
			break;
		case 3221225622u:
			exceptionIDs = ExceptionIDs.PrivilegedInstruction;
			break;
		default:
			FailFastViaClasslib(RhFailFastReason.InternalError, null, iP);
			break;
		}
		if (exceptionIDs != 0)
		{
			exceptionObj = GetClasslibException(exceptionIDs, iP);
		}
		exInfo.Init(exceptionObj, instructionFault);
		DispatchEx(ref exInfo._frameIter, ref exInfo);
		FallbackFailFast(RhFailFastReason.InternalError, null);
	}

	[RuntimeExport("RhThrowEx")]
	[StackTraceHidden]
	public unsafe static void RhThrowEx(object exceptionObj, ref ExInfo exInfo)
	{
		GCStress.TriggerGC();
		InternalCalls.RhpValidateExInfoStack();
		if (exceptionObj == null)
		{
			IntPtr iP = exInfo._pExContext->IP;
			exceptionObj = GetClasslibException(ExceptionIDs.NullReference, iP);
		}
		exInfo.Init(exceptionObj);
		DispatchEx(ref exInfo._frameIter, ref exInfo);
		FallbackFailFast(RhFailFastReason.InternalError, null);
	}

	[RuntimeExport("RhRethrow")]
	[StackTraceHidden]
	public static void RhRethrow(ref ExInfo activeExInfo, ref ExInfo exInfo)
	{
		GCStress.TriggerGC();
		InternalCalls.RhpValidateExInfoStack();
		object thrownException = activeExInfo.ThrownException;
		exInfo.Init(thrownException, ref activeExInfo);
		DispatchEx(ref exInfo._frameIter, ref exInfo);
		FallbackFailFast(RhFailFastReason.InternalError, null);
	}

	[StackTraceHidden]
	private unsafe static void DispatchEx(scoped ref StackFrameIterator frameIter, ref ExInfo exInfo)
	{
		Debug.Assert(exInfo._passNumber == 1, "expected asm throw routine to set the pass");
		object thrownException = exInfo.ThrownException;
		UIntPtr uIntPtr = MaxSP;
		byte* ptr = null;
		uint tryRegionIdx = uint.MaxValue;
		bool isFirstRethrowFrame = (exInfo._kind & ExKind.RethrowFlag) != 0;
		bool isFirstFrame = true;
		bool flag = false;
		byte* ptr2 = null;
		byte* ptr3 = null;
		UIntPtr prevFramePtr = UIntPtr.Zero;
		bool flag2 = false;
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		bool flag3 = frameIter.Init(exInfo._pExContext, (exInfo._kind & ExKind.InstructionFaultFlag) != 0, &flag);
		Debug.Assert(flag3, "RhThrowEx called with an unexpected context");
		OnFirstChanceExceptionViaClassLib(thrownException);
		uint idxStart = uint.MaxValue;
		while (flag3 && !flag2 && !flag)
		{
			ptr2 = frameIter.ControlPC;
			ptr3 = frameIter.OriginalControlPC;
			DebugScanCallFrame(exInfo._passNumber, frameIter.ControlPC, frameIter.SP);
			UpdateStackTrace(thrownException, exInfo._frameIter.FramePointer, (IntPtr)frameIter.OriginalControlPC, frameIter.SP, ref isFirstRethrowFrame, ref prevFramePtr, ref isFirstFrame, ref exInfo);
			if (FindFirstPassHandler(thrownException, idxStart, ref frameIter, out tryRegionIdx, out var pHandler))
			{
				uIntPtr = frameIter.SP;
				ptr = pHandler;
				DebugVerifyHandlingFrame(uIntPtr);
				break;
			}
			flag3 = frameIter.Next(&idxStart, &flag2, &flag);
		}
		if (flag2)
		{
		}
		if (ptr == null && zero == IntPtr.Zero && !flag)
		{
			OnUnhandledExceptionViaClassLib(thrownException);
			UnhandledExceptionFailFastViaClasslib(RhFailFastReason.UnhandledException, thrownException, (IntPtr)ptr3, ref exInfo);
		}
		Debug.Assert(ptr != null || zero != IntPtr.Zero || flag2 || flag, "We should have a handler if we're starting the second pass");
		Debug.Assert(!flag || ptr == null, "No catch handler should be returned for intercepted exceptions in the first pass");
		InternalCalls.RhpSetThreadDoNotTriggerGC();
		exInfo._passNumber = 2;
		exInfo._idxCurClause = tryRegionIdx;
		idxStart = uint.MaxValue;
		flag2 = false;
		flag = false;
		for (flag3 = frameIter.Init(exInfo._pExContext, (exInfo._kind & ExKind.InstructionFaultFlag) != 0, &flag); flag3 && (void*)frameIter.SP <= (void*)uIntPtr; flag3 = frameIter.Next(&idxStart, &flag2, &flag))
		{
			Debug.Assert(flag3, "second-pass EH unwind failed unexpectedly");
			DebugScanCallFrame(exInfo._passNumber, frameIter.ControlPC, frameIter.SP);
			if (flag)
			{
				ptr = null;
				break;
			}
			if (flag2)
			{
				Debug.Assert(zero != IntPtr.Zero, "Unwound to a reverse P/Invoke in the second pass. We should have a propagation handler.");
				Debug.Assert(frameIter.PreviousTransitionFrame != IntPtr.Zero, "Should have a transition frame for reverse P/Invoke.");
				Debug.Assert(frameIter.SP == uIntPtr, "Encountered a different reverse P/Invoke frame in the second pass.");
				break;
			}
			if (frameIter.SP == uIntPtr)
			{
				InvokeSecondPass(ref exInfo, idxStart, tryRegionIdx);
				break;
			}
			InvokeSecondPass(ref exInfo, idxStart);
		}
		exInfo._idxCurClause = tryRegionIdx;
		InternalCalls.RhpCallCatchFunclet(thrownException, ptr, frameIter.RegisterSet, ref exInfo);
		Debug.Fail("unreachable");
		FallbackFailFast(RhFailFastReason.InternalError, null);
	}

	[Conditional("DEBUG")]
	private unsafe static void DebugScanCallFrame(int passNumber, byte* ip, UIntPtr sp)
	{
		Debug.Assert(ip != null, "IP address must not be null");
	}

	[Conditional("DEBUG")]
	private unsafe static void DebugVerifyHandlingFrame(UIntPtr handlingFrameSP)
	{
		Debug.Assert(handlingFrameSP != MaxSP, "Handling frame must have an SP value");
		Debug.Assert((void*)handlingFrameSP > &handlingFrameSP, "Handling frame must have a valid stack frame pointer");
	}

	private unsafe static uint CalculateCodeOffset(byte* pbControlPC, in MethodRegionInfo methodRegionInfo)
	{
		uint num = (uint)(pbControlPC - methodRegionInfo._hotStartAddress);
		if (methodRegionInfo._coldSize != 0 && num >= methodRegionInfo._hotSize)
		{
			num = (uint)(methodRegionInfo._hotSize + (nuint)(nint)(pbControlPC - methodRegionInfo._coldStartAddress));
		}
		return num;
	}

	private static void UpdateStackTrace(object exceptionObj, UIntPtr curFramePtr, IntPtr ip, UIntPtr sp, ref bool isFirstRethrowFrame, ref UIntPtr prevFramePtr, ref bool isFirstFrame, ref ExInfo exInfo)
	{
		if (prevFramePtr == UIntPtr.Zero || curFramePtr != prevFramePtr)
		{
			AppendExceptionStackFrameViaClasslib(exceptionObj, ip, sp, ref exInfo, ref isFirstRethrowFrame, ref isFirstFrame);
		}
		prevFramePtr = curFramePtr;
	}

	[StackTraceHidden]
	private unsafe static bool FindFirstPassHandler(object exception, uint idxStart, ref StackFrameIterator frameIter, out uint tryRegionIdx, out byte* pHandler)
	{
		pHandler = null;
		tryRegionIdx = uint.MaxValue;
		EHEnum eHEnum = default(EHEnum);
		if (!InternalCalls.RhpEHEnumInitFromStackFrameIterator(ref frameIter, out var pMethodRegionInfo, &eHEnum))
		{
			return false;
		}
		byte* controlPC = frameIter.ControlPC;
		uint codeOffset = CalculateCodeOffset(controlPC, in pMethodRegionInfo);
		uint num = 0u;
		uint num2 = 0u;
		RhEHClause rhEHClause = default(RhEHClause);
		for (uint num3 = 0u; InternalCalls.RhpEHEnumNext(&eHEnum, &rhEHClause); num3++)
		{
			if (idxStart != uint.MaxValue)
			{
				if (num3 <= idxStart)
				{
					num = rhEHClause._tryStartOffset;
					num2 = rhEHClause._tryEndOffset;
					continue;
				}
				if (rhEHClause._tryStartOffset == num && rhEHClause._tryEndOffset == num2)
				{
					continue;
				}
				idxStart = uint.MaxValue;
			}
			RhEHClauseKind clauseKind = rhEHClause._clauseKind;
			if ((clauseKind != RhEHClauseKind.RH_EH_CLAUSE_TYPED && clauseKind != RhEHClauseKind.RH_EH_CLAUSE_FILTER) || !rhEHClause.ContainsCodeOffset(codeOffset))
			{
				continue;
			}
			if (clauseKind == RhEHClauseKind.RH_EH_CLAUSE_TYPED)
			{
				if (!ShouldTypedClauseCatchThisException(exception, (MethodTable*)rhEHClause._pTargetType, !frameIter.IsRuntimeWrappedExceptions))
				{
					continue;
				}
				pHandler = rhEHClause._handlerAddress;
				tryRegionIdx = num3;
				return true;
			}
			byte* filterAddress = rhEHClause._filterAddress;
			bool flag = false;
			try
			{
				flag = InternalCalls.RhpCallFilterFunclet(exception, filterAddress, frameIter.RegisterSet);
			}
			catch when (1 != 0)
			{
			}
			if (!flag)
			{
				continue;
			}
			pHandler = rhEHClause._handlerAddress;
			tryRegionIdx = num3;
			return true;
		}
		return false;
	}

	private unsafe static bool ShouldTypedClauseCatchThisException(object exception, MethodTable* pClauseType, bool tryUnwrapException)
	{
		return TypeCast.IsInstanceOfException(pClauseType, exception);
	}

	private static void InvokeSecondPass(ref ExInfo exInfo, uint idxStart)
	{
		InvokeSecondPass(ref exInfo, idxStart, uint.MaxValue);
	}

	private unsafe static void InvokeSecondPass(ref ExInfo exInfo, uint idxStart, uint idxLimit)
	{
		EHEnum eHEnum = default(EHEnum);
		if (!InternalCalls.RhpEHEnumInitFromStackFrameIterator(ref exInfo._frameIter, out var pMethodRegionInfo, &eHEnum))
		{
			return;
		}
		byte* controlPC = exInfo._frameIter.ControlPC;
		uint codeOffset = CalculateCodeOffset(controlPC, in pMethodRegionInfo);
		uint num = 0u;
		uint num2 = 0u;
		RhEHClause rhEHClause = default(RhEHClause);
		for (uint num3 = 0u; InternalCalls.RhpEHEnumNext(&eHEnum, &rhEHClause) && num3 < idxLimit; num3++)
		{
			if (idxStart != uint.MaxValue)
			{
				if (num3 <= idxStart)
				{
					num = rhEHClause._tryStartOffset;
					num2 = rhEHClause._tryEndOffset;
					continue;
				}
				if (rhEHClause._tryStartOffset == num && rhEHClause._tryEndOffset == num2)
				{
					continue;
				}
				idxStart = uint.MaxValue;
			}
			RhEHClauseKind clauseKind = rhEHClause._clauseKind;
			if (clauseKind == RhEHClauseKind.RH_EH_CLAUSE_FAULT && rhEHClause.ContainsCodeOffset(codeOffset))
			{
				byte* handlerAddress = rhEHClause._handlerAddress;
				exInfo._idxCurClause = num3;
				InternalCalls.RhpCallFinallyFunclet(handlerAddress, exInfo._frameIter.RegisterSet);
				exInfo._idxCurClause = uint.MaxValue;
			}
		}
	}

	[UnmanagedCallersOnly(EntryPoint = "RhpFailFastForPInvokeExceptionPreemp")]
	public unsafe static void RhpFailFastForPInvokeExceptionPreemp(IntPtr PInvokeCallsiteReturnAddr, void* pExceptionRecord, void* pContextRecord)
	{
		FailFastViaClasslib(RhFailFastReason.UnhandledExceptionFromPInvoke, null, PInvokeCallsiteReturnAddr);
	}

	[RuntimeExport("RhpFailFastForPInvokeExceptionCoop")]
	public unsafe static void RhpFailFastForPInvokeExceptionCoop(IntPtr classlibBreadcrumb, void* pExceptionRecord, void* pContextRecord)
	{
		FailFastViaClasslib(RhFailFastReason.UnhandledExceptionFromPInvoke, null, classlibBreadcrumb);
	}
}
