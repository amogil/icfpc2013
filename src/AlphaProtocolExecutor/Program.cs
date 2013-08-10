using System;
using lib.AlphaProtocol;

namespace AlphaProtocolExecutor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string[] ops = "and,shr1,shr4,tfold".Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            AlphaProtocol.PostSolution("MghPQA7IofdvKnWo4VpqH3Xk", 10, ops);
            Console.ReadKey();
        }
    }
}