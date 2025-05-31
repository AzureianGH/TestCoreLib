#define DEBUG
using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Internal.Runtime;

public struct MethodTable
{
	[StructLayout(LayoutKind.Explicit)]
	private struct RelatedTypeUnion
	{
		[FieldOffset(0)]
		public unsafe MethodTable* _pBaseType;

		[FieldOffset(0)]
		public unsafe MethodTable* _pRelatedParameterType;
	}

	private const int POINTER_SIZE = 8;

	private const int PADDING = 1;

	internal const int SZARRAY_BASE_SIZE = 24;

	private uint _uFlags;

	public uint _uBaseSize;

	private RelatedTypeUnion _relatedType;

	private ushort _usNumVtableSlots;

	private ushort _usNumInterfaces;

	private uint _uHashCode;

	internal static bool SupportsRelativePointers
	{
		[Intrinsic]
		get
		{
			throw new NotImplementedException();
		}
	}

	internal bool HasComponentSize => (int)_uFlags < 0;

	internal ushort ComponentSize => (ushort)(HasComponentSize ? ((ushort)_uFlags) : 0);

	internal ushort GenericParameterCount
	{
		get
		{
			Debug.Assert(IsGenericTypeDefinition);
			return (ushort)_uBaseSize;
		}
	}

	internal uint Flags => _uFlags;

