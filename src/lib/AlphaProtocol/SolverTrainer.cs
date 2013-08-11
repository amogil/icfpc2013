using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using lib.Brute;
using lib.Lang;
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
				var problem = gsc.Train(TrainProblemType.Any, 14);
				Console.Out.WriteLine("==== TrainProblem: {0}", problem);

				var solver = new Solver();
				var sw = Stopwatch.StartNew();
				var answer = solver.Solve(problem.id, problem.size, problem.OperatorsExceptBonus);
				sw.Stop();
				Console.Out.WriteLine("==== SolvedIn: {0} ms, Answer: {1}", sw.ElapsedMilliseconds, answer);
			}
		}

		[Test]
		public void Test()
		{
			Console.WriteLine(new BinaryBruteForcer("if0,not,or,shr16,shr4,xor".Split(',')).Enumerate(14 - 1).Count());
			Console.WriteLine(new BinaryBruteForcer("if0,not,or,shl1,shr16,xor".Split(',')).Enumerate(14 - 1).Count());
		}

		[Test]
		public void GetBonuses()
		{
			var problem = gsc.Train(TrainProblemType.Bonus, 42);
			Console.WriteLine(problem);

			var solver = new Solver();
			var sw = Stopwatch.StartNew();
			var answer = solver.Solve(problem.id, problem.size, problem.OperatorsExceptBonus, vs =>
				{
					var answersMask = new Mask(vs);
					Console.WriteLine(answersMask);
					return new BinaryBruteForcer(answersMask, problem.OperatorsExceptBonus).EnumerateBonus(problem.size - 1).Print(t => t.Printable());
				});
			sw.Stop();
			Console.Out.WriteLine("==== SolvedIn: {0} ms, Answer: {1}", sw.ElapsedMilliseconds, answer);
		}

		[Test]
		public void Debug()
		{
			var problem = new TrainResponse
			{
				id = "lHObnL0KiFVC3c8vPkDDzAUc",
				challenge = "(lambda (x_12074) (shr4 (shr4 (if0 (or (shr16 x_12074) 1) (shl1 (shr1 x_12074)) x_12074))))",
				size = 12,
				operators = "if0,or,shl1,shr1,shr16,shr4".Split(','),
			};
			Console.Out.WriteLine("==== TrainProblem: {0}", problem);

			var solver = new Solver();
			var sw = Stopwatch.StartNew();
			var answer = solver.Solve(problem.id, problem.size, problem.OperatorsExceptBonus);
			sw.Stop();
			Console.Out.WriteLine("==== SolvedIn: {0} ms, Answer: {1}", sw.ElapsedMilliseconds, answer);
		}

		[Test]
		public void MaskSelection()
		{
			while (true)
			{
				var problem = gsc.Train(TrainProblemType.Any, 12);
				Console.Out.WriteLine("==== TrainProblem: {0}", problem);

				var rnd = new Random();
				var allValues = new List<ulong>();
				for (int i = 0; i < 5; i++)
				{
					var args = Enumerable.Range(1, 64).Select(e => rnd.NextUInt64()).ToArray();
					var values = gsc.EvalProgram(problem.challenge, args);
					allValues.AddRange(values);
					var m = new Mask(values);
					Console.Out.WriteLine("CurrentMask: {0}, AccumulatedMask: {1}", m, new Mask(allValues.ToArray()));
				}
			}
		}
	}
}