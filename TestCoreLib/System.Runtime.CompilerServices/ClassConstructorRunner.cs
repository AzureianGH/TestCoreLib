using System.Threading;

namespace System.Runtime.CompilerServices;

internal static class ClassConstructorRunner
{
	private static object CheckStaticClassConstructionReturnGCStaticBase(ref StaticClassConstructionContext context, object gcStaticBase)
	{
		CheckStaticClassConstruction(ref context);
		return gcStaticBase;
	}

	private static IntPtr CheckStaticClassConstructionReturnNonGCStaticBase(ref StaticClassConstructionContext context, IntPtr nonGcStaticBase)
	{
		CheckStaticClassConstruction(ref context);
		return nonGcStaticBase;
	}

	private unsafe static void CheckStaticClassConstruction(ref StaticClassConstructionContext context)
	{
		while (true)
		{
			IntPtr cctorMethodAddress = context.cctorMethodAddress;
			if (cctorMethodAddress == (IntPtr)0)
			{
				break;
			}
			if (!(cctorMethodAddress == (IntPtr)1) && Interlocked.CompareExchange(ref context.cctorMethodAddress, (IntPtr)1, cctorMethodAddress) == cctorMethodAddress)
			{
				((delegate*<void>)(void*)cctorMethodAddress)();
				context.cctorMethodAddress = (IntPtr)0;
			}
		}
	}
}
