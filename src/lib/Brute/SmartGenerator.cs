using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using lib.Lang;

namespace lib.Brute
{
	public class SmartGenerator
	{
		private readonly Mask answersMask;
		private readonly byte[] outsideFoldOperations;
		private readonly byte[] noFoldOperations;
		private readonly byte[] inFoldOperations;
		private readonly bool tFold;

		public SmartGenerator(params string[] ops)
			:this(null, null, ops)
		{
		}

		public SmartGenerator(ulong[] inputs, ulong[] outputs, params string[] ops)
		{
			if (outputs != null)
			{
				answersMask = new Mask(outputs);
				filterInput = inputs.Last();
				filterOutput = outputs.Last();
			}
			tFold = ops.Contains("tfold");
			outsideFoldOperations =
				new byte[] {0, 1, 2}.Concat(
					ops.Where(t => t != "tfold").Select(o => (byte) Array.IndexOf(Operations.names, o)))
					.OrderBy(opIndex => Operations.all[opIndex].priority).ToArray();
			noFoldOperations = outsideFoldOperations.Where(o => o != 6)
				.OrderBy(opIndex => Operations.all[opIndex].priority).ToArray();
			inFoldOperations = new byte[] {3, 4}.Concat(noFoldOperations)
				.OrderBy(opIndex => Operations.all[opIndex].priority).ToArray();
		}

		public IEnumerable<byte[]> Enumerate(int size)
		{
			return EnumerateItems(size).Select(i => i.subtree.ToArray());
		}

