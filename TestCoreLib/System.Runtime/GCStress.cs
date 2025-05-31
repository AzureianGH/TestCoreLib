using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Runtime;

internal class GCStress
{
	private static bool _003CInitialized_003Ek__BackingField;

	private static GCStress Head;

	private static GCStress Tail;

	private GCStress Next;

	internal static bool Initialized
	{
		get
		{
			return _003CInitialized_003Ek__BackingField;
		}
		private set
		{
			_003CInitialized_003Ek__BackingField = value;
		}
	}

	[UnmanagedCallersOnly(EntryPoint = "RhGcStress_Initialize", CallConvs = new Type[] { typeof(CallConvCdecl) })]
	public static void Initialize()
	{
		if (!Initialized)
		{
			Initialized = true;
			Head = new GCStress();
			Tail = Head;
			int num = 10;
			for (int i = 0; i < num; i++)
			{
				Tail.Next = new GCStress();
				Tail = Tail.Next;
			}
			Head = Head.Next;
			InternalCalls.RhpInitializeGcStress();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Conditional("FEATURE_GC_STRESS")]
	internal static void TriggerGC()
	{
		if (Initialized)
		{
			InternalCalls.RhCollect(-1, InternalGCCollectionMode.Blocking);
		}
	}

	~GCStress()
	{
		Head = Head.Next;
		Tail.Next = new GCStress();
		Tail = Tail.Next;
	}

	private GCStress()
	{
	}
}
