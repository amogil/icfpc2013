using System;
using System.Diagnostics;
using System.Linq;

namespace lib.Lang
{
	public static class BinExpExtensions
	{
		public static string Printable(this byte[] program)
		{
			char[] map = new[] {'0', '1', 'x', 'i', 'a', 'Y', 'F', '~', '<', '>', 'r', 'R', '&', '|', '^', '+'};
			return new string(program.Select(b => map[b]).ToArray());
		}

		public static ulong Eval(this byte[] program, ulong x)
		{
			int offset;
			return Eval(program, 0, x, 0, 0, out offset);
		}

		private static ulong Eval(this byte[] program, int start, ulong x, ulong item, ulong acc, out int offset)
		{
			byte code = program[start];
			if (code == 0)
			{
				offset = start + 1;
				return 0;
			}
			if (code == 1)
			{
				offset = start + 1;
				return 1;
			}
			if (code == 2)
			{
				offset = start + 1;
				return x;
			}
			if (code == 3)
			{
				offset = start + 1;
				return item;
			}
			if (code == 4)
			{
				offset = start + 1;
				return acc;
			}
			if (code == 5)
			{
				var cond = program.Eval(start + 1, x, item, acc, out offset);
				var eZero = program.Eval(offset, x, item, acc, out offset);
				var eElse = program.Eval(offset, x, item, acc, out offset);
				return cond == 0 ? eZero : eElse;
			}
			if (code == 6)
			{
				var collection = program.Eval(start + 1, x, item, acc, out offset);
				var accValue = program.Eval(offset, x, item, acc, out offset);
				var bytes = BitConverter.GetBytes(collection);
				var nextIndex = offset;
				foreach (var b in bytes)
					accValue = program.Eval(nextIndex, x, b, accValue, out offset);
				return accValue;
			}
			if (code == 7)
				return ~program.Eval(start + 1, x, item, acc, out offset);
			if (code == 8)
				return program.Eval(start + 1, x, item, acc, out offset) << 1;
			if (code == 9)
				return program.Eval(start + 1, x, item, acc, out offset) >> 1;
			if (code == 10)
				return program.Eval(start + 1, x, item, acc, out offset) >> 4;
			if (code == 11)
				return program.Eval(start + 1, x, item, acc, out offset) >> 16;
			if (code == 12)
			{
				var a = program.Eval(start + 1, x, item, acc, out offset);
				var b = program.Eval(offset, x, item, acc, out offset);
				return a & b;
			}
			if (code == 13)
			{
				var a = program.Eval(start + 1, x, item, acc, out offset);
				var b = program.Eval(offset, x, item, acc, out offset);
				return a | b;
			}
			if (code == 14)
			{
				var a = program.Eval(start + 1, x, item, acc, out offset);
				var b = program.Eval(offset, x, item, acc, out offset);
				return a ^ b;
			}
			if (code == 15)
			{
				var a = program.Eval(start + 1, x, item, acc, out offset);
				var b = program.Eval(offset, x, item, acc, out offset);
				return unchecked(a + b);
			}
			throw new FormatException(code.ToString());
		}

		public static Tuple<string, int> ToSExpr(this byte[] program, int start = 0, bool insideFoldFunc = false)
		{
			Func<string, Tuple<string, int>> constant = name => Tuple.Create(name, start + 1);
			Func<string, int, Tuple<string, int>> fun = (name, count) =>
				{
					string s = "(" + name;
					int ind = start + 1;
					for (int i = 0; i < count; i++)
					{
						Tuple<string, int> v = program.ToSExpr(ind, insideFoldFunc);
						ind = v.Item2;
						s += " " + v.Item1;
					}

					return Tuple.Create(s + ")", ind);
				};
			byte b = program[start];
//			Debug.Assert(insideFoldFunc || (b != 3 && b != 4), program.Printable());
			if (b <= 4) return constant(Operations.names[b]);
			if (b == 6)
			{
				var e0 = program.ToSExpr(start + 1, insideFoldFunc);
				var e1 = program.ToSExpr(e0.Item2, insideFoldFunc);
				var e2 = program.ToSExpr(e1.Item2, true);
				string s = string.Format("(fold {0} {1} (lambda (i a) {2}))", e0.Item1, e1.Item1, e2.Item1);
				return Tuple.Create(s, e2.Item2);
			}
			else return fun(Operations.names[b], Operations.args[b]);
		}
	}
}