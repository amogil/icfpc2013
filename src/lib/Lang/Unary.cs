using System;

namespace lib.Lang
{
	public class Unary : Expr
	{
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