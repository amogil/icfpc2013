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
                .Select(
                    elms =>
                    new
                        {
                            Id = elms[0],
                            Size = elms[1],
                            Solved = elms[2],
                            Bonus = elms[3],
                            Operations = elms[4].Split(',').Select(t => t.Trim()).ToArray()
                        })
                .Select(d => new
                    {
                        d.Id,
                        Size = int.Parse(d.Size),
                        Tried = d.Solved.Length > 0,
                        Solved = d.Solved.Equals("True", StringComparison.OrdinalIgnoreCase),
                        Bonus = d.Bonus.Equals("Bonus", StringComparison.OrdinalIgnoreCase),
                        d.Operations,
                    })
                .ToList();

            var whitelist = new[]
                {
                    "0rDFjEiGtdYGifXNzi7IwCPU",
                };

            var problemsToSolve = problems.Where(p => !p.Tried && !p.Bonus && p.Size == 14).ToArray();
            if (whitelist.Any())
                problemsToSolve = problemsToSolve.Where(p => whitelist.Select(w => w == p.Id).Any()).ToArray();

            foreach (var problem in problemsToSolve.Take(5))
            {
                AlphaProtocol.PostSolution(problem.Id, problem.Size, problem.Operations);
            }

            // Ручной режим
            //            AlphaProtocol.PostSolution("GCDhHD6NjrWTpdg0EAnqthC", 12, "and,fold,not,plus,shl1".Split(','), true);
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }
    }
}