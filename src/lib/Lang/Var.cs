using System;
using System.Collections.Generic;

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

		public override IEnumerable<byte> ToBinExp()
		{
			switch (Name)
			{
				case "x":
					yield return 2;
					break;
				case "i":
					yield return 3;
					break;
				case "a":
					yield return 4;
					break;
				default:
					throw new FormatException("Can not convert non unified variable: " + Name);
			}
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