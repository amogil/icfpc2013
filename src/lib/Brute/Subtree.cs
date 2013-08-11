using System;
using lib.Lang;

namespace lib.Brute
{
	public class Subtree
	{
		public Subtree(byte[] buffer, int first, int last)
		{
			Buffer = buffer;
			First = first;
			Last = last;
		}

		public readonly byte[] Buffer;
		public readonly int First;
		public readonly int Last;

		public int Len
		{
			get { return Last - First + 1; }
		}

		public byte[] ToArray()
		{
			var res = new byte[Len];
			Array.Copy(Buffer, First, res, 0, Len);
			return res;
		}

		public int Size
		{
			get
			{
				int c = Len;
				for (int i = First; i <= Last; i++)
					if (Buffer[i] == Operations.Fold) c++;
				return c;
			}
		}
	}
}