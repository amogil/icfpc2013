using System;
using System.Collections.Generic;
using System.Linq;
using lib.Lang;

namespace lib.Brute
{
	public class BinaryBruteForcer
	{
		private readonly Mask answersMask;
		private readonly int a;
		private readonly int b;
		private readonly byte[] outsideFoldOperations;
		private readonly byte[] noFoldOperations;
		private readonly byte[] inFoldOperations;
		private bool tFold;
		private const int outsideFold = 0;
		private const int insideFold = 1;
		private const int insideFoldFunc = 2;

		public BinaryBruteForcer(params string[] ops)
			: this(null, 1, 1, ops)
		{

		}

		public BinaryBruteForcer(Mask answersMask, params string[] ops)
			: this(answersMask, 3, 1, ops)
		{
		}

		public BinaryBruteForcer(Mask answersMask, int a, int b, params string[] ops)
		{
			this.answersMask = answersMask;
			this.a = a;
			this.b = b;
			tFold = ops.Contains("tfold");
			outsideFoldOperations =
				new byte[] {0, 1, 2}.Concat(
					ops.Where(t => t != "tfold").Select(o => (byte) Array.IndexOf(Operations.names, o))).ToArray();
			noFoldOperations = outsideFoldOperations.Where(o => o != 6).ToArray();
			inFoldOperations = new byte[] {3, 4}.Concat(noFoldOperations).ToArray();
		}

		public IEnumerable<byte[]> EnumerateBonus(int size)
		{
			var prefix = new byte[30];
			prefix[0] = 5;
			prefix[1] = 12;
			prefix[2] = 1;
			return Enumerate(size, 3, outsideFoldOperations, prefix, 3, 0, 0, (1 << 1) | (1 << 5) | (1 << 12));
		}

		public IEnumerable<byte[]> Enumerate(int size)
		{
			if (tFold)
			{
				var prefix = new byte[30];
				prefix[0] = 6;
				var prefixSize = 1;
				return Enumerate(size - 2, 3, outsideFoldOperations, prefix, prefixSize, insideFold, 1);
			}
			else return Enumerate(size, 1, outsideFoldOperations, new byte[30], 0);
		}

		private bool EnoughSizeForOps(byte[] operations, int size, int freePlaces, int usedOps)
		{
			int needSize = freePlaces +
						   operations.Where(opIndex => opIndex > 4 && (usedOps & (1 << opIndex)) == 0)
									 .Sum(
										 opIndex => Operations.all[opIndex].size + Operations.all[opIndex].argsCount - 1);
			return needSize <= size;
		}

		private IEnumerable<byte[]> Enumerate(
			int size, int freePlaces,
			byte[] operations,
			byte[] prefix, int prefixSize, int foldPosition = 0, int foldPlaces = 0, int usedOps = 0)
		{
			if (size == 0 && freePlaces == 0)
			{
				var res = new byte[prefixSize];
				Array.Copy(prefix, res, prefixSize);
				yield return res;
			}
			else
			{
				if (!EnoughSizeForOps(operations, size, freePlaces, usedOps)) yield break;
				if (answersMask != null && prefixSize > 0 && (prefixSize + b) % a == 0 
					&& !answersMask.IncludedIn(prefix.GetMask(0, prefixSize - 1))) yield break;
				var newOperations = operations;
				if (foldPosition == insideFold && freePlaces == foldPlaces)
				{
					foldPosition = insideFoldFunc;
					newOperations = inFoldOperations;
				}
				else if (foldPosition == insideFoldFunc && freePlaces == foldPlaces - 1)
				{
					foldPosition = outsideFold;
					newOperations = noFoldOperations;
				}
				foreach (var opIndex in newOperations)
				{
					Operation op = Operations.all[opIndex];
					if (freePlaces - 1 + op.argsCount == 0 && op.size != size) continue;
					if (freePlaces - 1 + op.argsCount > size - op.size) continue;

					var recOperations = newOperations;
					if (foldPosition == insideFold && freePlaces == foldPlaces)
					{
						foldPosition = insideFoldFunc;
						recOperations = inFoldOperations;
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
							foldPosition, foldPlaces, usedOps | (1 << opIndex)))
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
}