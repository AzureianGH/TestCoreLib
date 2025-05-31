using System.Runtime.InteropServices;

namespace System.Runtime;

[StructLayout(LayoutKind.Explicit, Size = 360)]
internal struct StackFrameIterator
{
	[FieldOffset(16)]
	private UIntPtr _framePointer;

	[FieldOffset(24)]
	private IntPtr _controlPC;

	[FieldOffset(32)]
	private REGDISPLAY _regDisplay;

	[FieldOffset(344)]
	private IntPtr _originalControlPC;

	[FieldOffset(352)]
	private IntPtr _pPreviousTransitionFrame;

	internal unsafe byte* ControlPC => (byte*)(void*)_controlPC;

	internal unsafe byte* OriginalControlPC => (byte*)(void*)_originalControlPC;

	internal unsafe void* RegisterSet
	{
		get
		{
			fixed (REGDISPLAY* regDisplay = &_regDisplay)
			{
				return regDisplay;
			}
		}
	}

	internal UIntPtr SP => _regDisplay.SP;

	internal UIntPtr FramePointer => _framePointer;

	internal IntPtr PreviousTransitionFrame => _pPreviousTransitionFrame;

	internal bool IsRuntimeWrappedExceptions => false;

	internal unsafe bool Init(EH.PAL_LIMITED_CONTEXT* pStackwalkCtx, bool instructionFault = false, bool* fIsExceptionIntercepted = null)
	{
		return InternalCalls.RhpSfiInit(ref this, pStackwalkCtx, instructionFault, fIsExceptionIntercepted);
	}

	internal unsafe bool Next()
	{
		return Next(null, null, null);
	}

	internal unsafe bool Next(uint* uExCollideClauseIdx, bool* fIsExceptionIntercepted)
	{
		return Next(uExCollideClauseIdx, null, fIsExceptionIntercepted);
	}

	internal unsafe bool Next(uint* uExCollideClauseIdx, bool* fUnwoundReversePInvoke, bool* fIsExceptionIntercepted)
	{
		return InternalCalls.RhpSfiNext(ref this, uExCollideClauseIdx, fUnwoundReversePInvoke, fIsExceptionIntercepted);
	}
}
