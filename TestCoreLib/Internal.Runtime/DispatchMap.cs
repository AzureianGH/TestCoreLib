#define DEBUG
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Internal.Runtime;

internal struct DispatchMap
{
	internal struct DispatchMapEntry
	{
		internal ushort _usInterfaceIndex;

		internal ushort _usInterfaceMethodSlot;

		internal ushort _usImplMethodSlot;
	}

	internal struct StaticDispatchMapEntry
	{
		internal DispatchMapEntry _entry;

		internal ushort _usContextMapSource;
	}

	private ushort _standardEntryCount;

	private ushort _defaultEntryCount;

	private ushort _standardStaticEntryCount;

	private ushort _defaultStaticEntryCount;

	private DispatchMapEntry _dispatchMap;

	public uint NumStandardEntries => _standardEntryCount;

	public uint NumDefaultEntries => _defaultEntryCount;

	public uint NumStandardStaticEntries => _standardStaticEntryCount;

	public uint NumDefaultStaticEntries => _defaultStaticEntryCount;

	public unsafe int Size => 8 + sizeof(DispatchMapEntry) * (_standardEntryCount + _defaultEntryCount) + sizeof(StaticDispatchMapEntry) * (_standardStaticEntryCount + _defaultStaticEntryCount);

	public unsafe DispatchMapEntry* GetEntry(int index)
	{
		Debug.Assert(index <= _defaultEntryCount + _standardEntryCount);
		return (DispatchMapEntry*)Unsafe.AsPointer(in Unsafe.Add(ref _dispatchMap, index));
	}

	public unsafe DispatchMapEntry* GetStaticEntry(int index)
	{
		Debug.Assert(index <= _defaultStaticEntryCount + _standardStaticEntryCount);
		return (DispatchMapEntry*)((byte*)Unsafe.AsPointer(in Unsafe.Add(ref _dispatchMap, _standardEntryCount + _defaultEntryCount)) + (nint)index * (nint)sizeof(StaticDispatchMapEntry));
	}
}
