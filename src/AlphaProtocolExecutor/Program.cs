using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using lib.AlphaProtocol;
using lib.Brute;
using lib.Web;
using log4net;

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
        private static readonly ILog log = LogManager.GetLogger(typeof(AlphaProtocol));

        private static void Main(string[] args)
        {
            int size = ArgToInt(args[0], 18);
            int skip = ArgToInt(args[1], 0);
            int take = ArgToInt(args[2], int.MaxValue);
            Run(size, skip, take);
//               RunManual();
//            Test();
//            EvalTreesSizes(int.Parse(args[0]), int.Parse(args[1]));
        }

        private static int ArgToInt(string arg, int defaultVal = 0)
        {
            int r;
            return int.TryParse(arg, out r) ? r : defaultVal;
        }

        private static void Run(int size, int skip, int take)
        {
            var whitelist = new[]
                {
                    "LqUJiqwGnrvvcoYmP70sawHu",
                    "Ni6aU05uJlbmMwl2P7uvPwtD",
                    "PCKOdSBp1FBfQaOhHB5gBATf"
                };

            foreach (Problem problem in UnsolvedProblemsWithSize(size).Skip(skip).Take(take))
            {
//                if (whitelist == null || whitelist.Any(v => v == problem.Id))
//                {
                    try
                    {
                        ConcurrentWithoutShitAlphaProtocol.PostSolution(problem.Id, problem.Size, problem.Operations);
                    }
                    catch (Exception e)
                    {
                        log.Debug(string.Format("FAILED: {0}", e));
                    }
                    
//                }
            }

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private static Problem[] UnsolvedProblemsWithSize(int size)
        {
            return
                GetProblems()
                    .Where(p => !p.Tried && !p.Bonus && p.Size == size)
                    .ToArray();
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
            ConcurrentWithoutShitAlphaProtocol.PostSolution("ECDMQ7dNtWCSuNz95FKfFLpY", 17,
                                                            "and,fold,if0,not,shl1,xor".Split(','));
            Console.WriteLine("Press any key...");
            Console.ReadKey();

            
        }

        private static void EvalTreesSizes(int from, int count)
        {
            int threshold = 800*1000*1000;
            Problem[] problems = UnsolvedProblemsWithSize(16).Skip(from).Take(count).ToArray();
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
            int threshold = 400*1000*1000;
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