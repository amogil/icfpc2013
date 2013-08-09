using System;
using System.Collections.Generic;

namespace lib.Lang
{
	public class Unary : Expr
	{
		public static readonly Dictionary<string, Func<Expr, Expr>> Operators = new Dictionary<string, Func<Expr, Expr>>();

		static Unary()
		{
			Action<string, Func<UInt64, UInt64>> add = (name, f) => Operators.Add(name, argExpr => new Unary(name, argExpr, f));
			add("not", a => ~a);
			add("shl1", a => a << 1);
			add("shr1", a => a >> 1);
			add("shr4", a => a >> 4);
			add("shr16", a => a >> 16);
		}

		public string Name;
		public Expr Arg;
		public Func<UInt64, UInt64> Func;

		public Unary(string name, Expr arg, Func<UInt64, UInt64> func)
		{
			Name = String.Intern(name);
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