using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using lib.Lang;

namespace lib.Brute
{
	public class Force
	{
		public IEnumerable<Expr> Solve(int sizeMinus1, params string[] ops)
		{
			if (sizeMinus1 == 1)
			{
				return new Expr[] {new Const(0), new Const(1), new Var("x", v => v.x)};
			}
			var topUnary = Solve(sizeMinus1 - 1, ops).SelectMany(ast => GrowTree(ast, ops));
			var topBinary = SolveTopBinary(sizeMinus1, ops);
			return topUnary.Concat(topBinary);
		}

		private IEnumerable<Expr> SolveTopBinary(int sizeMinus1, string[] ops)
		{
			var treesWithSize = Enumerable.Range(1, sizeMinus1 - 2)
				.SelectMany(size => Solve(size, ops).Select(tree => new {size, tree})).ToList();
			var args = treesWithSize.Join(treesWithSize, g => g.size, g => sizeMinus1 - 1 - g.size, Tuple.Create);
			foreach (var op in Binary.BinaryOperators.Where(b => ops.Contains(b.Key)).Select(p => p.Value))
			{
				foreach (var ast in args.Select(argsPair => op(argsPair.Item1.tree, argsPair.Item2.tree)))
				{
					yield return ast;
				}
			}
		}

		private IEnumerable<Expr> GrowTree(Expr ast, string[] ops)
		{
			return Unary.UnaryOperators.Where(kv => ops.Contains(kv.Key)).Select(kv => kv.Value(ast));
		}
	}

	[TestFixture]
	public class Force_Test
	{
		[Test]
		public void Test()
		{
			//Size: 8, Operators: and,shl1,shr1
			var trees = new Force().Solve(7, "and","shl1", "shr1").ToList();
			
			Console.WriteLine(trees.Count());
			foreach (var tree in trees)
			{
				Console.WriteLine(tree);
			}
		}
	}
}
