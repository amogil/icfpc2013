using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using lib.Lang;
using lib.Web;

namespace lib.Brute
{
	[TestFixture]
	public class SmartGenerator_Test
	{
		[Test]
		public void Test()
		{
			foreach (var t in new SmartGenerator("if0").Enumerate(7))
			{
				Console.WriteLine(t.Printable());
			}
		}

		[TestCase("Id: Q8CaRCOpwfImQx4ZhFTW3b82, Challenge: (lambda (x_3842) (or (shr4 x_3842) 1)), Size: 5, Operators: or,shr4", TestName = "5")]
		[TestCase("Id: ErkXhpfABEVgAziRBgNyAAl7, Challenge: (lambda (x_11363) (shr1 (if0 (and (shr16 (shr4 x_11363)) (shr4 0)) x_11363 x_11363))), Size: 11, Operators: and,if0,shr1,shr16,shr4")]
		[TestCase("Id: h8tm9Awt9fVxIpGAyJ49eA4J, Challenge: (lambda (x_10772) (shl1 (if0 (or (not (shl1 0)) (shr4 x_10772)) 0 x_10772))), Size: 11, Operators: if0,not,or,shl1,shr4")]
		[TestCase("Id: AYTI87tdqN0qYuvM50gpb5UU, Challenge: (lambda (x_11454) (shr16 (if0 (and (shr4 (shr16 (shr1 x_11454))) x_11454) 1 x_11454))), Size: 11, Operators: and,if0,shr1,shr16,shr4")]
		[TestCase("Id: tP03Ueo4RbjUr7rTS5fP15Ym, Challenge: (lambda (x_10478) (shr4 (if0 (and (not (shr16 (shl1 1))) x_10478) 1 x_10478))), Size: 11, Operators: and,if0,not,shl1,shr16,shr4")]
		[TestCase("Id: 8xejannY4A6qJ4Jnq1AW3F1C, Challenge: (lambda (x_11226) (not (if0 (and (plus (shl1 x_11226) x_11226) x_11226) 0 x_11226))), Size: 11, Operators: and,if0,not,plus,shl1")]
		[TestCase("Id: sSV8yJ0TfPaiUMWDBT5vHAKN, Challenge: (lambda (x_10869) (shl1 (if0 (and (or x_10869 (shr16 x_10869)) x_10869) x_10869 x_10869))), Size: 11, Operators: and,if0,or,shl1,shr16")]
		[TestCase("Id: BEBwd0AADByfBOYfXG8JH23E, Challenge: (lambda (x_13034) (and (if0 (xor (shr16 (not (not x_13034))) 1) 0 1) x_13034)), Size: 12, Operators: and,if0,not,shr16,xor",
			TestName = "12 1")]
		[TestCase("Id: lHObnL0KiFVC3c8vPkDDzAUc, Challenge: (lambda (x_12074) (shr4 (shr4 (if0 (or (shr16 x_12074) 1) (shl1 (shr1 x_12074)) x_12074)))), Size: 12, Operators: if0,or,shl1,shr1,shr16,shr4",
			TestName = "12 2")]
		[TestCase("Id: lHObnL0KiFVC3c8vPkDDzAUc, Challenge: (lambda (x_12074) (shr4 (shr4 (if0 (or (shr16 x_12074) 1) (shl1 (shr1 x_12074)) x_12074)))), Size: 12, Operators: if0,or,shl1,shr1,shr16,shr4",
			TestName = "12 2")]
		[TestCase("Id: DOwDVsOZZdTXvcDBF0qjYg0f, Challenge: (lambda (x_22843) (shr1 (shr4 (fold x_22843 1 (lambda (x_22844 x_22845) (or (plus x_22844 x_22845) (shr4 x_22845))))))), Size: 13, Operators: fold,or,plus,shr1,shr4",
			TestName = "13")]
		public void MeasureMaskOptimization(string line)
		{
			var trainProblem = TrainResponse.Parse(line);
			byte[] p = Parser.ParseFunction(trainProblem.challenge);
			var ans = p.Printable();

			var operations = trainProblem.operators;
//			var random = new Random();
//			var answers = Enumerable.Range(0, 100).Select(i => random.NextUInt64()).Select(p.Eval).ToArray();
//			var mask = new Mask(answers);
//			Console.WriteLine("          answers mask: {0}", mask);
//			Console.WriteLine("challenge program mask: {0}", p.GetMask());
			Console.WriteLine("answer: {0}", ans);

			var sw = Stopwatch.StartNew();
			var result = new SmartGenerator(operations).Enumerate(trainProblem.size - 1).Select(f => f.Printable())
//				.Print(100)
				;
			Assert.IsTrue(result.Any(found => found == ans));
			Console.WriteLine(sw.Elapsed);
			sw.Restart();
			Assert.IsTrue(
				new BinaryBruteForcer(operations).Enumerate(trainProblem.size - 1)
												 .Any(found => found.Printable() == ans));
			Console.WriteLine(sw.Elapsed);
		}
	}

