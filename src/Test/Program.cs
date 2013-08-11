using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using lib;
using lib.Brute;

namespace Test
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			FindCountTreesForTrainProblems(14, 18);
		}

        private static void FindCountTreesForMyProblems()
        {
            var smallTxt = "small.txt";

            var smallIds =
                File.Exists("small.txt") ? new HashSet<string>(File.ReadAllLines(smallTxt)
                    .Where(line => line.Trim().Length > 0)
                    .Select(line => line.Split('\t'))
                    .Select(p => p[0])) : new HashSet<string>();

            MyProblem[] prob = ProblemsReader.ProblemsToSolve().Where(p => p.Size >= 14 && !smallIds.Contains(p.Id)).OrderBy(p => p.Size).ToArray();
            int limit = 100000000;
            foreach (MyProblem p in prob)
            {
                Console.Write("{2} {0}: {1}  ", p.Id, string.Join(",", p.Operations).PadRight(40), p.Size);
                int count = new BinaryBruteForcer(p.Operations).Enumerate(p.Size - 1).Take(limit).Count();
                if (count == limit)
                    Console.WriteLine(limit + "+");
                else
                {
                    Console.WriteLine(count);
                    File.AppendAllText(smallTxt, p.Id + "\t" + p.Size + "\r\n");
                }
            }
        }


        private static void FindCountTreesForTrainProblems(int startSize, int endSize)
        {
            ProblemsReader reader = ProblemsReader.CreateDefault();
            string filename = @"..\..\..\..\problems-samples\sizetrees.txt";
            var problems = reader.ReadProblems().Where(p => p.Size >= startSize && p.Size <= endSize);
            var trainproblems = problems.SelectMany(p => reader.ReadTrainProblems(p, true).Concat(reader.ReadTrainProblems(p, false)));

            foreach (var p in trainproblems)
            {
                var t0 = DateTime.Now;
                int count = new BinaryBruteForcer(p.operators).Enumerate(p.size - 1).Count();
                var t1 = DateTime.Now;
                var time = t1 - t0;
                var str = String.Format("{0}\ttrees:{1}\ttime:{2}\tsize:{3}\t{4}\n", p.id, count, time, p.size, p.ToString());
                File.AppendAllText(filename, str);
            }
        }
	}
}