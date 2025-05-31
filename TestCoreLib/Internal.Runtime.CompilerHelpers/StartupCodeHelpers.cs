#define DEBUG
using System;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Internal.Runtime.CompilerHelpers;

internal static class StartupCodeHelpers
{
	private static TypeManagerHandle[] s_modules;

	private static int s_moduleCount;

	private static IntPtr s_moduleGCStaticsSpines;

	[UnmanagedCallersOnly(EntryPoint = "InitializeModules")]
	internal unsafe static void InitializeModules(IntPtr osModule, IntPtr* pModuleHeaders, int count, IntPtr* pClasslibFunctions, int nClasslibFunctions)
	{
		RuntimeImports.RhpRegisterOsModule(osModule);
		TypeManagerHandle[] array = CreateTypeManagers(osModule, pModuleHeaders, count, pClasslibFunctions, nClasslibFunctions);
		object[] array2 = new object[count];
		for (int i = 0; i < array.Length; i++)
		{
			InitializeGlobalTablesForModule(array[i], i, array2);
		}
		s_moduleGCStaticsSpines = RuntimeImports.RhHandleAlloc(array2, GCHandleType.Normal);
		s_modules = array;
		s_moduleCount = array.Length;
		for (int j = 0; j < array.Length; j++)
		{
			RunInitializers(array[j], ReadyToRunSectionType.EagerCctor);
		}
	}

	internal static int GetLoadedModules(TypeManagerHandle[] outputModules)
	{
		if (outputModules != null)
		{
			int num = ((s_moduleCount < outputModules.Length) ? s_moduleCount : outputModules.Length);
			for (int i = 0; i < num; i++)
			{
				outputModules[i] = s_modules[i];
			}
		}
		return s_moduleCount;
	}