	public class SmartGenerator
	{
		private readonly byte[] outsideFoldOperations;
		private readonly byte[] noFoldOperations;
		private readonly byte[] inFoldOperations;
		private bool tFold;

		public SmartGenerator(params string[] ops)
		{
			tFold = ops.Contains("tfold");
			outsideFoldOperations =
				new byte[] {0, 1, 2}.Concat(
					ops.Where(t => t != "tfold").Select(o => (byte) Array.IndexOf(Operations.names, o))).ToArray();
			noFoldOperations = outsideFoldOperations.Where(o => o != 6).ToArray();
			inFoldOperations = new byte[] {3, 4}.Concat(noFoldOperations).ToArray();
		}
		
		public IEnumerable<byte[]> Enumerate(int size)
		{
			byte[] buffer = new byte[30];
			var unusedOpsToSpend = outsideFoldOperations.Sum(o => Operations.all[o].size + Operations.all[o].argsCount - 1);
			foreach (Subtree subtree in EnumerateSubtrees(false, size, buffer, 0, outsideFoldOperations, unusedOpsToSpend, 0).Select(t => t.subtree))
				yield return subtree.ToArray();
		}

		public class EnumerationItem
		{
			public EnumerationItem(Subtree subtree, int unusedSpent, int usedOps)
			{
				this.subtree = subtree;
				this.usedOps = usedOps;
				this.unusedSpent = unusedSpent;
			}

			public Subtree subtree;
			public int usedOps;
			public int unusedSpent;
		}

		public IEnumerable<EnumerationItem> EnumerateSubtrees(bool acceptSmallerSizes, int size, byte[] prefix, int prefixSize, byte[] operations, int unusedOpsToSpend, int usedOps)
		{
			if (size == 0) throw new Exception("should not be");
			if (unusedOpsToSpend > size) yield break; //не истратить столько

			unusedOpsToSpend = Math.Max(0, unusedOpsToSpend);


			foreach (var opIndex in operations)
			{
				Operation op = Operations.all[opIndex];
				if (op.size + op.argsCount > size) continue; //слишком жирная операция
				if (!acceptSmallerSizes && op.argsCount == 0 && size > 1) continue; //константа слишком мелкая
				prefix[prefixSize] = opIndex;
				var newUsedOps = usedOps | (1 << opIndex);
				var unusedSpent = (newUsedOps == usedOps) ? 0 : (op.size - 1 + op.argsCount);
				var newUnusedToSpent = unusedOpsToSpend - unusedSpent;
				if (op.argsCount == 0)
				{
					Debug.Assert(size == 1 || acceptSmallerSizes);
					if (unusedSpent >= unusedOpsToSpend - (size - 1))
						yield return new EnumerationItem(new Subtree(prefix, prefixSize, prefixSize), unusedSpent, newUsedOps);
				}
				else if (op.argsCount == 1)
				{
					//if (prefixSize == 3 && prefix[0] == 12 && prefix[1] == Operations.If && prefix[2] == 14) Debugger.Break();

					foreach (var subtree in EnumerableUnary(acceptSmallerSizes, size, prefix, prefixSize, operations, op, newUnusedToSpent, unusedSpent, newUsedOps)) yield return Check(subtree, unusedOpsToSpend, size);
				}
				else if (op.argsCount == 2)
				{
					foreach (var subtree in EnumerableBinary(acceptSmallerSizes, size, prefix, prefixSize, operations, op, newUnusedToSpent, unusedSpent, newUsedOps)) yield return Check(subtree, unusedOpsToSpend, size);
				}
				else if (opIndex == Operations.If)
					foreach (var subtree in EnumerateIf(acceptSmallerSizes, size, prefix, prefixSize, operations, newUnusedToSpent, unusedSpent, newUsedOps)) yield return Check(subtree, unusedOpsToSpend, size);
				else if (opIndex == Operations.Fold)
					foreach (var subtree in EnumerateFold(acceptSmallerSizes, size, prefix, prefixSize, newUnusedToSpent, unusedSpent, newUsedOps)) yield return Check(subtree, unusedOpsToSpend, size);
				else throw new NotImplementedException();
			}
		}

