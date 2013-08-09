using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using lib.Lang;

namespace lib.Brute
{
	public class Force
	{
		public IEnumerable<Expr> Solve(int sizeMinus1, params string[] ops)
		{
			return Solve(sizeMinus1, false, ops);
		}

		public IEnumerable<Expr> Solve(int sizeMinus1, bool inFold, params string[] ops)
		{
			if (sizeMinus1 == 1)
			{
				return inFold
					? new Expr[] { new Const(0), new Const(1), new Var("x", v => v.x), new Var("i", v => v.foldItem), new Var("a", v => v.foldAccumulator) }
					: new Expr[] {new Const(0), new Const(1), new Var("x", v => v.x)};
			}
			IEnumerable<Expr> topUnary = Solve(sizeMinus1 - 1, inFold, ops).SelectMany(ast => GrowTree(ast, ops));
			IEnumerable<Expr> topBinary = SolveTopBinary(sizeMinus1, inFold, ops);
			IEnumerable<Expr> topIf = SolveTopIf(sizeMinus1, inFold, ops);
			IEnumerable<Expr> topFold = inFold ? new Expr[0] : SolveTopFold(sizeMinus1 - 1, ops);

			return topUnary.Concat(topBinary).Concat(topIf).Concat(topFold);
		}

		private IEnumerable<Expr> SolveTopIf(int resultSize, bool inFold, string[] ops)
		{
			if (!ops.Contains("if0")) yield break;

			
			var treesWithSize = GetTreesWithSizeNotLargerThan(resultSize-3, inFold, ops).GroupBy(t => t.Item1, t => t.Item2).ToList();
			for(int condSize=1; condSize<=resultSize-3; condSize++)
			{
				var maxLeftSize = resultSize - 2 - condSize;
				for (int leftSize = 1; leftSize <= (maxLeftSize+1)/2; leftSize++)
				{
					var rightSize = resultSize - 1 - condSize - leftSize;
					Debug.Assert(leftSize <= rightSize);
					var condExpr = treesWithSize.FirstOrDefault(g => g.Key == condSize);
					var leftExpr = treesWithSize.FirstOrDefault(g => g.Key == leftSize);
					var rightExpr = treesWithSize.FirstOrDefault(g => g.Key == rightSize);
					if (condExpr == null || leftExpr == null || rightExpr == null) continue;
					var answers = condExpr.Join(leftExpr, i => 1, o => 1, Tuple.Create).Join(rightExpr, i => 1, o => 1, (tuple, right) => Tuple.Create(tuple.Item1, tuple.Item2, right));
					foreach (var answer in answers)
					{
						yield return new If0(answer.Item1, answer.Item2, answer.Item3);
						if (leftSize != rightSize)
							yield return new If0(answer.Item1, answer.Item3, answer.Item2);
					}
				}
			}
		}

		private IEnumerable<Expr> SolveTopBinary(int resultSize, bool inFold, string[] ops)
		{
			List<Tuple<int, Expr>> treesWithSize = GetTreesWithSizeNotLargerThan(resultSize-2, inFold, ops);
			var args = treesWithSize.Join(treesWithSize, g => g.Item1, g => resultSize - 1 - g.Item1, (left, right) => new {left = left.Item2, right = right.Item2});
			foreach (var op in Binary.BinaryOperators.Where(b => ops.Contains(b.Key)).Select(p => p.Value))
			{
				foreach (Expr ast in args.Select(argsPair => op(argsPair.left, argsPair.right)))
				{
					yield return ast;
				}
			}
		}

		private IEnumerable<Expr> SolveTopFold(int resultSize, string[] ops)
		{
			if (!ops.Contains("fold")) yield break;
			var noFold = ops.Where(o => o != "fold").ToArray();
			var treesWithSizeOutsideFold = GetTreesWithSizeNotLargerThan(resultSize - 3, false, noFold).GroupBy(t => t.Item1, t => t.Item2).ToList();
			var treesWithSizeInsideFold = GetTreesWithSizeNotLargerThan(resultSize - 3, true, noFold).GroupBy(t => t.Item1, t => t.Item2).ToList();
			for (int condSize = 1; condSize <= resultSize - 3; condSize++)
			{
				var maxLeftSize = resultSize - 2 - condSize;
				for (int leftSize = 1; leftSize <= maxLeftSize; leftSize++)
				{
					var rightSize = resultSize - 1 - condSize - leftSize;
					//Debug.Assert(leftSize <= rightSize);
					var collection = treesWithSizeOutsideFold.FirstOrDefault(g => g.Key == condSize);
					var start = treesWithSizeOutsideFold.FirstOrDefault(g => g.Key == leftSize);
					var foldFunc = treesWithSizeInsideFold.FirstOrDefault(g => g.Key == rightSize);
					if (collection == null || start == null || foldFunc == null) continue;
					var answers = collection.Join(start, i => 1, o => 1, Tuple.Create).Join(foldFunc, i => 1, o => 1, (tuple, right) => Tuple.Create(tuple.Item1, tuple.Item2, right));
					foreach (var answer in answers)
					{
						yield return new Fold(answer.Item1, answer.Item2, "i", "a", answer.Item3);
					}
				}
			}
		}

		private List<Tuple<int, Expr>> GetTreesWithSizeNotLargerThan(int maxTreeSize, bool inFold, string[] ops)
		{
			if (maxTreeSize < 1) return new List<Tuple<int, Expr>>();
			List<Tuple<int, Expr>> treesWithSize =
				Enumerable.Range(1, maxTreeSize)
				          .SelectMany(size => Solve(size, inFold, ops).Select(tree => Tuple.Create(size, tree))).ToList();
			return treesWithSize;
		}

		private IEnumerable<Expr> GrowTree(Expr ast, string[] ops)
		{
			return Unary.UnaryOperators.Where(kv => ops.Contains(kv.Key)).Select(kv => kv.Value(ast));
		}
	}

	[TestFixture]
	public class Force_Test
	{
		[TestCase(5, "shr4 if0", "(shr4 (if0 x x x))")]
		[TestCase(5, "shr4 if0", "(shr4 (if0 x x x))")]
		[TestCase(5, "shr4 if0", "(shr4 (if0 x x x))")]
		[TestCase(9, "or fold", "(fold x 0 (lambda (i a) (or (or i a) a)))")]
		[TestCase(10, "fold not or shr1 shr4", "(fold (shr1 (not x)) 0 (lambda (i a) (or i (shr4 a))))")]
		public void Test(int size, string ops, string expectedExpr)
		{
			var force = new Force();
			var trees = force.Solve(size, false, ops.Split(' ')).ToList();
			if (trees.All(tree => tree.ToSExpr() != expectedExpr))
			{
				foreach (var tree in trees)
				{
					Console.WriteLine(tree);
				}
				Assert.Fail();
			}
		}
	}

}