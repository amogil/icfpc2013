using System;

namespace lib.Lang
{
	public class Const : Expr
	{
		public readonly long value;

		public Const(Int64 value)
		{
			this.value = value;
		}


		public override long Eval(Vars vars)
		{
			return value;
		}

		public override string ToSExpr()
		{
			return value.ToString();
		}
	}
}