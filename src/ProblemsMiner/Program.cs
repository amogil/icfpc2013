using System;

namespace ProblemsMiner
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                PrintUsage();
                return;
            }
            var source = new ProblemsReader()
            {
                ProblemsFilename = args[0],
                TrainProblemsPath = args[1],
                TrainProblemsResultsPath = args[1],
                ProblemsResultsPath = args[2]
            };
            var problemsMiner = new ProblemsMiner(source);
            problemsMiner.DownloadTrainProblemsSamples(Int32.MaxValue);
        }

        private static void PrintUsage()
        {
            Console.WriteLine("ProblemsMiner.exe <path to problems file> <dir for problems samples files> <dir for real tast results data files>");
            Console.WriteLine("CTRL-C to stop this madness...");
        }
    }
}