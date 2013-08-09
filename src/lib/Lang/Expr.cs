using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lib.Lang
{
	public static class Extensions
	{
		public static IEnumerable<string> AlternateWith(this IEnumerable<string> items, string alternator)
		{
			return items.SelectMany((s, i) => i == 0 ? new[]{s} : new[]{alternator, s});
		}

		public static IEnumerator<T> MoveNextOrFail<T>(this IEnumerator<T> tokens)
		{
			if (!tokens.MoveNext()) throw new FormatException("unexpected end of tokens");
			return tokens;
		}

		public static T ExtractToken<T>(this IEnumerator<T> tokens)
		{
			var t = tokens.Current;
			tokens.MoveNextOrFail();
			return t;
		}
		
		public static IEnumerator<T> SkipToken<T>(this IEnumerator<T> tokens, T expectedToken)
		{
			if (!expectedToken.Equals(tokens.Current)) throw new FormatException(string.Format("Expected {0} but was {1}", expectedToken, tokens.Current));
			return tokens.MoveNextOrFail();
		}
	}
	
	public abstract class Expr
	{
//		public abstract Int64 Eval(Vars vars);
		public abstract string ToSExpr();

		public override string ToString()
		{
			return ToSExpr();
		}
		
		public static IEnumerable<string> Tokenize(string s)
		{
			var items = s.Split(" \t\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			var opens = items.SelectMany(i => i.Split(new[] {'('}).AlternateWith("("));
			var closes = opens.SelectMany(i => i.Split(new[] {')'}).AlternateWith(")"));
			return closes.Where(item => !string.IsNullOrEmpty(item));
		}

		public static Expr ParseNext(IEnumerator<string> tokens)
		{
			return Parse(tokens.MoveNextOrFail());
		}

		public static Expr Parse(string s)
		{
			return Parse(Tokenize(s + " #").GetEnumerator().MoveNextOrFail());
		}

		public static Expr Parse(IEnumerator<string> tokens)
		{
			if (tokens.Current == "(") return ParseSList(tokens);
			var t = tokens.ExtractToken();
			if (t == "1") return new Const(1);
			if (t == "0") return new Const(0);
			if (t == "#") throw new FormatException("wrong format");
			return new Var(t);
		}

		private static Expr ParseSList(IEnumerator<string> tokens)
		{
			tokens.SkipToken("(");
			Expr res;
			if (tokens.Current == "if0") res = new If0(ParseNext(tokens), Parse(tokens), Parse(tokens));
			else if (tokens.Current == "fold") res = ParseFold(tokens.MoveNextOrFail());
			else if (tokens.Current == "not") res = new Unary("not", ParseNext(tokens), v => ~v);
			else if (tokens.Current == "shl1") res = new Unary("shl1", ParseNext(tokens), v => v << 1);
			else if (tokens.Current == "shr1") res = new Unary("shr1", ParseNext(tokens), v => v >> 1);
			else if (tokens.Current == "shr4") res = new Unary("shr4", ParseNext(tokens), v => v >> 4);
			else if (tokens.Current == "shr16") res = new Unary("shr16", ParseNext(tokens), v => v >> 16);
			else if (tokens.Current == "and") res = new Binary("and", ParseNext(tokens), Parse(tokens), (a, b) => a & b);
			else if (tokens.Current == "or") res = new Binary("or", ParseNext(tokens), Parse(tokens), (a, b) => a | b);
			else if (tokens.Current == "xor") res = new Binary("xor", ParseNext(tokens), Parse(tokens), (a, b) => a ^ b);
			else if (tokens.Current == "plus") res = new Binary("plus", ParseNext(tokens), Parse(tokens), (a, b) => a + b);
			else throw new FormatException("unknown function " + tokens.Current);
			tokens.SkipToken(")");
			return res;
		}

		private static Expr ParseFold(IEnumerator<string> tokens)
		{
			var e1 = Parse(tokens);
			var e2 = Parse(tokens);
			tokens = tokens.SkipToken("(").SkipToken("lambda").SkipToken("(");
			var id1 = tokens.ExtractToken();
			var id2 = tokens.ExtractToken();
			tokens.SkipToken(")");
			var expr = Parse(tokens);
			return new Fold(e1, e2, id1, id2, expr);
		}

	}

	public class Var : Expr
	{
		public string Name { get; set; }

		public Var(string name)
		{
			Name = name;
		}

		public override string ToSExpr()
		{
			return Name;
		}
	}

	public class Binary : Expr
	{
		public string Name { get; set; }
		public Expr A { get; set; }
		public Expr B { get; set; }
		public Func<long, long, long> Func { get; set; }

		public Binary(string name, Expr a, Expr b, Func<Int64, Int64, Int64> func)
		{
			Name = name;
			A = a;
			B = b;
			Func = func;
		}

		public override string ToSExpr()
		{
			return string.Format("({0} {1} {2})", Name, A, B);
		}
	}

	public class Unary : Expr
	{
		public string Name { get; set; }
		public Expr Arg { get; set; }
		public Func<long, long> Func { get; set; }

		public Unary(string name, Expr arg, Func<Int64, Int64> func)
		{
			Name = name;
			Arg = arg;
			Func = func;
		}

		public override string ToSExpr()
		{
			return string.Format("({0} {1})", Name, Arg);
		}
	}

	public class Fold : Expr
	{
		public Expr Start { get; set; }
		public Expr Collection { get; set; }
		public string StartVarName { get; set; }
		public string ItemVarName { get; set; }
		public Expr Func { get; set; }

		public Fold(Expr start, Expr collection, string startVarName, string itemVarName, Expr func)
		{
			Start = start;
			Collection = collection;
			StartVarName = startVarName;
			ItemVarName = itemVarName;
			Func = func;
		}

		public override string ToSExpr()
		{
			return string.Format("(fold {0} {1} (lambda ({2} {3}) {4}))", Start, Collection, StartVarName, ItemVarName, Func);
		}
	}

	public class If0 : Expr
	{
		public readonly Expr cond;
		public readonly Expr trueExpr;
		public readonly Expr falseExpr;

		public If0(Expr cond, Expr trueExpr, Expr falseExpr)
		{
			this.cond = cond;
			this.trueExpr = trueExpr;
			this.falseExpr = falseExpr;
		}

		public override string ToSExpr()
		{
			return string.Format("(if0 {0} {1} {2})", cond, trueExpr, falseExpr);
		}
	}

	public class Const : Expr
	{
		public readonly long value;

		public Const(Int64 value)
		{
			this.value = value;
		}


		public override string ToSExpr()
		{
			return value.ToString();
		}
	}


	public class Vars
	{
	}
}
