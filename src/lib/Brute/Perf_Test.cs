using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using lib.Lang;

namespace lib.Brute
{
	[TestFixture]
	[Explicit]
	public class Perf_Test
	{
		[Test]
		public void Force()
		{
			var force = new Force();
			Run(force.Solve, arg => new Vars(arg), (tree, arg) => tree.Eval(arg));
		}

		[Test]
		public void BinForce()
		{
			Run((size, ops) => new BinaryBruteForcer(ops).Enumerate(size), arg => arg, (tree, arg) => tree.Eval(arg));
		}

		private void Run<TTree, TArg>(Func<int, string[], IEnumerable<TTree>> getTrees, Func<ulong, TArg> createArg, Func<TTree, TArg, ulong> eval)
		{
			const long mb = 1024 * 1024;
			var rnd = new Random();
			var unaryOps = Unary.Operators.Select(t => t.Key).ToArray();
			var binaryOps = Binary.Operators.Select(t => t.Key).ToArray();
			var allOps = unaryOps.Concat(binaryOps).ToArray();
			var ops = allOps;
			const int problemSize = 8;
			
			var args = new List<TArg>();
			const int argsCount = 2;
			for (var i = 0; i < argsCount; i++)
				args.Add(createArg(rnd.NextUInt64()));

			Console.Out.WriteLine("size: {0}, ops: [{1}], #args: {2}, WS: {3} mb", problemSize, string.Join(" ", ops), argsCount, Environment.WorkingSet / mb);

			long treesCount = 0;
			var sw = Stopwatch.StartNew();
			for (var iter = 0; iter < argsCount; iter++)
			{
				var arg = args[iter];
				foreach (var tree in getTrees(problemSize, ops))
				{
					eval(tree, arg);
					if (iter == 0)
						++treesCount;
				}
			}
			sw.Stop();
			Console.Out.WriteLine("#trees: {0}, eval took: {1} ms, WS: {2} mb", treesCount, sw.ElapsedMilliseconds, Environment.WorkingSet / mb);
		}
	}
}