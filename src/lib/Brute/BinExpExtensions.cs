using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using lib.Lang;

namespace lib.Brute
{
	public class Parser
	{
		public static IEnumerable<string> Tokenize(string s)
		{
			var items = s.Split(" \t\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			var opens = items.SelectMany(i => i.Split(new[] { '(' }).AlternateWith("("));
			var closes = opens.SelectMany(i => i.Split(new[] { ')' }).AlternateWith(")"));
			return closes.Where(item => !string.IsNullOrEmpty(item));
		}

		public static byte[] ParseExpr(string s, string argName = null, string foldItemName = null, string foldAccName = null)
		{
			return ParseExpr(Tokenize(s + " #").GetEnumerator().MoveNextOrFail(), argName, foldItemName, foldAccName).ToArray();
		}

		public static byte[] ParseFunction(string s)
		{
			var tokens = Tokenize(s + " #").GetEnumerator().MoveNextOrFail();
			tokens.SkipToken("(").SkipToken("lambda").SkipToken("(");
			var varName = tokens.ExtractToken();
			tokens.SkipToken(")");
			return ParseExpr(tokens, varName, null, null).ToArray();
		}

		public static IEnumerable<byte> ParseExpr(IEnumerator<string> tokens, string argName, string foldItemName, string foldAccName)
		{
			if (tokens.Current == "(")
				foreach (var b in ParseCall(tokens, argName, foldItemName, foldAccName))
					yield return b;
			else
			{
				var t = tokens.ExtractToken();
				if (t == "1") yield return 1;
				else if (t == "0") yield return 0;
				else if (t == "#") throw new FormatException("wrong format");
				else if (t == foldItemName) yield return 3;
				else if (t == foldAccName) yield return 4;
				else if (t == argName) yield return 2;
				else throw new FormatException("unknown var " + t);
			}
		}

		private static IEnumerable<byte> ParseCall(IEnumerator<string> tokens, string argName, string foldItemName, string foldAccName)
		{
			Func<IEnumerable<byte>> parse = () => ParseExpr(tokens, argName, foldItemName, foldAccName);
			tokens.SkipToken("(");
			var t = tokens.ExtractToken();
			IEnumerable<byte> res;
			if (t == "if0") res = ParseFun(5, 3, parse);
			else if (t == "fold")
				res = ParseFold(tokens, argName, foldItemName, foldAccName);
			else if (t == "not") res = ParseFun(7, 1, parse);
			else if (t == "shl1") res = ParseFun(8, 1, parse);
			else if (t == "shr1") res = ParseFun(9, 1, parse);
			else if (t == "shr4") res = ParseFun(10, 1, parse);
			else if (t == "shr16") res = ParseFun(11, 1, parse);

			else if (t == "and") res = ParseFun(12, 2, parse);
			else if (t == "or") res = ParseFun(13, 2, parse);
			else if (t == "xor") res = ParseFun(14, 2, parse);
			else if (t == "plus") res = ParseFun(15, 2, parse);
			else throw new FormatException("unknown function " + t);
			foreach (var b in res) yield return b;
			tokens.SkipToken(")");
		}

		private static IEnumerable<byte> ParseFun(byte fun, int argsCount, Func<IEnumerable<byte>> parse)
		{
			yield return fun;
			for (int i = 0; i < argsCount; i++)
				foreach (var b in parse()) yield return b;
		}

		private static IEnumerable<byte> ParseFold(IEnumerator<string> tokens, string argName, string foldItemName, string foldAccName)
		{
			Func<IEnumerable<byte>> parse = () => ParseExpr(tokens, argName, foldItemName, foldAccName);
			yield return 6;
			foreach (var b in parse()) yield return b;
			foreach (var b in parse()) yield return b;
			tokens.SkipToken("(").SkipToken("lambda").SkipToken("(");
			var itemName = tokens.ExtractToken();
			var accName = tokens.ExtractToken();
			tokens.SkipToken(")");
			var expr = ParseExpr(tokens, argName, itemName, accName);
			foreach (var b in expr) yield return b;
			tokens.SkipToken(")");
		}
	}


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
			if (b == 0) return constant("0");
			if (b == 1) return constant("1");
			if (b == 2) return constant("x");
			if (b == 3) return constant("i");
			if (b == 4) return constant("a");
			if (b == 5) return fun("if0", 3);
			if (b == 6)
			{
				var e0 = program.ToSExpr(start + 1);
				var e1 = program.ToSExpr(e0.Item2);
				var e2 = program.ToSExpr(e1.Item2);
				string s = string.Format("(fold {0} {1} (lambda (i a) {2}))", e0.Item1, e1.Item1, e2.Item1);
				return Tuple.Create(s, e2.Item2);
			}
			if (b == 7) return fun("not", 1);
			if (b == 8) return fun("shl1", 1);
			if (b == 9) return fun("shr1", 1);
			if (b == 10) return fun("shr4", 1);
			if (b == 11) return fun("shr16", 1);
			if (b == 12) return fun("and", 2);
			if (b == 13) return fun("or", 2);
			if (b == 14) return fun("xor", 2);
			if (b == 15) return fun("plus", 2);
			else throw new FormatException(b.ToString());
		}
	}

	[TestFixture]
	public class BinExpExtensions_Test
	{
		[TestCase("0")]
		[TestCase("1")]
		[TestCase("(shl1 1)")]
		[TestCase("(shr1 1)")]
		[TestCase("(shr4 1)")]
		[TestCase("(shr16 1)")]
		[TestCase("(not 1)")]
		[TestCase("(and 1 0)")]
		[TestCase("(or 1 0)")]
		[TestCase("(xor 1 0)")]
		[TestCase("(plus 1 0)")]
		[TestCase("(if0 0 1 0)")]
		[TestCase("(fold 0 1 (lambda (i a) (plus i a)))")]
		[TestCase("(xor (fold 0 1 (lambda (i a) (plus i a))) 1)")]
		public void ParseCorrect(string s)
		{
			Console.WriteLine(Parser.ParseExpr(s).Printable());
			Assert.AreEqual(s, Parser.ParseExpr(s).ToSExpr().Item1);
		}
	}
}