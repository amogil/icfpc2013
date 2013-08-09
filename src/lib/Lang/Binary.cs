using System;
using System.Collections.Generic;

namespace lib.Lang
{
	public class Binary : Expr
	{
		public static readonly Dictionary<string, Func<Expr, Expr, Expr>> Operators = new Dictionary<string, Func<Expr, Expr, Expr>>();

		static Binary()
		{
			Action<string, Func<UInt64, UInt64, UInt64>> add = (name, f) => Operators.Add(name, (left, right) => new Binary(name, left, right, f));
			add("and", (a, b) => a & b);
			add("or", (a, b) => a | b);
			add("xor", (a, b) => a ^ b);
			add("plus", (a, b) => unchecked(a + b));
		}

		public string Name;
		public Expr A;
		public Expr B;
		public Func<UInt64, UInt64, UInt64> Func;

		public Binary(string name, Expr a, Expr b, Func<UInt64, UInt64, UInt64> func)
		{
			Name = String.Intern(name);
			A = a;
			B = b;
			Func = func;
		}

		public override UInt64 Eval(Vars vars)
		{
			return Func(A.Eval(vars), B.Eval(vars));
		}

	    public override object Clone()
	    {
	        return new Binary((string) Name.Clone(), (Expr) A.Clone(), (Expr) B.Clone(), Func);
	    }

	    public override string ToSExpr()
		{
			return string.Format("({0} {1} {2})", Name, A, B);
		}
	}
}