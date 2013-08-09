using System;

namespace lib.Lang
{
	public class Var : Expr
	{
		public string Name;
		public Func<Vars, UInt64> Func;

		public Var(string name, Func<Vars, UInt64> func)
		{
			Name = String.Intern(name);
			Func = func;
		}

	    public override object Clone()
	    {
	        return new Var(Name, Func);
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