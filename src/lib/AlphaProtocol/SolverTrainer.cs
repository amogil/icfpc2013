using System;
using System.Diagnostics;
using NUnit.Framework;

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
				var problem = gsc.Train(TrainType.Simple, 12);
				Console.Out.WriteLine("==== TrainProblem: {0}", problem);

				var sw = Stopwatch.StartNew();
				var answer = AlphaProtocol.PostSolution(problem.id, problem.size, problem.OperatorsExceptBonus);
				sw.Stop();
				Console.Out.WriteLine("==== SolvedIn: {0} ms, Answer: {1}", sw.ElapsedMilliseconds, answer);
			}
		}
	}
}