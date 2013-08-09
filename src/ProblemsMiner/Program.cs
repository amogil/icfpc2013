using System;

namespace ProblemsMiner
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                PrintUsage();
                return;
            }
            var problemsMiner = new ProblemsMiner(args[0], args[1]);
            problemsMiner.DownloadProblemsSamples(Int32.MaxValue);
        }

        private static void PrintUsage()
        {
            Console.WriteLine("ProblemsMiner.exe <path to problems file> <dir for problems samples files>");
            Console.WriteLine("CTRL-C to stop this madness...");
        }
    }
}