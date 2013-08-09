using System;
using System.Collections.Generic;

namespace lib.Lang
{
	public class Binary : Expr
	{
		public static Dictionary<string, Func<Expr, Expr, Expr>> BinaryOperators = new Dictionary<string, Func<Expr, Expr, Expr>>();
		static Binary()
		{
			Action<string, Func<Int64, Int64, Int64>> add = (name, f) => BinaryOperators.Add(name, (left, right) => new Binary(name, left, right, f));
			add("and",(a,b) => a&b);
			add("or", (a, b) => a|b);
			add("xor", (a, b) => a^b);
			add("plus", (a, b) => a + b);
		}

		public string Name { get; set; }
		public Expr A { get; set; }
		public Expr B { get; set; }
		public Func<UInt64, UInt64, UInt64> Func { get; set; }

		public Binary(string name, Expr a, Expr b, Func<UInt64, UInt64, UInt64> func)
		{
			Name = name;
			A = a;
			B = b;
			Func = func;
		}

		public override UInt64 Eval(Vars vars)
		{
			return Func(A.Eval(vars), B.Eval(vars));
		}

		public override string ToSExpr()
		{
			return string.Format("({0} {1} {2})", Name, A, B);
		}
	}
}