using System;

namespace Internal.Runtime;

public struct TypeManagerHandle
{
	private struct TypeManager
	{
		public IntPtr OsHandle;

		public IntPtr ReadyToRunHeader;
	}

	private unsafe TypeManager* _handleValue;

	public unsafe bool IsNull => _handleValue == null;

	public unsafe IntPtr OsModuleBase => _handleValue->OsHandle;

	public unsafe TypeManagerHandle(IntPtr handleValue)
	{
		_handleValue = (TypeManager*)(void*)handleValue;
	}

	public unsafe IntPtr GetIntPtrUNSAFE()
	{
		return (IntPtr)_handleValue;
	}
}
