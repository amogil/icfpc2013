using System;
using System.Collections.Generic;

namespace lib.Lang
{
	public abstract class Binary : Expr
	{
		public static readonly Dictionary<string, Func<Expr, Expr, Expr>> Operators = new Dictionary<string, Func<Expr, Expr, Expr>>
		{
			{"and", (left, right) => new BinaryAnd(left, right)},
			{"or", (left, right) => new BinaryOr(left, right)},
			{"xor", (left, right) => new BinaryXor(left, right)},
			{"plus", (left, right) => new BinaryPlus(left, right)},
		};

		public string Name;
		public Expr A;
		public Expr B;

		public Binary(string name, Expr a, Expr b)
		{
			Name = String.Intern(name);
			A = a;
			B = b;
		}

		public override string ToSExpr()
		{
			return string.Format("({0} {1} {2})", Name, A, B);
		}
	}

	public class BinaryAnd : Binary
	{
		public BinaryAnd(Expr a, Expr b)
			: base("and", a, b)
		{
		}

		public override UInt64 Eval(Vars vars)
		{
			return A.Eval(vars) & B.Eval(vars);
		}

		public override object Clone()
		{
			return new BinaryAnd((Expr)A.Clone(), (Expr)B.Clone());
		}
	}

	public class BinaryOr : Binary
	{
		public BinaryOr(Expr a, Expr b)
			: base("or", a, b)
		{
		}

		public override UInt64 Eval(Vars vars)
		{
			return A.Eval(vars) | B.Eval(vars);
		}

		public override object Clone()
		{
			return new BinaryOr((Expr)A.Clone(), (Expr)B.Clone());
		}
	}

	public class BinaryXor : Binary
	{
		public BinaryXor(Expr a, Expr b)
			: base("xor", a, b)
		{
		}

		public override UInt64 Eval(Vars vars)
		{
			return A.Eval(vars) ^ B.Eval(vars);
		}

		public override object Clone()
		{
			return new BinaryXor((Expr)A.Clone(), (Expr)B.Clone());
		}
	}

	public class BinaryPlus : Binary
	{
		public BinaryPlus(Expr a, Expr b)
			: base("plus", a, b)
		{
		}

		public override UInt64 Eval(Vars vars)
		{
			var ra = A.Eval(vars);
			var rb = B.Eval(vars);
			return unchecked (ra + rb);
		}

		public override object Clone()
		{
			return new BinaryPlus((Expr)A.Clone(), (Expr)B.Clone());
		}
	}
}