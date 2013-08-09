using System;

namespace lib.Lang
{
	public class Fold : Expr
	{
		public Expr Start;
		public Expr Collection;
		public string ItemName;
		public string AccName;
		public Expr Func;

		public Fold(Expr collection, Expr start, string itemName, string accName, Expr func)
		{
			Start = start;
			Collection = collection;
			ItemName = itemName;
			AccName = accName;
			Func = func;
		}

		public override UInt64 Eval(Vars vars)
		{
			vars.foldAccumulator = Start.Eval(vars);
			var bytes = BitConverter.GetBytes(Collection.Eval(vars));
			foreach (var b in bytes)
			{
				vars.foldItem = b;
				vars.foldAccumulator = Func.Eval(vars);
			}
			return vars.foldAccumulator;
		}

		public override string ToSExpr()
		{
			return string.Format("(fold {0} {1} (lambda ({2} {3}) {4}))", Collection, Start, ItemName, AccName, Func);
		}
	}
}