	internal ushort ExtendedFlags
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (ushort)((!HasComponentSize) ? ((ushort)_uFlags) : 0);
		}
	}

	internal uint RawBaseSize => _uBaseSize;

	internal uint BaseSize
	{
		get
		{
			Debug.Assert(IsCanonical || IsArray);
			return _uBaseSize;
		}
	}

	internal ushort NumVtableSlots => _usNumVtableSlots;

	internal ushort NumInterfaces => _usNumInterfaces;

	internal uint HashCode => _uHashCode;

	private EETypeKind Kind => (EETypeKind)(_uFlags & 0x30000);

	internal bool HasGenericVariance => (_uFlags & 0x800000) != 0;

	internal bool IsFinalizable => (_uFlags & 0x100000) != 0;

	internal bool IsNullable => ElementType == EETypeElementType.Nullable;

	internal bool IsDefType
	{
		get
		{
			EETypeKind kind = Kind;
			return kind == EETypeKind.CanonicalEEType || kind == EETypeKind.GenericTypeDefEEType;
		}
	}

	internal bool IsCanonical => Kind == EETypeKind.CanonicalEEType;

	internal bool IsString => ComponentSize == 2 && IsCanonical;

	internal bool IsArray
	{
		get
		{
			EETypeElementType elementType = ElementType;
			return elementType == EETypeElementType.Array || elementType == EETypeElementType.SzArray;
		}
	}

	internal int ArrayRank
	{
		get
		{
			Debug.Assert(IsArray);
			int num = (int)(ParameterizedTypeShape - 24);
			if (num > 0)
			{
				return num / 8;
			}
			return 1;
		}
	}

	internal bool IsSzArray
	{
		get
		{
			Debug.Assert(IsArray);
			return BaseSize == 24;
		}
	}

	internal unsafe bool IsMultiDimensionalArray
	{
		get
		{
			Debug.Assert(HasComponentSize);
			return BaseSize > (uint)(3 * sizeof(IntPtr));
		}
	}

	internal bool IsGeneric => (_uFlags & 0x2000000) != 0;

	internal bool IsGenericTypeDefinition => Kind == EETypeKind.GenericTypeDefEEType;

	internal unsafe MethodTable* GenericDefinition
	{
		get
		{
			Debug.Assert(IsGeneric);
			uint fieldOffset = GetFieldOffset(EETypeField.ETF_GenericDefinition);
			if (IsDynamicType || !SupportsRelativePointers)
			{
				return GetField<Pointer<MethodTable>>(fieldOffset).Value;
			}
			return GetField<RelativePointer<MethodTable>>(fieldOffset).Value;
		}
	}

	internal unsafe uint GenericArity
	{
		get
		{
			Debug.Assert(IsGeneric);
			return GenericDefinition->GenericParameterCount;
		}
	}

	internal unsafe MethodTableList GenericArguments
	{
		get
		{
			Debug.Assert(IsGeneric);
			void* ptr = (byte*)Unsafe.AsPointer(in this) + GetFieldOffset(EETypeField.ETF_GenericComposition);
			uint genericArity = GenericArity;
			if (IsDynamicType || !SupportsRelativePointers)
			{
				MethodTable* pFirst = (MethodTable*)((genericArity == 1) ? ptr : (*(MethodTable**)ptr));
				return new MethodTableList(pFirst);
			}
			RelativePointer<MethodTable>* pFirst2 = (RelativePointer<MethodTable>*)((genericArity == 1) ? ptr : ((void*)((RelativePointer*)ptr)->Value));
			return new MethodTableList(pFirst2);
		}
	}

	internal unsafe GenericVariance* GenericVariance
	{
		get
		{
			Debug.Assert(IsGeneric || IsGenericTypeDefinition);
			if (!HasGenericVariance)
			{
				return null;
			}
			if (IsGeneric)
			{
				return GenericDefinition->GenericVariance;
			}
			uint fieldOffset = GetFieldOffset(EETypeField.ETF_GenericComposition);
			if (IsDynamicType || !SupportsRelativePointers)
			{
				return GetField<Pointer<GenericVariance>>(fieldOffset).Value;
			}
			return GetField<RelativePointer<GenericVariance>>(fieldOffset).Value;
		}
	}

	internal bool IsPointer => ElementType == EETypeElementType.Pointer;

	internal bool IsByRef => ElementType == EETypeElementType.ByRef;

	internal bool IsInterface => ElementType == EETypeElementType.Interface;

	internal bool IsByRefLike => IsValueType && (_uFlags & 0x10) != 0;

	internal bool IsDynamicType => (_uFlags & 0x80000) != 0;

	internal bool IsParameterizedType => Kind == EETypeKind.ParameterizedEEType;

	internal bool IsFunctionPointer => Kind == EETypeKind.FunctionPointerEEType;

	internal uint ParameterizedTypeShape
	{
		get
		{
			Debug.Assert(IsParameterizedType);
			return _uBaseSize;
		}
	}

	internal uint NumFunctionPointerParameters
	{
		get
		{
			Debug.Assert(IsFunctionPointer);
			return _uBaseSize & 0x7FFFFFFF;
		}
	}

	internal bool IsUnmanagedFunctionPointer
	{
		get
		{
			Debug.Assert(IsFunctionPointer);
			return (_uBaseSize & 0x80000000u) != 0;
		}
	}

	internal unsafe MethodTableList FunctionPointerParameters
	{
		get
		{
			void* pFirst = (byte*)Unsafe.AsPointer(in this) + GetFieldOffset(EETypeField.ETF_FunctionPointerParameters);
			if (IsDynamicType || !SupportsRelativePointers)
			{
				return new MethodTableList((MethodTable*)pFirst);
			}
			return new MethodTableList((RelativePointer<MethodTable>*)pFirst);
		}
	}

	internal unsafe MethodTable* FunctionPointerReturnType
	{
		get
		{
			Debug.Assert(IsFunctionPointer);
			return _relatedType._pRelatedParameterType;
		}
	}

	internal bool RequiresAlign8
	{
		get
		{
			if (HasComponentSize)
			{
				Debug.Fail("RequiresAlign8 called for array or string");
			}
			return (_uFlags & 0x1000) != 0;
		}
	}

	internal bool IsIDynamicInterfaceCastable => (ExtendedFlags & 8) != 0;

	internal bool IsValueType => ElementType < EETypeElementType.Class;

	internal bool IsPrimitive => ElementType < EETypeElementType.ValueType;

	internal bool HasSealedVTableEntries => (_uFlags & 0x400000) != 0;

	internal bool ContainsGCPointers => (_uFlags & 0x1000000) != 0;

	internal bool IsTrackedReferenceWithFinalizer => (ExtendedFlags & 4) != 0;

	internal uint ValueTypeFieldPadding
	{
		get
		{
			Debug.Assert(IsValueType);
			return (_uFlags & 0xE0) >> 5;
		}
	}

	internal unsafe uint ValueTypeSize
	{
		get
		{
			Debug.Assert(IsValueType);
			return BaseSize - (uint)(sizeof(ObjHeader) + sizeof(MethodTable*) + (int)ValueTypeFieldPadding);
		}
	}

	internal unsafe MethodTable** InterfaceMap
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (MethodTable**)((byte*)Unsafe.AsPointer(in this) + sizeof(MethodTable) + sizeof(void*) * _usNumVtableSlots);
		}
	}

	internal bool HasDispatchMap => (_uFlags & 0x40000) != 0;

	internal unsafe DispatchMap* DispatchMap
	{
		get
		{
			if (!HasDispatchMap)
			{
				return null;
			}
			uint fieldOffset = GetFieldOffset(EETypeField.ETF_DispatchMap);
			if (IsDynamicType || !SupportsRelativePointers)
			{
				return GetField<Pointer<DispatchMap>>(fieldOffset).Value;
			}
			return GetField<RelativePointer<DispatchMap>>(fieldOffset).Value;
		}
	}

	internal IntPtr FinalizerCode
	{
		get
		{
			Debug.Assert(IsFinalizable);
			uint fieldOffset = GetFieldOffset(EETypeField.ETF_Finalizer);
			if (IsDynamicType || !SupportsRelativePointers)
			{
				return GetField<Pointer>(fieldOffset).Value;
			}
			return GetField<RelativePointer>(fieldOffset).Value;
		}
	}

	internal unsafe MethodTable* BaseType
	{
		get
		{
			if (!IsCanonical)
			{
				if (IsArray)
				{
					return GetArrayEEType();
				}
				return null;
			}
			return _relatedType._pBaseType;
		}
	}

	internal unsafe MethodTable* NonArrayBaseType
	{
		get
		{
			Debug.Assert(!IsArray, "array type not supported in NonArrayBaseType");
			Debug.Assert(IsCanonical || IsGenericTypeDefinition, "we expect type definitions here");
			Debug.Assert(!IsGenericTypeDefinition || _relatedType._pBaseType == null, "callers assume this would be null for a generic definition");
			return _relatedType._pBaseType;
		}
	}

	internal unsafe MethodTable* NullableType
	{
		get
		{
			Debug.Assert(IsNullable);
			Debug.Assert(GenericArity == 1);
			return GenericArguments[0];
		}
	}

	internal byte NullableValueOffset
	{
		get
		{
			Debug.Assert(IsNullable);
			int num = (int)(_uFlags & 0x700) >> 8;
			return (byte)(1 << num);
		}
	}

	internal unsafe MethodTable* RelatedParameterType
	{
		get
		{
			Debug.Assert(IsParameterizedType);
			return _relatedType._pRelatedParameterType;
		}
	}

	internal unsafe MethodTable* DynamicTemplateType
	{
		get
		{
			Debug.Assert(IsDynamicType);
			return GetField<Pointer<MethodTable>>(EETypeField.ETF_DynamicTemplateType).Value;
		}
	}

	internal bool IsDynamicTypeWithCctor => (DynamicTypeFlags & DynamicTypeFlags.HasLazyCctor) != 0;

	internal IntPtr DynamicGcStaticsData
	{
		get
		{
			Debug.Assert((DynamicTypeFlags & DynamicTypeFlags.HasGCStatics) != 0);
			return GetField<IntPtr>(EETypeField.ETF_DynamicGcStatics);
		}
	}

	internal IntPtr DynamicNonGcStaticsData
	{
		get
		{
			Debug.Assert((DynamicTypeFlags & DynamicTypeFlags.HasNonGCStatics) != 0);
			return GetField<IntPtr>(EETypeField.ETF_DynamicNonGcStatics);
		}
	}

	internal IntPtr DynamicThreadStaticsIndex
	{
		get
		{
			Debug.Assert((DynamicTypeFlags & DynamicTypeFlags.HasThreadStatics) != 0);
			return GetField<IntPtr>(EETypeField.ETF_DynamicThreadStaticOffset);
		}
	}

	internal unsafe TypeManagerHandle TypeManager
	{
		get
		{
			uint fieldOffset = GetFieldOffset(EETypeField.ETF_TypeManagerIndirection);
			IntPtr intPtr = ((!IsDynamicType && SupportsRelativePointers) ? GetField<RelativePointer>(fieldOffset).Value : GetField<Pointer>(fieldOffset).Value);
			return *(TypeManagerHandle*)(void*)intPtr;
		}
	}

	internal unsafe void* WritableData
	{
		get
		{
			uint fieldOffset = GetFieldOffset(EETypeField.ETF_WritableData);
			if (!IsDynamicType && SupportsRelativePointers)
			{
				return (void*)GetField<RelativePointer>(fieldOffset).Value;
			}
			return (void*)GetField<Pointer>(fieldOffset).Value;
		}
	}

	internal DynamicTypeFlags DynamicTypeFlags
	{
		get
		{
			Debug.Assert(IsDynamicType);
			return (DynamicTypeFlags)(nint)GetField<IntPtr>(EETypeField.ETF_DynamicTypeFlags);
		}
	}

	internal EETypeElementType ElementType => (EETypeElementType)((_uFlags >> 26) & 0x1F);

	internal unsafe IntPtr GetClasslibFunction(ClassLibFunctionId id)
	{
		return (IntPtr)InternalCalls.RhpGetClasslibFunctionFromEEType((MethodTable*)Unsafe.AsPointer(in this), id);
	}

	[Intrinsic]
	public unsafe static extern MethodTable* Of<T>();

	internal unsafe IntPtr* GetVTableStartAddress()
	{
		return (IntPtr*)((byte*)Unsafe.AsPointer(in this) + sizeof(MethodTable));
	}

	private unsafe static IntPtr FollowRelativePointer(int* pDist)
	{
		int num = *pDist;
		return (IntPtr)((byte*)pDist + num);
	}

	private unsafe void* GetSealedVirtualTable()
	{
		Debug.Assert(HasSealedVTableEntries);
		uint fieldOffset = GetFieldOffset(EETypeField.ETF_SealedVirtualSlots);
		byte* ptr = (byte*)Unsafe.AsPointer(in this);
		if (IsDynamicType || !SupportsRelativePointers)
		{
			return *(void**)(ptr + fieldOffset);
		}
		return (void*)FollowRelativePointer((int*)(ptr + fieldOffset));
	}

	internal unsafe IntPtr GetSealedVirtualSlot(ushort slotNumber)
	{
		void* sealedVirtualTable = GetSealedVirtualTable();
		if (!SupportsRelativePointers)
		{
			return ((IntPtr*)sealedVirtualTable)[(int)slotNumber];
		}
		return FollowRelativePointer((int*)((byte*)sealedVirtualTable + (nint)(int)slotNumber * (nint)4));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe uint GetFieldOffset(EETypeField eField)
	{
		uint num = (uint)(sizeof(MethodTable) + IntPtr.Size * _usNumVtableSlots);
		num += (uint)(sizeof(MethodTable*) * NumInterfaces);
		uint num2 = ((IsDynamicType || !SupportsRelativePointers) ? ((uint)IntPtr.Size) : 4u);
		if (eField == EETypeField.ETF_TypeManagerIndirection)
		{
			return num;
		}
		num += num2;
		if (eField == EETypeField.ETF_WritableData)
		{
			return num;
		}
		num += num2;
		if (eField == EETypeField.ETF_DispatchMap)
		{
			Debug.Assert(HasDispatchMap);
			return num;
		}
		if (HasDispatchMap)
		{
			num += num2;
		}
		if (eField == EETypeField.ETF_Finalizer)
		{
			Debug.Assert(IsFinalizable);
			return num;
		}
		if (IsFinalizable)
		{
			num += num2;
		}
		if (eField == EETypeField.ETF_SealedVirtualSlots)
		{
			return num;
		}
		if (HasSealedVTableEntries)
		{
			num += num2;
		}
		if (eField == EETypeField.ETF_GenericDefinition)
		{
			Debug.Assert(IsGeneric);
			return num;
		}
		if (IsGeneric)
		{
			num += num2;
		}
		if (eField == EETypeField.ETF_GenericComposition)
		{
			Debug.Assert(IsGeneric || (IsGenericTypeDefinition && HasGenericVariance));
			return num;
		}
		if (IsGeneric || (IsGenericTypeDefinition && HasGenericVariance))
		{
			num += num2;
		}
		if (eField == EETypeField.ETF_FunctionPointerParameters)
		{
			Debug.Assert(IsFunctionPointer);
			return num;
		}
		if (IsFunctionPointer)
		{
			num += NumFunctionPointerParameters * num2;
		}
		if (eField == EETypeField.ETF_DynamicTemplateType)
		{
			Debug.Assert(IsDynamicType);
			return num;
		}
		if (IsDynamicType)
		{
			num += (uint)IntPtr.Size;
		}
		DynamicTypeFlags dynamicTypeFlags = (DynamicTypeFlags)0;
		if (eField == EETypeField.ETF_DynamicTypeFlags)
		{
			Debug.Assert(IsDynamicType);
			return num;
		}
		if (IsDynamicType)
		{
			dynamicTypeFlags = (DynamicTypeFlags)(nint)GetField<IntPtr>(num);
			num += (uint)IntPtr.Size;
		}
		if (eField == EETypeField.ETF_DynamicGcStatics)
		{
			Debug.Assert((dynamicTypeFlags & DynamicTypeFlags.HasGCStatics) != 0);
			return num;
		}
		if ((dynamicTypeFlags & DynamicTypeFlags.HasGCStatics) != 0)
		{
			num += (uint)IntPtr.Size;
		}
		if (eField == EETypeField.ETF_DynamicNonGcStatics)
		{
			Debug.Assert((dynamicTypeFlags & DynamicTypeFlags.HasNonGCStatics) != 0);
			return num;
		}
		if ((dynamicTypeFlags & DynamicTypeFlags.HasNonGCStatics) != 0)
		{
			num += (uint)IntPtr.Size;
		}
		if (eField == EETypeField.ETF_DynamicThreadStaticOffset)
		{
			Debug.Assert((dynamicTypeFlags & DynamicTypeFlags.HasThreadStatics) != 0);
			return num;
		}
		Debug.Fail("Unknown MethodTable field type");
		return 0u;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ref T GetField<T>(EETypeField eField)
	{
		return ref Unsafe.As<byte, T>(ref ((byte*)Unsafe.AsPointer(in this))[GetFieldOffset(eField)]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ref T GetField<T>(uint offset)
	{
		return ref Unsafe.As<byte, T>(ref ((byte*)Unsafe.AsPointer(in this))[offset]);
	}

	internal unsafe MethodTable* GetArrayEEType()
	{
		return Of<Array>();
	}

	internal Exception GetClasslibException(ExceptionIDs id)
	{
		return RuntimeExceptionHelpers.GetRuntimeException(id);
	}
}
