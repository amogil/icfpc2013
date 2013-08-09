namespace lib.Lang
{
	public class Fold : Expr
	{
		public Expr Start { get; set; }
		public Expr Collection { get; set; }
		public string StartVarName { get; set; }
		public string ItemVarName { get; set; }
		public Expr Func { get; set; }

		public Fold(Expr start, Expr collection, string startVarName, string itemVarName, Expr func)
		{
			Start = start;
			Collection = collection;
			StartVarName = startVarName;
			ItemVarName = itemVarName;
			Func = func;
		}

		public override string ToSExpr()
		{
			return string.Format("(fold {0} {1} (lambda ({2} {3}) {4}))", Start, Collection, StartVarName, ItemVarName, Func);
		}
	}
}