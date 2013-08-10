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
                .ReadAllLines(@"C:\Users\pe\Documents\GitHub\icfpc2013\problems.txt")
                .Select(l => l.Trim())
                .Where(l => l.Length > 0)
                .Select(l => l.Split('\t'))
                .Select(elms => new { Id = elms[0], Size = elms[1], Solved = elms[2], Operations = String.Join(" ", elms.Skip(3).ToArray()) })
                .Select(d => new
                    {
                        Id = d.Id,
                        Size = int.Parse(d.Size),
                        Tried = d.Solved.Length > 0,
                        Solved = d.Solved.Length > 0 ? Convert.ToBoolean(d.Solved) : false,
                        Operations = d.Operations.Split(' ').Select(e => e.Trim()).Where(e => e.Length > 0).ToArray()
                    });
            var unTriedProblemsFixedSize = problems.Where(p => !p.Tried && p.Size == 4).ToArray();
            foreach (var problem in unTriedProblemsFixedSize)
            {
                Console.WriteLine("Press any key to start solve problem {0}", problem.Id);
                Console.ReadKey();
                AlphaProtocol.PostSolution(problem.Id, problem.Size, problem.Operations);
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            
        }
    }
}