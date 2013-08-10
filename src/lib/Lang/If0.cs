using System;
using System.Collections.Generic;
using System.Linq;

namespace lib.Lang
{
	public class If0 : Expr
	{
		public readonly Expr cond;
		public readonly Expr trueExpr;
		public readonly Expr falseExpr;

		public If0(Expr cond, Expr trueExpr, Expr falseExpr)
		{
			this.cond = cond;
			this.trueExpr = trueExpr;
			this.falseExpr = falseExpr;
		}

		public override object Clone()
		{
			return new If0((Expr) cond.Clone(), (Expr) trueExpr.Clone(), (Expr) falseExpr.Clone());
		}

		public override IEnumerable<byte> ToBinExp()
		{
			return new byte[] {5}
				.Concat(cond.ToBinExp())
				.Concat(trueExpr.ToBinExp())
				.Concat(falseExpr.ToBinExp());
		}

		public override UInt64 Eval(Vars vars)
		{
			if (cond.Eval(vars) == 0)
				return trueExpr.Eval(vars);
			else 
				return falseExpr.Eval(vars);
		}

		public override string ToSExpr()
		{
			return string.Format("(if0 {0} {1} {2})", cond, trueExpr, falseExpr);
		}
	}
}