using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace System.Runtime;

internal static class __Finalizer
{
	[UnmanagedCallersOnly(EntryPoint = "ProcessFinalizers")]
	public static void ProcessFinalizers()
	{
		while (true)
		{
			if (InternalCalls.RhpWaitForFinalizerRequest() != 0)
			{
				int observedFullGcCount = RuntimeImports.RhGetGcCollectionCount(RuntimeImports.RhGetMaxGcGeneration(), getSpecialGCCount: false);
				uint fCount = DrainQueue();
				InternalCalls.RhpSignalFinalizationComplete(fCount, observedFullGcCount);
			}
			else
			{
				InternalCalls.RhCollect(0, InternalGCCollectionMode.Blocking, lowMemoryP: true);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private unsafe static uint DrainQueue()
	{
		uint num = 0u;
		while (true)
		{
			object obj = InternalCalls.RhpGetNextFinalizableObject();
			if (obj == null)
			{
				break;
			}
			num++;
			try
			{
				((delegate*<object, void>)(void*)obj.GetMethodTable()->FinalizerCode)(obj);
			}
			catch (Exception ex) when (ExceptionHandling.IsHandledByGlobalHandler(ex))
			{
			}
		}
		return num;
	}
}
