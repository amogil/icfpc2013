using System;

namespace lib.Lang
{
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

		public override long Eval(Vars vars)
		{
			return Func(Arg.Eval(vars));
		}

		public override string ToSExpr()
		{
			return string.Format("({0} {1})", Name, Arg);
		}
	}
}