using System;
using System.Collections.Generic;

namespace lib.Lang
{
	public class Unary : Expr
	{
		public static Dictionary<string, Func<Expr, Expr>> UnaryOperators = new Dictionary<string, Func<Expr, Expr>>();
		static Unary()
		{
			Action<string, Func<Int64, Int64>> add = (name, f) => UnaryOperators.Add(name, argExpr => new Unary(name, argExpr, f));
			add("not", a => ~a);
			add("shl1", a => a<<1);
			add("shr1", a => a>>1);
			add("shr4", a => a>>4);
			add("shr16", a => a>>16);
		}

		public string Name { get; set; }
		public Expr Arg { get; set; }
		public Func<UInt64, UInt64> Func { get; set; }

		public Unary(string name, Expr arg, Func<UInt64, UInt64> func)
		{
			Name = name;
			Arg = arg;
			Func = func;
		}

		public override UInt64 Eval(Vars vars)
		{
			return Func(Arg.Eval(vars));
		}

		public override string ToSExpr()
		{
			return string.Format("({0} {1})", Name, Arg);
		}
	}
}