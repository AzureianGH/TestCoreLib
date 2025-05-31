using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Internal.Runtime; // For RuntimeImport attribute


namespace System;

[StructLayout(LayoutKind.Sequential)]
public class String
{
	private const int POINTER_SIZE = 8;

	internal const int FIRST_CHAR_OFFSET = 12;

	private int _stringLength;

	private char _firstChar;

	public int Length => _stringLength;

	public static string Empty => "";

	public char this[int index]
	{
		get
		{
			if (index < 0 || index >= _stringLength)
			{
				throw new IndexOutOfRangeException();
			}
			return index == 0 ? _firstChar : this[index - 1 + FIRST_CHAR_OFFSET / sizeof(char)];
		}
		set
		{
			if (index < 0 || index >= _stringLength)
			{
				throw new IndexOutOfRangeException();
			}
			if (index == 0)
			{
				_firstChar = value;
			}
			else
			{
				this[index - 1 + FIRST_CHAR_OFFSET / sizeof(char)] = value;
			}
		}
	}

	public String(char first, int length)
	{
		_stringLength = length;
		_firstChar = first;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[RuntimeImport("*", "_FastAllocateString")]
	public static extern string FastAllocStr(int length);




	


}
