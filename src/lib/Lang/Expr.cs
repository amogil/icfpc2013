using System;
using System.Collections.Generic;
using System.Linq;

namespace lib.Lang
{
	public abstract class Expr : ICloneable
	{
		public abstract UInt64 Eval(Vars vars);
		public abstract string ToSExpr();
		public abstract object Clone();

		public override string ToString()
		{
			return ToSExpr();
		}


		public Expr GetUnified()
		{
			var copy = (Expr) this.Clone();
		    var u = new Unifier();

            u.Unify(copy);

		    return copy;
		}
		
		public static IEnumerable<string> Tokenize(string s)
		{
			var items = s.Split(" \t\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			var opens = items.SelectMany(i => i.Split(new[] {'('}).AlternateWith("("));
			var closes = opens.SelectMany(i => i.Split(new[] {')'}).AlternateWith(")"));
			return closes.Where(item => !string.IsNullOrEmpty(item));
		}

		public static Expr ParseExpr(string s, string argName = null, string foldItemName = null, string foldAccName = null)
		{
			return ParseExpr(Tokenize(s + " #").GetEnumerator().MoveNextOrFail(), argName, foldItemName, foldAccName);
		}

		public static Expr ParseFunction(string s)
		{
			var tokens = Tokenize(s + " #").GetEnumerator().MoveNextOrFail();
			tokens.SkipToken("(").SkipToken("lambda").SkipToken("(");
			var varName = tokens.ExtractToken();
			tokens.SkipToken(")");
			return ParseExpr(tokens, varName, null, null);
		}

		public static UInt64 Eval(string sExpr, UInt64 arg)
		{
			return ParseFunction(sExpr).Eval(new Vars(arg));
		}

		public static Expr ParseExpr(IEnumerator<string> tokens, string argName, string foldItemName, string foldAccName)
		{
			if (tokens.Current == "(") return ParseCall(tokens, argName, foldItemName, foldAccName);
			var t = tokens.ExtractToken();
			if (t == "1") return new Const(1);
			if (t == "0") return new Const(0);
			if (t == "#") throw new FormatException("wrong format");
			if (t == foldItemName) return new Var(t, vars => vars.foldItem);
			if (t == foldAccName) return new Var(t, vars => vars.foldAccumulator);
			if (t == argName) return new Var(t, vars => vars.x);
			else throw new FormatException("unknown var " + t);
		}

		private static Expr ParseCall(IEnumerator<string> tokens, string argName, string foldItemName, string foldAccName)
		{
			Func<Expr> parse = () => ParseExpr(tokens, argName, foldItemName, foldAccName);
			tokens.SkipToken("(");
			var t = tokens.ExtractToken();
			Expr res;
			if (t == "if0") res = new If0(parse(), parse(), parse());
			else if (t == "fold") res = ParseFold(tokens, argName, foldItemName, foldAccName);
			else if (t == "not") res = new Unary("not", parse(), v => ~v);
			else if (t == "shl1") res = new Unary("shl1", parse(), v => v << 1);
			else if (t == "shr1") res = new Unary("shr1", parse(), v => v >> 1);
			else if (t == "shr4") res = new Unary("shr4", parse(), v => v >> 4);
			else if (t == "shr16") res = new Unary("shr16", parse(), v => v >> 16);
			else if (t == "and") res = new Binary("and", parse(), parse(), (a, b) => a & b);
			else if (t == "or") res = new Binary("or", parse(), parse(), (a, b) => a | b);
			else if (t == "xor") res = new Binary("xor", parse(), parse(), (a, b) => a ^ b);
			else if (t == "plus") res = new Binary("plus", parse(), parse(), (a, b) => unchecked(a + b));
			else throw new FormatException("unknown function " + t);
			tokens.SkipToken(")");
			return res;
		}

		private static Expr ParseFold(IEnumerator<string> tokens, string argName, string foldItemName, string foldAccName)
		{
			Func<Expr> parse = () => ParseExpr(tokens, argName, foldItemName, foldAccName);
			var collection = parse();
			var start = parse();
			tokens.SkipToken("(").SkipToken("lambda").SkipToken("(");
			var itemName = tokens.ExtractToken();
			var accName = tokens.ExtractToken();
			tokens.SkipToken(")");
			var expr = ParseExpr(tokens, argName, itemName, accName);
			tokens.SkipToken(")");
			return new Fold(collection, start, itemName, accName, expr);
		}

	}


	public class Vars
	{
		public Vars(UInt64 x)
		{
			this.x = x;
		}

		public UInt64 x;
		public UInt64 foldItem;
		public UInt64 foldAccumulator;
	}
}
