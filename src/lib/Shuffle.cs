using System;
using System.Collections.Generic;
using System.Linq;

namespace ProblemsMiner
{
    public static class ShuffleExtension
    {
        public static T[] Shuffle<T>(this IEnumerable<T> original)
        {
            var result = (T[]) original.ToArray().Clone();
            for (int n = result.Length - 1; n > 0; --n)
            {
                var r = new Random();
                int k = r.Next(n + 1);
                T temp = result[n];
                result[n] = result[k];
                result[k] = temp;
            }
            return result;
        }
    }
}