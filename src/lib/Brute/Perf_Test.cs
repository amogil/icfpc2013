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
			Console.Out.WriteLine("WS: {0} mb", Environment.WorkingSet / mb);

			var sw = Stopwatch.StartNew();
			var trees = getTrees(8, allOps).ToList();
			sw.Stop();
			Console.Out.WriteLine("#trees: {0}, gen took: {1} ms, WS: {2} mb", trees.Count, sw.ElapsedMilliseconds, Environment.WorkingSet / mb);

			var args = new List<TArg>();
			for (var i = 0; i < 4; i++)
				args.Add(createArg(rnd.NextUInt64()));

			Console.WriteLine("eval");
			sw.Restart();
			foreach (var arg in args)
				foreach (var tree in trees)
					eval(tree, arg);
			sw.Stop();
			Console.Out.WriteLine("#trees: {0}, eval took: {1} ms, WS: {2} mb", trees.Count, sw.ElapsedMilliseconds, Environment.WorkingSet / mb);
		}
	}
}