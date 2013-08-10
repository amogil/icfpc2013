using System;
using System.Collections.Generic;
using System.Linq;

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
			ItemName = String.Intern(itemName);
			AccName = String.Intern(accName);
			Func = func;
		}

		public override object Clone()
		{
			return new Fold((Expr) Collection.Clone(), (Expr) Start.Clone(), ItemName, AccName, (Expr) Func.Clone());
		}

		public override IEnumerable<byte> ToBinExp()
		{
			return new byte[] {6}
				.Concat(Collection.ToBinExp())
				.Concat(Start.ToBinExp())
				.Concat(Func.ToBinExp());
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