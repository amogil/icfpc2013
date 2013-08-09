using System;

namespace lib.Lang
{
	public class Fold : Expr
	{
		public Expr Start { get; set; }
		public Expr Collection { get; set; }
		public string ItemName { get; set; }
		public string AccName { get; set; }
		public Expr Func { get; set; }

		public Fold(Expr collection, Expr start, string itemName, string accName, Expr func)
		{
			Start = start;
			Collection = collection;
			ItemName = itemName;
			AccName = accName;
			Func = func;
		}

		public override long Eval(Vars vars)
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