		public IEnumerable<EnumerationItem> EnumerateItems(int size)
		{
			byte[] buffer = new byte[30];
			var unusedOpsToSpend = outsideFoldOperations.Sum(o => Operations.all[o].size + Operations.all[o].argsCount - 1);
			if (tFold)
			{
				buffer[0] = Operations.Fold;
				return EnumerateFold(size, size, buffer, 0, unusedOpsToSpend, 0, 0);
				//.Select(i => new EnumerationItem(new Subtree(i.subtree.Buffer, 0, i.subtree.Last), i.unusedSpent, i.usedOps));
			}
			else return EnumerateSubtrees(size, size, buffer, 0, outsideFoldOperations, unusedOpsToSpend, 0);
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

		public ulong? filterInput;
		public ulong? filterOutput;

		public IEnumerable<EnumerationItem> EnumerateSubtrees(int minSize, int maxSize, byte[] prefix, int prefixSize, byte[] operations, int unusedOpsToSpend, int usedOps)
		{
			if (maxSize == 0) throw new Exception("should not be");
			if (unusedOpsToSpend > maxSize) yield break; //не истратить столько
			minSize = Math.Min(minSize, maxSize);

			if (answersMask != null && prefixSize > 0 && prefixSize> 0 && maxSize > 3)
			{
				var mask = prefix.GetMask(0, prefixSize - 1);
				if(!answersMask.IncludedIn(mask))
				{
					yield break;
				}
				var maskWithInputValue = prefix.GetMask(filterInput.Value, 0, prefixSize - 1);
				if (!new Mask(new[] {filterOutput.Value}).IncludedIn(maskWithInputValue))
				{
					yield break;
				}
			}

			unusedOpsToSpend = Math.Max(0, unusedOpsToSpend);
			foreach (var opIndex in operations)
			{
				Operation op = Operations.all[opIndex];
				if (op.size + op.argsCount > maxSize) continue; //слишком жирная операция
				if (op.argsCount == 0 && minSize > 1) continue; //константа слишком мелкая
				prefix[prefixSize] = opIndex;
				var newUsedOps = usedOps | (1 << opIndex);
				var unusedSpent = (newUsedOps == usedOps) ? 0 : (op.size - 1 + op.argsCount);
				var newUnusedToSpent = unusedOpsToSpend - unusedSpent;
				if (op.argsCount == 0)
				{
					if (unusedSpent >= unusedOpsToSpend - (maxSize - 1))
						yield return new EnumerationItem(new Subtree(prefix, prefixSize, prefixSize), unusedSpent, newUsedOps);
				}
				else if (op.argsCount == 1)
					foreach (var subtree in EnumerableUnary(minSize, maxSize, prefix, prefixSize, operations, op, newUnusedToSpent, unusedSpent, newUsedOps)) yield return subtree;
				else if (op.argsCount == 2)
					foreach (var subtree in EnumerableBinary(minSize, maxSize, prefix, prefixSize, operations, op, newUnusedToSpent, unusedSpent, newUsedOps)) yield return subtree;
				else if (opIndex == Operations.If)
					foreach (var subtree in EnumerateIf(minSize, maxSize, prefix, prefixSize, operations, newUnusedToSpent, unusedSpent, newUsedOps)) yield return subtree;
				else if (opIndex == Operations.Fold)
					foreach (var subtree in EnumerateFold(minSize, maxSize, prefix, prefixSize, newUnusedToSpent, unusedSpent, newUsedOps)) yield return subtree;
				else throw new NotImplementedException(op.Name);
			}
		}

		private IEnumerable<EnumerationItem> EnumerableUnary(int minSize, int maxSize, byte[] prefix, int prefixSize, byte[] operations, Operation op, int unusedOpsToSpend, int unusedSpent, int usedOps)
		{
			foreach (var item in EnumerateSubtrees(minSize - op.size, maxSize - op.size, prefix, prefixSize + 1, operations, unusedOpsToSpend, usedOps))
				yield return new EnumerationItem(new Subtree(prefix, prefixSize, item.subtree.Last), item.unusedSpent + unusedSpent, item.usedOps);
		}

		private IEnumerable<EnumerationItem> EnumerableBinary(int minSize, int maxSize, byte[] prefix, int prefixSize, byte[] operations, Operation op, int unusedOpsToSpend, int unusedSpent, int usedOps)
		{
			var totalArgsSize = maxSize - op.size;
			var minLeftTree = (minSize - op.size)/2;

			foreach (var arg0 in EnumerateSubtrees(minLeftTree, totalArgsSize - 1, prefix, prefixSize + 1, operations, unusedOpsToSpend, usedOps))
			{
				var withFirstArgMask = prefix.GetMask(arg0.subtree.First - 1, arg0.subtree.Last);
				var singleSecond = withFirstArgMask.IsConstant();
				
				var leftMaxSize = maxSize - op.size - arg0.subtree.Size;
				var leftMinSize = minSize - op.size - arg0.subtree.Size;
				Debug.Assert(leftMaxSize >= 1);
				//TODO: symm optimization
				foreach (var arg1 in EnumerateSubtrees(leftMinSize, leftMaxSize, prefix, prefixSize + arg0.subtree.Len + 1, operations, unusedOpsToSpend - arg0.unusedSpent, arg0.usedOps))
				{
					yield return new EnumerationItem(new Subtree(prefix, prefixSize, arg1.subtree.Last), unusedSpent + arg0.unusedSpent + arg1.unusedSpent, arg1.usedOps);
					if (singleSecond) 
						break;
				}
			}
		}
		
		private IEnumerable<EnumerationItem> EnumerateIf(int minSize, int maxSize, byte[] prefix, int prefixSize, byte[] operations, int unusedOpsToSpend, int unusedSpent, int usedOps)
		{
			foreach (var cond in EnumerateSubtrees(1, maxSize - 3, prefix, prefixSize + 1, operations, unusedOpsToSpend, usedOps))
			{
				var condMask = cond.subtree.GetMask();
				var singleZero = condMask.CantBeZero();
				var singleElse = condMask.IsZero();
				var leftMaxSize = maxSize - 1 - cond.subtree.Size;
				var leftMinSize = minSize - 1 - cond.subtree.Size;
				Debug.Assert(leftMaxSize >= 2);
				foreach (var enumerationItem in EnumerationIfBranches(leftMinSize, leftMaxSize, prefix, prefixSize, operations, cond, unusedOpsToSpend, unusedSpent, singleElse, singleZero)) 
					yield return enumerationItem;
			}
		}

		private IEnumerable<EnumerationItem> EnumerationIfBranches(int minSize, int maxSize, byte[] prefix, int prefixSize, byte[] operations, EnumerationItem cond, int unusedOpsToSpend, int unusedSpent, bool singleElse, bool singleZero)
		{
			foreach (var zeroExp in EnumerateSubtrees(1, maxSize - 1, prefix, prefixSize + 1 + cond.subtree.Len, operations, unusedOpsToSpend - cond.unusedSpent, cond.usedOps))
			{
				var elseMaxSize = maxSize - zeroExp.subtree.Size;
				var elseMinSize = minSize - zeroExp.subtree.Size;
				Debug.Assert(elseMaxSize >= 1);
				var spent = cond.unusedSpent + zeroExp.unusedSpent;
				bool something = false;
				foreach (var elseExp in EnumerateSubtrees(elseMinSize, elseMaxSize, prefix, prefixSize + 1 + cond.subtree.Len + zeroExp.subtree.Len, operations, unusedOpsToSpend - spent, zeroExp.usedOps))
				{
					yield return new EnumerationItem(new Subtree(prefix, prefixSize, elseExp.subtree.Last), unusedSpent + spent + elseExp.unusedSpent, elseExp.usedOps);
					if (singleElse) yield break; // yield break;
					something = true;
				}
				if (something && singleZero) yield break; // yield break;
			}
		}

		private IEnumerable<EnumerationItem> EnumerateFold(int minSize, int maxSize, byte[] prefix, int prefixSize, int unusedOpsToSpend, int unusedSpent, int usedOps)
		{
			foreach (var e1 in EnumerateSubtrees(1, maxSize - 4, prefix, prefixSize + 1, noFoldOperations, unusedOpsToSpend, usedOps))
			{
				var leftMaxSize = maxSize - 2 - e1.subtree.Size;
				var leftMinSize = minSize - 2 - e1.subtree.Size;
				Debug.Assert(leftMaxSize >= 2);
				foreach (var e2 in EnumerateSubtrees(1, leftMaxSize - 1, prefix, prefixSize + 1 + e1.subtree.Len, noFoldOperations, unusedOpsToSpend - e1.unusedSpent, e1.usedOps))
				{
					var e3MaxSize = leftMaxSize - e2.subtree.Size;
					var e3MinSize = leftMinSize - e2.subtree.Size;
					Debug.Assert(e3MaxSize >= 1);
					var spent = e1.unusedSpent + e2.unusedSpent;
					foreach (var e3 in EnumerateSubtrees(e3MinSize, e3MaxSize, prefix, prefixSize + 1 + e1.subtree.Len + e2.subtree.Len, inFoldOperations, unusedOpsToSpend - spent, e2.usedOps))
					{
						yield return new EnumerationItem(new Subtree(prefix, prefixSize, e3.subtree.Last), unusedSpent + spent + e3.unusedSpent, e3.usedOps);
					}
				}
			}
		}



	}
}