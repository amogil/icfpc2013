using System;

namespace lib.Lang
{
	public class Const : Expr
	{
		public readonly UInt64 value;

		public Const(UInt64 value)
		{
			this.value = value;
		}


		public override UInt64 Eval(Vars vars)
		{
			return value;
		}

		public override string ToSExpr()
		{
			return value.ToString();
		}
	}
}