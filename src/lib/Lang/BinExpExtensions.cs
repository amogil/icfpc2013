using System;
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
			return Eval(program, 0, x, 0, 0).Item1;
		}

		private static Tuple<ulong, int> Eval(this byte[] program, int start, ulong x, ulong item, ulong acc)
		{
			Func<ulong, Tuple<ulong, int>> constant = v => Tuple.Create(v, start + 1);
			Func<Func<ulong, ulong>, Tuple<ulong, int>> unary = f =>
			{
				var t = program.Eval(start + 1, x, item, acc);
				return Tuple.Create(f(t.Item1), t.Item2);
			};
			Func<Func<ulong, ulong, ulong>, Tuple<ulong, int>> binary = f =>
			{
				var a = program.Eval(start + 1, x, item, acc);
				var b = program.Eval(a.Item2, x, item, acc);
				return Tuple.Create(f(a.Item1, b.Item1), b.Item2);
			};
			byte code = program[start];
			if (code == 0) return constant(0);
			if (code == 1) return constant(1);
			if (code == 2) return constant(x);
			if (code == 3) return constant(item);
			if (code == 4) return constant(acc);
			if (code == 5)
			{
				var cond = program.Eval(start + 1, x, item, acc);
				var eZero = program.Eval(cond.Item2, x, item, acc);
				var eElse = program.Eval(eZero.Item2, x, item, acc);
				return Tuple.Create(cond.Item1 == 0 ? eZero.Item1 : eElse.Item1, eElse.Item2);
			}
			if (code == 6)
			{
				var collection = program.Eval(start + 1, x, item, acc);
				var initAcc = program.Eval(collection.Item2, x, item, acc);
				var bytes = BitConverter.GetBytes(collection.Item1);
				var accValue = initAcc.Item1;
				var nextIndex = initAcc.Item2;
				foreach (var b in bytes)
				{
					var res = program.Eval(initAcc.Item2, x, b, accValue);
					accValue = res.Item1;
					nextIndex = res.Item2;
				}
				return Tuple.Create(accValue, nextIndex);
			}
			if (code == 7) return unary(a => ~a);
			if (code == 8) return unary(a => a<<1);
			if (code == 9) return unary(a => a>>1);
			if (code == 10) return unary(a => a>>4);
			if (code == 11) return unary(a => a>>16);
			if (code == 12) return binary((a, b) => a & b);
			if (code == 13) return binary((a, b) => a | b);
			if (code == 14) return binary((a, b) => a ^ b);
			if (code == 15) return binary((a, b) => unchecked(a + b));
			throw new FormatException(code.ToString());
		}

		public static Tuple<string, int> ToSExpr(this byte[] program, int start = 0)
		{
			Func<string, Tuple<string, int>> constant = name => Tuple.Create(name, start + 1);
			Func<string, int, Tuple<string, int>> fun = (name, count) =>
				{
					string s = "(" + name;
					int ind = start + 1;
					for (int i = 0; i < count; i++)
					{
						Tuple<string, int> v = program.ToSExpr(ind);
						ind = v.Item2;
						s += " " + v.Item1;
					}

					return Tuple.Create(s + ")", ind);
				};
			byte b = program[start];
			if (b <= 4) return constant(Operations.names[b]);
			if (b == 6)
			{
				var e0 = program.ToSExpr(start + 1);
				var e1 = program.ToSExpr(e0.Item2);
				var e2 = program.ToSExpr(e1.Item2);
				string s = string.Format("(fold {0} {1} (lambda (i a) {2}))", e0.Item1, e1.Item1, e2.Item1);
				return Tuple.Create(s, e2.Item2);
			}
			else return fun(Operations.names[b], Operations.args[b]);
		}
	}
}