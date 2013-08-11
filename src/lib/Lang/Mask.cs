using System;
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
			one = values.Aggregate((ulong)0, (acc, i) => acc | i);
			zero = values.Aggregate((ulong)0, (acc, i) => acc | ~i);
		}

		public Mask(string s)
		{
			s = s.PadLeft(64, 'x');
			one = 0;
			zero = 0;
			for (int i = 0; i < 64; i++)
			{
				var c = s[63 - i];
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

		public Mask FastPlus(Mask other)
		{
			ulong xxx = 0;
			var i = 0;
			for (; i < 64 && !(one.Bit(i) && other.one.Bit(i)); i++)
			{
				xxx |= ((ulong) 1) << i;
			}
			for (var j = 63; j > i && !(one.Bit(j) && other.one.Bit(j)); j--)
			{
				xxx |= ((ulong)1) << j;
			}
			xxx = ulong.MaxValue ^ xxx;
			ulong resZero = (zero & other.zero) | xxx;
			ulong resOne = (one | other.one) | xxx;
			return new Mask(resOne, resZero);
		}

		public Mask Plus(Mask other)
		{
			const ulong resZero = ulong.MaxValue;
			const ulong resOne = 0;
			var res = new Mask(resOne, resZero);
			var pOnesCount = 0;
			var pZerosCount = 1;
			for (var i = 0; i < 64; ++i)
			{
				var canBeOneMy = one.Bit(i);
				var canBeZeroMy = zero.Bit(i);
				var canBeOneOther = other.one.Bit(i);
				var canBeZeroOther = other.zero.Bit(i);

				var onesCount = pOnesCount;
				var zerosCount = pZerosCount;
				CountZerosAndOnes(canBeOneMy, canBeZeroMy, ref onesCount, ref zerosCount);
				CountZerosAndOnes(canBeOneOther, canBeZeroOther, ref onesCount, ref zerosCount);

				if (onesCount == 2 && zerosCount == 1 || zerosCount == 3)
				{
					res.one = res.one.UnsetBit(i);
					res.zero = res.zero.SetBit(i);
				}
				else if (onesCount == 1 && zerosCount == 2 || onesCount == 3)
				{
					res.one = res.one.SetBit(i);
					res.zero = res.zero.UnsetBit(i);
				}
				else
				{
					res.one = res.one.SetBit(i);
					res.zero = res.zero.SetBit(i);
				}

				if (zerosCount == 2)
				{
					pOnesCount = 0;
					pZerosCount = 1;
				}
				else if (onesCount >= 2)
				{
					pOnesCount = 1;
					pZerosCount = 0;
				}
				else
				{
					pOnesCount = 0;
					pZerosCount = 0;
				}
			}
			return res;

		}

		private static void CountZerosAndOnes(bool canBeOne, bool canBeZero, ref int onesCount, ref int zerosCount)
		{
			if (canBeOne)
			{
				if (!canBeZero)
					onesCount++;
			}
			else
			{
				if (canBeZero)
					zerosCount++;
			}
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

		public bool IsConstant()
		{
			return ~(one ^ zero) == 0;
		}
	}
}