using System;
using System.IO;
using System.Linq;
using lib.AlphaProtocol;

namespace AlphaProtocolExecutor
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var problems = File
				.ReadAllLines(@"../../../../problems.txt")
				.Select(l => l.Trim())
				.Where(l => l.Length > 0)
				.Select(l => l.Split('\t'))
				.Select(elms => new { Id = elms[0], Size = elms[1], Solved = elms[2], Bonus = elms[3], Operations = elms[4].Split(',').Select(t => t.Trim()).ToArray() })
				.Select(d => new
				{
					Id = d.Id,
					Size = int.Parse(d.Size),
					Tried = d.Solved.Length > 0,
					Solved = d.Solved.Equals("True", StringComparison.OrdinalIgnoreCase),
					Bonus = d.Bonus.Equals("Bonus", StringComparison.OrdinalIgnoreCase),
					Operations = d.Operations,
				})
				.ToList();
			var problemsToSolve = problems.Where(p => !p.Tried && !p.Bonus && p.Size == 7).ToArray();
			foreach (var problem in problemsToSolve)
			{
				AlphaProtocol.PostSolution(problem.Id, problem.Size, problem.Operations);
			}
            Console.WriteLine("Press any key...");
            Console.ReadKey();
		}
	}
}