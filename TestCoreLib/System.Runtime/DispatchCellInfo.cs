using Internal.Runtime;

namespace System.Runtime;

internal struct DispatchCellInfo
{
	public DispatchCellType CellType;

	public unsafe MethodTable* InterfaceType;

	public ushort InterfaceSlot;

	public byte HasCache;

	public uint MetadataToken;

	public uint VTableOffset;
}