		public EnumerationItem Check(EnumerationItem item, int shouldSpend, int requestedSize)
		{
			if (item.unusedSpent < shouldSpend - (requestedSize - item.subtree.Size))
				throw new Exception(item.subtree.ToArray().Printable());
			return item;
		}

		private IEnumerable<EnumerationItem> EnumerateFold(bool acceptSmallerSizes, int size, byte[] prefix, int prefixSize, int unusedOpsToSpend, int unusedSpent, int usedOps)
		{
			foreach (var e1 in EnumerateSubtrees(true, size - 4, prefix, prefixSize + 1, noFoldOperations, unusedOpsToSpend, usedOps))
			{
				var leftSize = size - 2 - e1.subtree.Size;
				Debug.Assert(leftSize >= 2);
				foreach (var e2 in EnumerateSubtrees(true, leftSize - 1, prefix, prefixSize + 1 + e1.subtree.Len, noFoldOperations, unusedOpsToSpend - e1.unusedSpent, e1.usedOps))
				{
					var e3Size = leftSize - e2.subtree.Size;
					Debug.Assert(e3Size >= 1);
					var spent = e1.unusedSpent + e2.unusedSpent;
					foreach (var e3 in EnumerateSubtrees(acceptSmallerSizes, e3Size, prefix, prefixSize + 1 + e1.subtree.Len + e2.subtree.Len, inFoldOperations, unusedOpsToSpend - spent, e2.usedOps))
					{
						yield return new EnumerationItem(new Subtree(prefix, prefixSize, e3.subtree.Last), unusedSpent + spent + e3.unusedSpent, e3.usedOps);
					}
				}
			}
		}

		private IEnumerable<EnumerationItem> EnumerateIf(bool acceptSmallerSizes, int size, byte[] prefix, int prefixSize, byte[] operations, int unusedOpsToSpend, int unusedSpent, int usedOps)
		{
			foreach (var cond in EnumerateSubtrees(true, size - 3, prefix, prefixSize + 1, operations, unusedOpsToSpend, usedOps))
			{
				var leftSize = size - 1 - cond.subtree.Size;
				Debug.Assert(leftSize >= 2);
				foreach (var zeroExp in EnumerateSubtrees(true, leftSize - 1, prefix, prefixSize + 1 + cond.subtree.Len, operations, unusedOpsToSpend - cond.unusedSpent, cond.usedOps))
				{
					var elseSize = leftSize - zeroExp.subtree.Size;
					Debug.Assert(elseSize >= 1);
					var spent = cond.unusedSpent + zeroExp.unusedSpent;
					foreach (var elseExp in EnumerateSubtrees(acceptSmallerSizes, elseSize, prefix, prefixSize + 1 + cond.subtree.Len + zeroExp.subtree.Len, operations, unusedOpsToSpend - spent, zeroExp.usedOps))
					{
						yield return new EnumerationItem(new Subtree(prefix, prefixSize, elseExp.subtree.Last), unusedSpent + spent + elseExp.unusedSpent , elseExp.usedOps);
					}
				}
			}
		}

		private IEnumerable<EnumerationItem> EnumerableUnary(bool acceptSmallerSizes, int size, byte[] prefix, int prefixSize, byte[] operations, Operation op, int unusedOpsToSpend, int unusedSpent, int usedOps)
		{
			foreach (var item in EnumerateSubtrees(acceptSmallerSizes, size - op.size, prefix, prefixSize + 1, operations, unusedOpsToSpend, usedOps))
				yield return new EnumerationItem(new Subtree(prefix, prefixSize, item.subtree.Last), item.unusedSpent + unusedSpent, item.usedOps);
		}

		private IEnumerable<EnumerationItem> EnumerableBinary(bool acceptSmallerSizes, int size, byte[] prefix, int prefixSize, byte[] operations, Operation op, int unusedOpsToSpend, int unusedSpent, int usedOps)
		{
			foreach (var arg0 in EnumerateSubtrees(true, size - op.size - 1, prefix, prefixSize + 1, operations, unusedOpsToSpend, usedOps))
			{
				var leftSize = size - op.size - arg0.subtree.Size;
				Debug.Assert(leftSize >= 1);
				//TODO: symm optimization
				foreach (var arg1 in EnumerateSubtrees(acceptSmallerSizes, leftSize, prefix, prefixSize + arg0.subtree.Len + 1, operations, unusedOpsToSpend - arg0.unusedSpent, arg0.usedOps))
				{
					yield return new EnumerationItem(new Subtree(prefix, prefixSize, arg1.subtree.Last), unusedSpent + arg0.unusedSpent + arg1.unusedSpent, arg1.usedOps);
				}
			}
		}


	}
}