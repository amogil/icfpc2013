using System;
using System.Collections.Generic;
using System.Linq;

namespace lib.Lang
{
	public static class Extensions
	{
		public static IEnumerable<string> Print(this IEnumerable<string> items, int maxCount = 1000)
		{
			return items.Print(s => s);

		}
		public static void DumpAll(this IEnumerable<byte[]> items, int maxCount = 1000)
		{
			items.Print(t => t.Printable()).ToList();

		}
		public static IEnumerable<T> Print<T>(this IEnumerable<T> items, Func<T, string> toString, int maxCount = 1000)
		{
			int printedCount = 0;
			foreach (var item in items)
			{
				if (printedCount <= maxCount)
				{
					Console.WriteLine(toString(item));
					printedCount++;
				}
				yield return item;
			}
		}

		public static IEnumerable<string> AlternateWith(this IEnumerable<string> items, string alternator)
		{
			return items.SelectMany((s, i) => i == 0 ? new[]{s} : new[]{alternator, s});
		}

		public static IEnumerator<T> MoveNextOrFail<T>(this IEnumerator<T> tokens)
		{
			if (!tokens.MoveNext()) throw new FormatException("unexpected end of tokens");
			return tokens;
		}

		public static T ExtractToken<T>(this IEnumerator<T> tokens)
		{
			var t = tokens.Current;
			tokens.MoveNextOrFail();
			return t;
		}
		
		public static IEnumerator<T> SkipToken<T>(this IEnumerator<T> tokens, T expectedToken)
		{
			if (!expectedToken.Equals(tokens.Current)) throw new FormatException(string.Format("Expected {0} but was {1}", expectedToken, tokens.Current));
			return tokens.MoveNextOrFail();
		}
	}
}