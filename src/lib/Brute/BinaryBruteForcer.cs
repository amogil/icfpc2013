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
		private byte[] outsideFoldOperations;
		private byte[] noFoldOperations;
		private byte[] inFoldOperations;
		private bool tFold;

		public BinaryBruteForcer(params string[] ops)
		{
			tFold = ops.Contains("tfold");
			outsideFoldOperations = new byte[] { 0, 1, 2 }.Concat(
				ops.Join(Operations.names.Select((s, i) => Tuple.Create(s, i)), i => i, o => o.Item1, (inner, outer) => (byte)outer.Item2))
			                                              .ToArray();
			noFoldOperations = outsideFoldOperations.Where(o => o != 6).ToArray();
			inFoldOperations = new byte[] { 3, 4 }.Concat(noFoldOperations).ToArray();
		}

		public IEnumerable<byte[]> Enumerate(int size)
		{
			return Enumerate(size, 1, outsideFoldOperations, new byte[30], 0);
		}

		
		private IEnumerable<byte[]> Enumerate(
			int size, int freePlaces, 
			byte[] operations,
			byte[] prefix, int prefixSize, int foldPosition = 0, int foldPlaces = 0)
		{
			const int outsideFold = 0;
			const int insideFold = 1;
			const int insideFoldFunc = 2;

			if (size < 0 || freePlaces < 0)
			{
				Console.WriteLine("should not be!");
				yield break;
			}
			if (size == 0 && freePlaces == 0)
			{
				var res = new byte[prefixSize];
				Array.Copy(prefix, res, prefixSize);
//				res.ToSExpr();
//				Debug.Assert(res.Count(b => b == 6) <= 1);
				yield return res;
			}
			else
			{
				var newOperations = operations;
				if (foldPosition == insideFold && freePlaces == foldPlaces)
				{
					foldPosition = insideFoldFunc;
					newOperations = inFoldOperations;
				}
				else if (foldPosition == insideFoldFunc && freePlaces == foldPlaces-1)
				{
					foldPosition = outsideFold;
					newOperations = noFoldOperations;
				}
				foreach (byte opIndex in newOperations)
				{
					Operation op = Operations.all[opIndex];
					if (freePlaces - 1 + op.argsCount == 0 && op.size != size) continue;
					if (freePlaces - 1 + op.argsCount > size - op.size) continue;

					var recOperations = newOperations;
					if (foldPosition == insideFold && freePlaces == foldPlaces)
					{
						foldPosition = insideFoldFunc;
						recOperations= inFoldOperations;
					}
					else if (foldPosition == insideFoldFunc && freePlaces == foldPlaces - 1)
					{
						foldPosition = outsideFold;
						recOperations = noFoldOperations;
					} 
					if (opIndex == 6)
					{
						foldPosition = insideFold;
						foldPlaces = freePlaces;
					}
					prefix[prefixSize] = opIndex;
					foreach (var expr in 
						Enumerate(
							size - op.size, 
							freePlaces - 1 + op.argsCount,
							opIndex == 6 ? noFoldOperations : recOperations,
							prefix, prefixSize + 1, 
							foldPosition, foldPlaces))
						yield return expr;
					if (opIndex == 6)
					{
						foldPosition = outsideFold;
						foldPlaces = freePlaces;
					}
				}
			}
		}
	}

	[TestFixture]
	public class BinaryBruteForcer_Test
	{
		[TestCase(5, "shr4 if0", 111)]
		[TestCase(3, "fold not", 3)]
		[TestCase(5, "fold not", 48)]
		[TestCase(8, "or not fold", 10407)]
		[TestCase(9, "or fold", 14427)]
		[TestCase(9, "not shl1 fold", 25968)]
		[TestCase(10, "fold not shr1 shr4", 671409)]
		[TestCase(10, "fold not or shr1 shr4", 5465043)]
		public void TestAll(int size, string ops, int expectedCount)
		{
			var force = new BinaryBruteForcer(ops.Split(' '));
			Stopwatch sw = Stopwatch.StartNew();
			var all = force.Enumerate(size);
			Console.WriteLine("new calculated");
			Assert.AreEqual(expectedCount, all.Count());
			Console.WriteLine(sw.Elapsed);
		}

		[Test]
		public void Test()
		{
			IEnumerable<byte[]> items = new BinaryBruteForcer("xor", "not").Enumerate(13);
			Console.WriteLine(items.Count());
//			foreach (var item in items)
//			{
//				Console.WriteLine(item.Printable());
//			}
		}
	}
}