using System;

namespace lib.Lang
{
	public class Var : Expr
	{
		public string Name { get; set; }
		public Func<Vars, long> Func { get; set; }

		public Var(string name, Func<Vars, Int64> func)
		{
			Name = name;
			Func = func;
		}

		public override long Eval(Vars vars)
		{
			return Func(vars);
		}

		public override string ToSExpr()
		{
			return Name;
		}
	}
}