#define DEBUG
using System.Diagnostics;

namespace Internal.Runtime;

internal readonly struct MethodTableList
{
	private const int IsRelative = 1;

	private unsafe readonly void* _pFirst;

	public unsafe MethodTable* this[int index]
	{
		get
		{
			if (((nuint)_pFirst & (nuint)1u) != 0)
			{
				return ((RelativePointer<MethodTable>*)((byte*)_pFirst - 1 + (nint)index * (nint)sizeof(RelativePointer<MethodTable>)))->Value;
			}
			return ((MethodTable**)_pFirst)[index];
		}
	}

	public unsafe MethodTableList(MethodTable* pFirst)
	{
		Debug.Assert(((nuint)pFirst & (nuint)1u) == 0);
		_pFirst = pFirst;
	}

	public unsafe MethodTableList(RelativePointer<MethodTable>* pFirst)
	{
		Debug.Assert(((nuint)pFirst & (nuint)1u) == 0);
		_pFirst = (void*)((nuint)pFirst | (nuint)1u);
	}
}
