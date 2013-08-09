using System;

namespace lib.Lang
{
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

		public override long Eval(Vars vars)
		{
			return Func(A.Eval(vars), B.Eval(vars));
		}

		public override string ToSExpr()
		{
			return string.Format("({0} {1} {2})", Name, A, B);
		}
	}
}