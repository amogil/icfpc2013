using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace lib.Lang
{
	public class Mask
	{
		public ulong one;
		public ulong zero;

		public static Mask X = new Mask(UInt64.MaxValue, UInt64.MaxValue);
		public static Mask _1;
		public static Mask _0;

		static Mask()
		{
			_0 = new Mask(one: 0, zero: ~(ulong)0);
			_1 = new Mask(one: 1, zero: ~(ulong)1);
		}

		public Mask(ICollection<ulong> values)
		{
			one = values.Aggregate((ulong) 0, (acc, i) => acc | i);
			zero = values.Aggregate((ulong) 0, (acc, i) => acc | ~i);
		}

		public Mask(string s)
		{
			s = s.PadLeft(64, 'x');
			one = 0;
			zero = 0;
			for (int i = 0; i < 64; i++)
			{
				var c = s[63-i];
				one = one | ((c != '0' ? (ulong)1 : 0) << i);
				zero = zero | ((c != '1' ? (ulong)1 : 0) << i);
			}
		}

		public Mask(ulong one, ulong zero)
		{
			this.one = one;
			this.zero = zero;
		}

		public bool IncludedIn(Mask big)
		{
			return ((~big.one & one) == 0) && ((~big.zero & zero) == 0);
		}

		public override string ToString()
		{
			return new string(Enumerable.Range(0, 64).Select(i => MaskChar(one.Bit(i), zero.Bit(i))).Reverse().ToArray());
		}

		private char MaskChar(bool canBeOne, bool canBeZero)
		{
			if (canBeOne && canBeZero) return 'x';
			if (canBeOne) return '1';
			if (canBeZero) return '0';
			else return '?';
		}

		public Mask Not()
		{
			return new Mask(zero, one);
		}

		public Mask ShiftLeft(int shift)
		{
			return new Mask(one << shift, ~(~zero << shift));
		}

		public Mask ShiftRight(int shift)
		{
			return new Mask(one >> shift, ~(~zero >> shift));
		}

		public Mask And(Mask other)
		{
			return new Mask(one & other.one, zero | other.zero);
		}

		public Mask Or(Mask other)
		{
			return new Mask(one | other.one, zero & other.zero);
		}

		public Mask Xor(Mask other)
		{
			return new Mask((one & other.zero | zero & other.one), (one & other.one | zero & other.zero));
		}

		public Mask Plus(Mask other)
		{
			var a = ToString();
			var b = other.ToString();
			char p = '0';
			var res = "";
			for (int i = 63; i >=0; i--)
			{
				var ai = a[i];
				var bi = b[i];
				var set = new[] {ai, bi, p};
				var onesCount = set.Count(c => c == '1');
				var zerosCount = set.Count(c => c == '0');
				if (onesCount == 2 && zerosCount == 1 || zerosCount == 3)
					res = '0' + res;
				else if (onesCount == 1 && zerosCount == 2 || onesCount == 3)
					res = '1' + res;
				else
					res = 'x' + res;
				if (zerosCount == 2)
					p = '0';
				else if (onesCount >= 2)
					p = '1';
				else
					p = 'x';
			}
			return new Mask(res);

		}

		public bool CantBeZero()
		{
			return (one & ~zero) != 0;
		}

		public bool IsZero()
		{
			return (zero & ~one) == ulong.MaxValue;
		}

		public Mask Union(Mask other)
		{
			return new Mask(one | other.one, zero | other.zero);
		}
	}
}