	private unsafe static TypeManagerHandle[] CreateTypeManagers(IntPtr osModule, IntPtr* pModuleHeaders, int count, IntPtr* pClasslibFunctions, int nClasslibFunctions)
	{
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			if (pModuleHeaders[i] != IntPtr.Zero)
			{
				num++;
			}
		}
		TypeManagerHandle* ptr = stackalloc TypeManagerHandle[num];
		int num2 = 0;
		for (int j = 0; j < count; j++)
		{
			if (pModuleHeaders[j] != IntPtr.Zero)
			{
				TypeManagerHandle typeManagerHandle = RuntimeImports.RhpCreateTypeManager(osModule, pModuleHeaders[j], pClasslibFunctions, nClasslibFunctions);
				int length;
				IntPtr intPtr = RuntimeImports.RhGetModuleSection(typeManagerHandle, ReadyToRunSectionType.DehydratedData, out length);
				if (intPtr != IntPtr.Zero)
				{
					RehydrateData(intPtr, length);
				}
				ptr[num2++] = typeManagerHandle;
			}
		}
		TypeManagerHandle[] array = new TypeManagerHandle[num];
		for (int k = 0; k < num; k++)
		{
			array[k] = ptr[k];
		}
		return array;
	}

	private unsafe static void InitializeGlobalTablesForModule(TypeManagerHandle typeManager, int moduleIndex, object[] gcStaticBaseSpines)
	{
		int length;
		TypeManagerSlot* ptr = (TypeManagerSlot*)(void*)RuntimeImports.RhGetModuleSection(typeManager, ReadyToRunSectionType.TypeManagerIndirection, out length);
		ptr->TypeManager = typeManager;
		ptr->ModuleIndex = moduleIndex;
		IntPtr intPtr = RuntimeImports.RhGetModuleSection(typeManager, ReadyToRunSectionType.GCStaticRegion, out length);
		if (intPtr != IntPtr.Zero)
		{
			StartupDebug.Assert(length % (MethodTable.SupportsRelativePointers ? 4 : sizeof(IntPtr)) == 0);
			object[] array = InitializeStatics(intPtr, length);
			StartupDebug.Assert((uint)moduleIndex < (uint)gcStaticBaseSpines.Length);
			Unsafe.Add(ref Unsafe.As<byte, object>(ref Unsafe.As<RawArrayData>(gcStaticBaseSpines).Data), moduleIndex) = array;
		}
		IntPtr intPtr2 = RuntimeImports.RhGetModuleSection(typeManager, ReadyToRunSectionType.FrozenObjectRegion, out length);
		if (intPtr2 != IntPtr.Zero)
		{
			StartupDebug.Assert(length % IntPtr.Size == 0);
			InitializeModuleFrozenObjectSegment(intPtr2, length);
		}
	}

	private unsafe static void InitializeModuleFrozenObjectSegment(IntPtr segmentStart, int length)
	{
		if (RuntimeImports.RhRegisterFrozenSegment((void*)segmentStart, (nuint)length, (nuint)length, (nuint)length) == IntPtr.Zero)
		{
			RuntimeExceptionHelpers.FailFast("Failed to register frozen object segment for the module.");
		}
	}

	internal static void RunModuleInitializers()
	{
		for (int i = 0; i < s_moduleCount; i++)
		{
			RunInitializers(s_modules[i], ReadyToRunSectionType.ModuleInitializerList);
		}
	}

	private unsafe static void RunInitializers(TypeManagerHandle typeManager, ReadyToRunSectionType section)
	{
		int length;
		byte* ptr = (byte*)(void*)RuntimeImports.RhGetModuleSection(typeManager, section, out length);
		StartupDebug.Assert(length % (MethodTable.SupportsRelativePointers ? 4 : sizeof(IntPtr)) == 0);
		for (byte* ptr2 = ptr; ptr2 < ptr + length; ptr2 += (MethodTable.SupportsRelativePointers ? 4 : sizeof(IntPtr)))
		{
			(MethodTable.SupportsRelativePointers ? ((delegate*<void>)_003CRunInitializers_003Eg__ReadRelPtr32_007C9_0(ptr2)) : ((delegate*<void>)(*(IntPtr*)ptr2)))();
		}
	}

	private unsafe static object[] InitializeStatics(IntPtr gcStaticRegionStart, int length)
	{
		byte* ptr = (byte*)(void*)gcStaticRegionStart + length;
		object[] array = new object[length / (MethodTable.SupportsRelativePointers ? 4 : sizeof(IntPtr))];
		ref object source = ref Unsafe.As<byte, object>(ref Unsafe.As<RawArrayData>(array).Data);
		int num = 0;
		for (byte* ptr2 = (byte*)(void*)gcStaticRegionStart; ptr2 < ptr; ptr2 += (MethodTable.SupportsRelativePointers ? 4 : sizeof(IntPtr)))
		{
			IntPtr* ptr3 = (IntPtr*)(MethodTable.SupportsRelativePointers ? _003CInitializeStatics_003Eg__ReadRelPtr32_007C10_0(ptr2) : (*(IntPtr**)ptr2));
			nint num2 = (MethodTable.SupportsRelativePointers ? ((nint)_003CInitializeStatics_003Eg__ReadRelPtr32_007C10_0(ptr3)) : ((nint)(*ptr3)));
			if ((num2 & 1) == 1)
			{
				object obj = null;
				RuntimeImports.RhAllocateNewObject(new IntPtr(num2 & -4), 64u, &obj);
				if (obj == null)
				{
					RuntimeExceptionHelpers.FailFast("Failed allocating GC static bases");
				}
				if ((num2 & 2) == 2)
				{
					void* ptr4 = (MethodTable.SupportsRelativePointers ? _003CInitializeStatics_003Eg__ReadRelPtr32_007C10_0((byte*)ptr3 + 4) : ((void*)ptr3[1]));
					RuntimeImports.RhBulkMoveWithWriteBarrier(ref obj.GetRawData(), ref *(byte*)ptr4, obj.GetRawObjectDataSize());
				}
				StartupDebug.Assert(num < array.Length);
				Unsafe.Add(ref source, num) = obj;
				*ptr3 = *(IntPtr*)(&obj);
			}
			num++;
		}
		return array;
	}

	private unsafe static void RehydrateData(IntPtr dehydratedData, int length)
	{
		byte* ptr = (byte*)_003CRehydrateData_003Eg__ReadRelPtr32_007C11_0((void*)dehydratedData);
		byte* ptr2 = (byte*)(void*)dehydratedData + 4;
		byte* ptr3 = (byte*)(void*)dehydratedData + length;
		int* ptr4 = (int*)ptr3;
		while (ptr2 < ptr3)
		{
			ptr2 = DehydratedDataCommand.Decode(ptr2, out var command, out var payload);
			switch (command)
			{
			case 0:
				StartupDebug.Assert(payload != 0);
				if (payload < 4)
				{
					*ptr = *ptr2;
					if (payload > 1)
					{
						*((short*)(ptr + payload) - 1) = *((short*)(ptr2 + payload) - 1);
					}
				}
				else if (payload < 8)
				{
					*(int*)ptr = *(int*)ptr2;
					*((int*)(ptr + payload) - 1) = *((int*)(ptr2 + payload) - 1);
				}
				else if (payload <= 16)
				{
					*(long*)ptr = *(long*)ptr2;
					*((long*)(ptr + payload) - 1) = *((long*)(ptr2 + payload) - 1);
				}
				else
				{
					Unsafe.CopyBlock(ptr, ptr2, (uint)payload);
				}
				ptr += payload;
				ptr2 += payload;
				continue;
			case 1:
				ptr += payload;
				continue;
			case 3:
				*(void**)ptr = _003CRehydrateData_003Eg__ReadRelPtr32_007C11_0(ptr4 + payload);
				ptr += sizeof(void*);
				continue;
			case 2:
				_003CRehydrateData_003Eg__WriteRelPtr32_007C11_1(ptr, _003CRehydrateData_003Eg__ReadRelPtr32_007C11_0(ptr4 + payload));
				ptr += 4;
				continue;
			case 5:
				while (payload-- > 0)
				{
					*(void**)ptr = _003CRehydrateData_003Eg__ReadRelPtr32_007C11_0(ptr2);
					ptr += sizeof(void*);
					ptr2 += 4;
				}
				continue;
			case 4:
				break;
			default:
				continue;
			}
			while (payload-- > 0)
			{
				_003CRehydrateData_003Eg__WriteRelPtr32_007C11_1(ptr, _003CRehydrateData_003Eg__ReadRelPtr32_007C11_0(ptr2));
				ptr += 4;
				ptr2 += 4;
			}
		}
	}

	internal unsafe static void* _003CRunInitializers_003Eg__ReadRelPtr32_007C9_0(void* address)
	{
		return (byte*)address + *(int*)address;
	}

	internal unsafe static void* _003CInitializeStatics_003Eg__ReadRelPtr32_007C10_0(void* address)
	{
		return (byte*)address + *(int*)address;
	}

	internal unsafe static void* _003CRehydrateData_003Eg__ReadRelPtr32_007C11_0(void* address)
	{
		return (byte*)address + *(int*)address;
	}

	internal unsafe static void _003CRehydrateData_003Eg__WriteRelPtr32_007C11_1(void* dest, void* value)
	{
		*(int*)dest = (int)((byte*)value - (byte*)dest);
	}
}
