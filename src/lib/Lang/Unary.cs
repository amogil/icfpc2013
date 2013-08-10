using System;
using System.Collections.Generic;
using System.Linq;

namespace lib.Lang
{
	public abstract class Unary : Expr
	{
		public static readonly Dictionary<string, Func<Expr, Expr>> Operators = new Dictionary<string, Func<Expr, Expr>>
		{
			{"not", arg => new UnaryNot(arg)},
			{"shl1", arg => new UnaryShl1(arg)},
			{"shr1", arg => new UnaryShr1(arg)},
			{"shr4", arg => new UnaryShr4(arg)},
			{"shr16", arg => new UnaryShr16(arg)},
		};

		public static readonly Dictionary<string, byte> OperatorsBinForms = new Dictionary<string, byte>
			{
				{"not", 7},
				{"shl1", 8},
				{"shr1", 9},
				{"shr4", 10},
				{"shr16", 11}
			};


		public string Name;
		public Expr Arg;

		protected Unary(string name, Expr arg)
		{
			Name = String.Intern(name);
			Arg = arg;
		}

		public override string ToSExpr()
		{
			return string.Format("({0} {1})", Name, Arg);
		}

		public override IEnumerable<byte> ToBinExp()
		{
			return new byte[]{OperatorsBinForms[Name]}.Concat(Arg.ToBinExp());
		}
	}

	public class UnaryNot : Unary
	{
		public UnaryNot(Expr arg)
			: base("not", arg)
		{
		}

		public override UInt64 Eval(Vars vars)
		{
			return ~Arg.Eval(vars);
		}

		public override object Clone()
		{
			return new UnaryNot((Expr)Arg.Clone());
		}
	}

	public class UnaryShl1 : Unary
	{
		public UnaryShl1(Expr arg)
			: base("shl1", arg)
		{
		}

		public override UInt64 Eval(Vars vars)
		{
			return Arg.Eval(vars) << 1;
		}

		public override object Clone()
		{
			return new UnaryShl1((Expr)Arg.Clone());
		}
	}

	public class UnaryShr1 : Unary
	{
		public UnaryShr1(Expr arg)
			: base("shr1", arg)
		{
		}

		public override UInt64 Eval(Vars vars)
		{
			return Arg.Eval(vars) >> 1;
		}

		public override object Clone()
		{
			return new UnaryShr1((Expr)Arg.Clone());
		}
	}

	public class UnaryShr4 : Unary
	{
		public UnaryShr4(Expr arg)
			: base("shr4", arg)
		{
		}

		public override UInt64 Eval(Vars vars)
		{
			return Arg.Eval(vars) >> 4;
		}

		public override object Clone()
		{
			return new UnaryShr4((Expr)Arg.Clone());
		}
	}

	public class UnaryShr16 : Unary
	{
		public UnaryShr16(Expr arg)
			: base("shr16", arg)
		{
		}

		public override UInt64 Eval(Vars vars)
		{
			return Arg.Eval(vars) >> 16;
		}

		public override object Clone()
		{
			return new UnaryShr16((Expr)Arg.Clone());
		}
	}
}