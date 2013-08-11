using System.Diagnostics;
using lib.AlphaProtocol;
using lib.Brute;
using log4net;

namespace AlphaProtocolExecutor
{
	internal class SolverProgram
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(Solver));

		public static void Run()
		{
			var gsc = new GameServerClient();
			while (true)
			{
				var problem = gsc.Train(TrainProblemType.Any, 14);
				log.DebugFormat("==== TrainProblem: {0}", problem);

				var solver = new Solver();
				var sw = Stopwatch.StartNew();
				var answer = solver.Solve(problem.id, problem.size, problem.OperatorsExceptBonus, (args, values) => new SmartGenerator(args, values, problem.OperatorsExceptBonus).Enumerate(problem.size - 1));
				sw.Stop();
				log.DebugFormat("==== SolvedIn: {0} ms, Answer: {1}", sw.ElapsedMilliseconds, answer);
			}
		}
	}
}