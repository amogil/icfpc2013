namespace lib.Lang
{
	public class Var : Expr
	{
		public string Name { get; set; }

		public Var(string name)
		{
			Name = name;
		}

		public override string ToSExpr()
		{
			return Name;
		}
	}
}