using System;
using System.Collections.Generic;
using System.Linq;

namespace lib.Lang
{
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


	public class Vars
	{
	}
}
