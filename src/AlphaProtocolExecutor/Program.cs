using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using lib.AlphaProtocol;
using lib.Brute;
using lib.Web;

namespace AlphaProtocolExecutor
{
    internal class Problem
    {
        public string Id { get; set; }

        public int Size { get; set; }

        public bool Solved { get; set; }

        public bool Bonus { get; set; }

        public string[] Operations { get; set; }

        public bool Tried { get; set; }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
//            Run();
            RunManual();
            //Test();
//            EvalTreesSizes(int.Parse(args[0]), int.Parse(args[1]));
        }

        private static void Run()
        {
            var whitelist = new[]
                {
                    "8q7jeQqjKoa9g0v6IOXixzJ3"
                };

            foreach (Problem problem in UnsolvedProblemsWithSize(15))
            {
                if (whitelist.Any(v => v == problem.Id))
                    AlphaProtocol.PostSolution(problem.Id, problem.Size, problem.Operations);
            }

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private static Problem[] UnsolvedProblemsWithSize(int size)
        {
            return GetProblems().Where(p => !p.Tried && !p.Bonus && p.Size == size).ToArray();
        }

        private static IEnumerable<Problem> GetProblems()
        {
            List<Problem> problems = File
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
                .Select(d => new Problem
                    {
                        Id = d.Id,
                        Size = int.Parse(d.Size),
                        Tried = d.Solved.Length > 0,
                        Solved = d.Solved.Equals("True", StringComparison.OrdinalIgnoreCase),
                        Bonus = d.Bonus.Equals("Bonus", StringComparison.OrdinalIgnoreCase),
                        Operations = d.Operations,
                    })
                .ToList();
            return problems;
        }

        private static void RunManual()
        {
            AlphaProtocol.PostSolution("spcrDoEBmeHgxgTAth0IZBVX", 14, "fold,not,plus,shr1,xor".Split(','));
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private static void EvalTreesSizes(int from, int count)
        {
            int threshold = 300*1000*1000;
            Problem[] problems = UnsolvedProblemsWithSize(15).Skip(from).Take(count).ToArray();
            Console.WriteLine("Starting for {0} tasks", problems.Length);
            Task<int>[] tasks = problems.Select(p => Task.Run(() => new BinaryBruteForcer(p.Operations).
                                                                        Enumerate(p.Size - 1).
                                                                        TakeWhile((t, i) => i < threshold).
                                                                        Count()))
                                        .ToArray();
            Task.WaitAll(tasks.ToArray<Task>());
            for (int i = 0; i < tasks.Length; i++)
            {
                Console.WriteLine("Task {0} has size {1}", problems[i].Id, tasks[i].Result);
            }
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private static void Test()
        {
            int threshold = 300*1000*1000;
            while (true)
            {
                var gsc = new GameServerClient();
                TrainResponse problem = gsc.Train(TrainProblemType.Any, 15);
                Console.Out.WriteLine("==== TrainProblem: {0}", problem);

                Task<int> countTask = Task.Run(() =>
                    {
                        IEnumerable<byte[]> trees = new BinaryBruteForcer(problem.operators).Enumerate(problem.size - 1);
                        return trees.TakeWhile((t, i) => i < threshold).Count();
                    });
                Stopwatch sw = Stopwatch.StartNew();
                string answer = ConcurrentWithoutShitAlphaProtocol.PostSolution(problem.id, problem.size,
                                                                                problem.operators);
                sw.Stop();
                Console.Out.WriteLine("==== Solved, waiting for TreeSize...");
                countTask.Wait();
                Console.Out.WriteLine("==== SolvedIn: {0} ms, Answer: {1}, TreeSize: {2}", sw.ElapsedMilliseconds,
                                      answer,
                                      countTask.Result);
            }
        }
    }
}