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
			var force = new BinaryBruteForcer();
			Run(force.Enumerate, arg => arg, (tree, arg) => tree.Eval(arg));
		}

		private void Run<TTree, TArg>(Func<int, string[], IEnumerable<TTree>> getTrees, Func<ulong, TArg> createArg, Func<TTree, TArg, ulong> eval)
		{
			const long mb = 1024 * 1024;
			var rnd = new Random();
			var unaryOps = Unary.Operators.Select(t => t.Key).ToArray();
			var binaryOps = Binary.Operators.Select(t => t.Key).ToArray();
			var allOps = unaryOps.Concat(binaryOps).ToArray();
			
			var args = new List<TArg>();
			const int argsCount = 2;
			for (var i = 0; i < argsCount; i++)
				args.Add(createArg(rnd.NextUInt64()));

			Console.Out.WriteLine("WS: {0} mb", Environment.WorkingSet / mb);
			long treeCount = 0;
			var sw = Stopwatch.StartNew();
			for (var iter = 0; iter < argsCount; iter++)
			{
				var arg = args[iter];
				foreach (var tree in getTrees(8, allOps))
				{
					eval(tree, arg);
					if (iter == 0)
						++treeCount;
				}
			}
			sw.Stop();
			Console.Out.WriteLine("#args: {0}, #trees: {1}, eval took: {2} ms, WS: {3} mb", argsCount, treeCount, sw.ElapsedMilliseconds, Environment.WorkingSet / mb);
		}
	}
}