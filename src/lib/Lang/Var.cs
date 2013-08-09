using System;

namespace lib.Lang
{
	public class Var : Expr
	{
		public string Name { get; set; }
		public Func<Vars, UInt64> Func { get; set; }

		public Var(string name, Func<Vars, UInt64> func)
		{
			Name = name;
			Func = func;
		}

		public override UInt64 Eval(Vars vars)
		{
			return Func(vars);
		}

		public override string ToSExpr()
		{
			return Name;
		}
	}
}