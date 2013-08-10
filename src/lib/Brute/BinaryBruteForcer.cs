using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using lib.Lang;

namespace lib.Brute
{


	public class BinaryBruteForcer
	{
		public IEnumerable<byte[]> Enumerate(int size, params string[] ops)
		{
			bool tFold = ops.Contains("tfold");
			byte[] operations =
				new byte[] { 0, 1, 2 }.Concat(
					ops.Join(Operations.names.Select((s, i) => Tuple.Create(s, i)), i => i, o => o.Item1, (inner, outer) => (byte)outer.Item2))
				.ToArray(); 
			byte[] inFoldOperations = new byte[] { 3, 4 }.Concat(operations).ToArray();
			return Enumerate(size, 1, operations, inFoldOperations, new byte[0]);
		}

		private IEnumerable<byte[]> Enumerate(int size, int freePlaces, byte[] operations, byte[] inFoldOperations, byte[] prefix)
		{
			if (size < 0 || freePlaces < 0)
			{
				Console.WriteLine("should not be!");
				yield break;
			}
			if (size == 0 && freePlaces == 0) yield return prefix;
			else
			foreach (byte op in operations)
			{
				if (freePlaces - 1 + Operations.args[op] == 0 && Operations.sizes[op] != size) continue; // слишком рано все закончилось
				if (freePlaces - 1 + Operations.args[op] > size - Operations.sizes[op]) continue;
				var newOperations = op == 6 ? inFoldOperations : operations;
				foreach (var expr in Enumerate(size - Operations.sizes[op], freePlaces - 1 + Operations.args[op], newOperations, inFoldOperations, prefix.Concat(new[] { op }).ToArray()))
					yield return expr;
			}
		}
	}

	[TestFixture]
	public class BinaryBruteForcer_Test
	{
		[TestCase(5, "shr4 if0", "(shr4 (if0 x x x))")]
		[TestCase(5, "shr4 if0", "(shr4 (if0 x x x))")]
		[TestCase(5, "shr4 if0", "(shr4 (if0 x x x))")]
		[TestCase(9, "or fold", "(fold x 0 (lambda (i a) (or (or i a) a)))")]
		[TestCase(10, "fold not or shr1 shr4", "(fold (shr1 (not x)) 0 (lambda (i a) (or i (shr4 a))))")]
		public void TestAll(int size, string ops, string expectedExpr)
		{
			var force = new BinaryBruteForcer();
			Stopwatch sw = Stopwatch.StartNew();
			var trees = force.Enumerate(size, ops.Split(' ')).ToList();
			Console.WriteLine(trees.Count());
			Console.WriteLine(sw.Elapsed);
			if (trees.All(tree => tree.ToSExpr().Item1 != expectedExpr))
			{
				foreach (var tree in trees)
				{
					Console.WriteLine(tree);
				}
				Assert.Fail();
			}
		}

		[Test]
		public void Test()
		{
			IEnumerable<byte[]> items = new BinaryBruteForcer().Enumerate(13, "xor", "not");
			Console.WriteLine(items.Count());
//			foreach (var item in items)
//			{
//				Console.WriteLine(item.Printable());
//			}
		}
	}
}