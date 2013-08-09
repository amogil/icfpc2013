using System;

namespace lib.Guesser
{
    internal static class RandomExtensions
    {
        public static ulong NextUInt64(this Random random)
        {
            var buffer = new byte[sizeof (UInt64)];
            random.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }
    }
}