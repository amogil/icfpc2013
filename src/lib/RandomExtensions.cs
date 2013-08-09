using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace lib
{
	public static class RandomExtensions
	{
		public static bool NextBool([NotNull] this Random random)
		{
			return random.Next() % 2 == 0;
		}

		public static byte NextByte([NotNull] this Random random)
		{
			return (byte)random.Next();
		}

		public static uint NextUint([NotNull] this Random random)
		{
			return (uint)random.Next();
		}

		public static ushort NextUshort([NotNull] this Random random)
		{
			return (ushort)random.Next(ushort.MinValue, ushort.MaxValue);
		}

		public static long NextLong([NotNull] this Random random)
		{
			var highBits = ((long)random.Next()) << 32;
			var lowBits = (long)random.Next();
			return highBits + lowBits;
		}

		public static ulong NextUlong([NotNull] this Random random)
		{
			var highBits = ((ulong)random.Next()) << 32;
			var lowBits = (ulong)random.Next();
			return highBits + lowBits;
		}

		[NotNull]
		public static byte[] NextBytes([NotNull] this Random random, int length)
		{
			var buf = new byte[length];
			random.NextBytes(buf);
			return buf;
		}

		public static T NextItem<T>([NotNull] this Random random, [NotNull] IList<T> items)
		{
			return items[random.Next(items.Count)];
		}

		public static void Shuffle<T>([NotNull] this Random random, [NotNull] IList<T> items)
		{
			for (var i = 0; i < items.Count - 1; i++)
			{
				var j = random.Next(i + 1, items.Count);
				var tmp = items[i];
				items[i] = items[j];
				items[j] = tmp;
			}
		}
	}
}