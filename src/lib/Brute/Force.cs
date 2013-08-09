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
			var unOps = Unary.Operators.Where(kv => ops.Contains(kv.Key)).Select(kv => kv.Value).ToArray();
			var binOps = Binary.Operators.Where(kv => ops.Contains(kv.Key)).Select(kv => kv.Value).ToArray();
			var hasIf = ops.Contains("if0");
			var hasFold = ops.Contains("fold");
			return Solve(sizeMinus1, false, unOps, binOps, hasIf, hasFold);
		}

		private readonly Dictionary<Tuple<int, bool>, IList<Expr>> cache = new Dictionary<Tuple<int, bool>, IList<Expr>>();

		public IEnumerable<Expr> Solve(int sizeMinus1, bool inFold, Func<Expr, Expr>[] unOps, Func<Expr, Expr, Expr>[] binOps, bool hasIf, bool hasFold)
		{
			if (sizeMinus1 == 1)
			{
				Expr[] exprs = inFold ? new Expr[] {new Const(0), new Const(1), new Var("x", v => v.x), new Var("i", v => v.foldItem), new Var("a", v => v.foldAccumulator)} : new Expr[] {new Const(0), new Const(1), new Var("x", v => v.x)};
				return exprs;
			}

			var topUnary = Solve(sizeMinus1 - 1, inFold, unOps, binOps, hasIf, hasFold).SelectMany(ast => unOps.Select(op => op(ast)));
			var topBinary = SolveTopBinary(sizeMinus1, inFold, unOps, binOps, hasIf, hasFold);
			var topIf = SolveTopIf(sizeMinus1, inFold, unOps, binOps, hasIf, hasFold);
			var topFold = inFold ? new Expr[0] : SolveTopFold(sizeMinus1 - 1, unOps, binOps, hasIf, hasFold);

			return topUnary.Concat(topBinary).Concat(topIf).Concat(topFold);
		}

		private IList<Expr> Cache(int size, bool inFold, Func<IList<Expr>> getTrees)
		{
			IList<Expr> v;
			var key = Tuple.Create(size, inFold);
			if (!cache.TryGetValue(key, out v))
				cache.Add(key, v = getTrees());
			return v;
		}

		private IEnumerable<Expr> SolveTopIf(int resultSize, bool inFold, Func<Expr, Expr>[] unOps, Func<Expr, Expr, Expr>[] binOps, bool hasIf, bool hasFold)
		{
			if (!hasIf) yield break;

			List<IGrouping<int, Expr>> treesWithSize = GetTreesWithSizeNotLargerThan(resultSize - 3, inFold, unOps, binOps, hasIf, hasFold).GroupBy(t => t.Item1, t => t.Item2).ToList();
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

		private IEnumerable<Expr> SolveTopBinary(int resultSize, bool inFold, Func<Expr, Expr>[] unOps, Func<Expr, Expr, Expr>[] binOps, bool hasIf, bool hasFold)
		{
			List<Tuple<int, Expr>> treesWithSize = GetTreesWithSizeNotLargerThan(resultSize-2, inFold, unOps, binOps, hasIf, hasFold);
			var args = treesWithSize.Join(treesWithSize, g => g.Item1, g => resultSize - 1 - g.Item1, (left, right) => new {left = left.Item2, right = right.Item2});
			foreach (var op in binOps)
			{
				foreach (Expr ast in args.Select(argsPair => op(argsPair.left, argsPair.right)))
				{
					yield return ast;
				}
			}
		}

		private IEnumerable<Expr> SolveTopFold(int resultSize, Func<Expr, Expr>[] unOps, Func<Expr, Expr, Expr>[] binOps, bool hasIf, bool hasFold)
		{
			if (!hasFold) yield break;
			var treesWithSizeOutsideFold = GetTreesWithSizeNotLargerThan(resultSize - 3, false, unOps, binOps, hasIf, false).GroupBy(t => t.Item1, t => t.Item2).ToList();
			var treesWithSizeInsideFold = GetTreesWithSizeNotLargerThan(resultSize - 3, true, unOps, binOps, hasIf, false).GroupBy(t => t.Item1, t => t.Item2).ToList();
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

		private List<Tuple<int, Expr>> GetTreesWithSizeNotLargerThan(int maxTreeSize, bool inFold, Func<Expr, Expr>[] unOps, Func<Expr, Expr, Expr>[] binOps, bool hasIf, bool hasFold)
		{
			if (maxTreeSize < 1) return new List<Tuple<int, Expr>>();
			List<Tuple<int, Expr>> treesWithSize =
				Enumerable.Range(1, maxTreeSize)
						  .SelectMany(size => Solve(size, inFold, unOps, binOps, hasIf, hasFold).Select(tree => Tuple.Create(size, tree))).ToList();
			return treesWithSize;
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
			Stopwatch sw = Stopwatch.StartNew();
			var trees = force.Solve(size, ops.Split(' ')).ToList();
			Console.WriteLine(sw.Elapsed);
			Console.WriteLine(trees.Count());
			if (trees.All(tree => tree.ToSExpr() != expectedExpr))
			{
				foreach (var tree in trees)
				{
					Console.WriteLine(tree);
				}
				Assert.Fail();
			}
		}

		[Test]
		[Explicit]
		public void Perf()
		{
			var rnd = new Random();
			var force = new Force();
			var unaryOps = Unary.Operators.Select(t => t.Key);
			var binaryOps = Binary.Operators.Select(t => t.Key);
			var allOps = unaryOps.Concat(binaryOps).ToArray();

			var sw = Stopwatch.StartNew();
			var trees = force.Solve(8, allOps).ToList();
			sw.Stop();
			Console.Out.WriteLine("#trees: {0}, gen took: {1} ms", trees.Count, sw.ElapsedMilliseconds);

			var args = new List<Vars>();
			for (var i = 0; i < 4; i++)
				args.Add(new Vars(rnd.NextUlong()));
			Console.WriteLine("eval");
			sw.Restart();
			foreach (var arg in args)
				foreach (var tree in trees)
					tree.Eval(arg);
			sw.Stop();
			Console.Out.WriteLine("#trees: {0}, eval took: {1} ms", trees.Count, sw.ElapsedMilliseconds);
		}
	}
}