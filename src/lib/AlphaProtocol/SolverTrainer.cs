using System;
using System.Diagnostics;
using NUnit.Framework;
using lib.Web;

namespace lib.AlphaProtocol
{
	[TestFixture]
	public class SolverTrainer
	{
		private readonly GameServerClient gsc = new GameServerClient();

		[Test]
		public void Perf()
		{
			while (true)
			{
				var problem = gsc.Train(TrainProblemType.Any, 12);
				Console.Out.WriteLine("==== TrainProblem: {0}", problem);

				var sw = Stopwatch.StartNew();
				var answer = AlphaProtocol.PostSolution(problem.id, problem.size, problem.OperatorsExceptBonus, true);
				sw.Stop();
				Console.Out.WriteLine("==== SolvedIn: {0} ms, Answer: {1}", sw.ElapsedMilliseconds, answer);
			}
		}

		[Test]
		public void Debug()
		{
			var problem = new TrainResponse
			{
				id = "c1ZbBSHdECw62rTApgFfp9B0",
				challenge = "(lambda (x_11179) (fold x_11179 0 (lambda (x_11179 x_11180) (or (or (shr4 x_11180) 1) x_11179))))",
				size = 11,
				operators = "or,shr4,tfold".Split(','),
			};
			Console.Out.WriteLine("==== TrainProblem: {0}", problem);

			var sw = Stopwatch.StartNew();
			var answer = AlphaProtocol.PostSolution(problem.id, problem.size, problem.OperatorsExceptBonus, true);
			sw.Stop();
			Console.Out.WriteLine("==== SolvedIn: {0} ms, Answer: {1}", sw.ElapsedMilliseconds, answer);
	}
	}
}