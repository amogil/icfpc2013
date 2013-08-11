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

        private void SetChar(int ind, char c)
        {
            switch (c)
            {
                case('0'):
                    {
                        one = one.UnsetBit(ind);
                        zero = zero.SetBit(ind);
                        break;
                    }
                case('1'):
                    {
                        one = one.SetBit(ind);
                        zero = zero.UnsetBit(ind);
                        break;
                    }
                case('x'):
                    {
                        one = one.SetBit(ind);
                        zero = zero.SetBit(ind);
                        break;
                    }
                default:
                    {
                        one.UnsetBit(ind);
                        zero.UnsetBit(ind);
                        break;
                    }
            }
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
		    char p = MaskChar(false, true);
		    char ai, bi;
		    ulong resZero = ulong.MaxValue;
		    ulong resOne = 0;
		    var res = new Mask(resOne, resZero);
			for (int i = 0; i <=63 ; ++i)
			{
			    ai = MaskChar(this.one.Bit(i), this.zero.Bit(i));
			    bi = MaskChar(other.one.Bit(i), other.zero.Bit(i));
				
				var set = new[] {ai, bi, p};
				var onesCount = set.Count(c => c == '1');
				var zerosCount = set.Count(c => c == '0');
				if (onesCount == 2 && zerosCount == 1 || zerosCount == 3)
				    res.SetChar(i, '0');
				else if (onesCount == 1 && zerosCount == 2 || onesCount == 3)
					res.SetChar(i, '1');
				else
					res.SetChar(i, 'x');

				if (zerosCount == 2)
					p = '0';
				else if (onesCount >= 2)
					p = '1';
				else
					p = 'x';
			}
		    return res;

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