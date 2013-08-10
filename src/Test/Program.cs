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
				int count = new BinaryBruteForcer(p.Operations).Enumerate(p.Size-1).Take(limit).Count();
				if (count == limit)
					Console.WriteLine(limit + "+");
				else
				{
					Console.WriteLine(count);
					File.AppendAllText(smallTxt, p.Id + "\t" + p.Size + "\r\n");
				}
			}
		}
	}
}