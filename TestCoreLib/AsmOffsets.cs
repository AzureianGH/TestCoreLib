internal static class AsmOffsets
{
	internal const int CLUMP_SIZE = 2048;

	internal const int LOG2_CLUMP_SIZE = 11;

	internal const int OFFSETOF__Object__m_pEEType = 0;

	internal const int OFFSETOF__Array__m_Length = 8;

	internal const int OFFSETOF__String__m_Length = 8;

	internal const int OFFSETOF__String__m_FirstChar = 12;

	internal const int STRING_COMPONENT_SIZE = 2;

	internal const int STRING_BASE_SIZE = 22;

	internal const int MAX_STRING_LENGTH = 1073741791;

	internal const int OFFSETOF__MethodTable__m_usComponentSize = 0;

	internal const int OFFSETOF__MethodTable__m_uFlags = 0;

	internal const int OFFSETOF__MethodTable__m_uBaseSize = 4;

	internal const int OFFSETOF__MethodTable__m_VTable = 24;

	internal const int OFFSETOF__Thread__m_eeAllocContext = 0;

	internal const int OFFSETOF__Thread__m_ThreadStateFlags = 64;

	internal const int OFFSETOF__Thread__m_pTransitionFrame = 72;

	internal const int OFFSETOF__Thread__m_pDeferredTransitionFrame = 80;

	internal const int OFFSETOF__Thread__m_ppvHijackedReturnAddressLocation = 104;

	internal const int OFFSETOF__Thread__m_pvHijackedReturnAddress = 112;

	internal const int OFFSETOF__Thread__m_pExInfoStackHead = 120;

	internal const int OFFSETOF__Thread__m_threadAbortException = 128;

	internal const int SIZEOF__EHEnum = 32;

	internal const int OFFSETOF__gc_alloc_context__alloc_ptr = 0;

	internal const int OFFSETOF__gc_alloc_context__alloc_limit = 8;

	internal const int OFFSETOF__ee_alloc_context__combined_limit = 0;

	internal const int OFFSETOF__ee_alloc_context__m_rgbAllocContextBuffer = 8;

	internal const int OFFSETOF__InterfaceDispatchCell__m_pCache = 8;

	internal const int OFFSETOF__InterfaceDispatchCache__m_rgEntries = 32;

	internal const int SIZEOF__InterfaceDispatchCacheEntry = 16;

	internal const int SIZEOF__ExInfo = 400;

	internal const int OFFSETOF__ExInfo__m_pPrevExInfo = 0;

	internal const int OFFSETOF__ExInfo__m_pExContext = 8;

	internal const int OFFSETOF__ExInfo__m_exception = 16;

	internal const int OFFSETOF__ExInfo__m_kind = 24;

	internal const int OFFSETOF__ExInfo__m_passNumber = 25;

	internal const int OFFSETOF__ExInfo__m_idxCurClause = 28;

	internal const int OFFSETOF__ExInfo__m_frameIter = 32;

	internal const int OFFSETOF__ExInfo__m_notifyDebuggerSP = 392;

	internal const int OFFSETOF__PInvokeTransitionFrame__m_RIP = 0;

	internal const int OFFSETOF__PInvokeTransitionFrame__m_FramePointer = 8;

	internal const int OFFSETOF__PInvokeTransitionFrame__m_pThread = 16;

	internal const int OFFSETOF__PInvokeTransitionFrame__m_Flags = 24;

	internal const int OFFSETOF__PInvokeTransitionFrame__m_PreservedRegs = 32;

	internal const int SIZEOF__StackFrameIterator = 360;

	internal const int OFFSETOF__StackFrameIterator__m_FramePointer = 16;

	internal const int OFFSETOF__StackFrameIterator__m_ControlPC = 24;

	internal const int OFFSETOF__StackFrameIterator__m_RegDisplay = 32;

	internal const int OFFSETOF__StackFrameIterator__m_OriginalControlPC = 344;

	internal const int OFFSETOF__StackFrameIterator__m_pPreviousTransitionFrame = 352;

	internal const int SIZEOF__PAL_LIMITED_CONTEXT = 80;

	internal const int OFFSETOF__PAL_LIMITED_CONTEXT__IP = 0;

	internal const int OFFSETOF__PAL_LIMITED_CONTEXT__Rsp = 8;

	internal const int OFFSETOF__PAL_LIMITED_CONTEXT__Rbp = 16;

	internal const int OFFSETOF__PAL_LIMITED_CONTEXT__Rax = 24;

	internal const int OFFSETOF__PAL_LIMITED_CONTEXT__Rbx = 32;

	internal const int OFFSETOF__PAL_LIMITED_CONTEXT__Rdx = 40;

	internal const int OFFSETOF__PAL_LIMITED_CONTEXT__R12 = 48;

	internal const int OFFSETOF__PAL_LIMITED_CONTEXT__R13 = 56;

	internal const int OFFSETOF__PAL_LIMITED_CONTEXT__R14 = 64;

	internal const int OFFSETOF__PAL_LIMITED_CONTEXT__R15 = 72;

	internal const int SIZEOF__REGDISPLAY = 136;

	internal const int OFFSETOF__REGDISPLAY__SP = 120;

	internal const int OFFSETOF__REGDISPLAY__pRbx = 24;

	internal const int OFFSETOF__REGDISPLAY__pRbp = 32;

	internal const int OFFSETOF__REGDISPLAY__pRsi = 40;

	internal const int OFFSETOF__REGDISPLAY__pRdi = 48;

	internal const int OFFSETOF__REGDISPLAY__pR12 = 88;

	internal const int OFFSETOF__REGDISPLAY__pR13 = 96;

	internal const int OFFSETOF__REGDISPLAY__pR14 = 104;

	internal const int OFFSETOF__REGDISPLAY__pR15 = 112;
}
