using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
				var problem = gsc.Train(TrainProblemType.Fold, 16);
				Console.Out.WriteLine("==== TrainProblem: {0}", problem);

				var solver = new Solver();
				var sw = Stopwatch.StartNew();
				var answer = solver.SolveBinaryBrute(problem.id, problem.size, problem.OperatorsExceptBonus);
				sw.Stop();

				int solutionSize = Expr.ParseFunction(answer).GetUnified().ToBinExp().ToArray().Size() + 1;
				int problemSize = Expr.ParseFunction(problem.challenge).GetUnified().ToBinExp().ToArray().Size() + 1;
				Console.Out.WriteLine("==== SolvedIn: {0} ms, Answer: {1}, ProblemSize: {2}, SolutionSize: {3}", sw.ElapsedMilliseconds, answer, problemSize, solutionSize);
			}
		}

		[Test]
		public void Perf_Smart()
		{
			while (true)
			{
				var problem = gsc.Train(TrainProblemType.Fold, 15);
				Console.Out.WriteLine("==== TrainProblem: {0}", problem);

				var sw = Stopwatch.StartNew();
				var answer = ConcurrentWithoutShitAlphaProtocol.PostSolution(problem.id, problem.size, problem.operators);
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
			while (true)
			{
				var problem = gsc.Train(TrainProblemType.Bonus137);
				Console.WriteLine(problem);
				Console.WriteLine("SHORT" + problem.challenge);

				/*var solver = new Solver();
				var sw = Stopwatch.StartNew();
				var answer = solver.Solve(problem.id, problem.size, problem.OperatorsExceptBonus, vs =>
					{
						var answersMask = new Mask(vs);
						Console.WriteLine(answersMask);
						return
							new BinaryBruteForcer(answersMask, problem.OperatorsExceptBonus).EnumerateBonus(problem.size - 1)
																							.Print(t => t.Printable());
					});
				sw.Stop();
				Console.Out.WriteLine("==== SolvedIn: {0} ms, Answer: {1}", sw.ElapsedMilliseconds, answer);*/
			}
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
			var answer = solver.SolveBinaryBrute(problem.id, problem.size, problem.OperatorsExceptBonus);
			sw.Stop();
			Console.Out.WriteLine("==== SolvedIn: {0} ms, Answer: {1}", sw.ElapsedMilliseconds, answer);
		}

		[Test]
		public void CuttingTest()
		{
			var keys = GenerateFirstSamples();
			ProblemsReader reader = ProblemsReader.CreateDefault();
			for (int i = 8; i < 14; ++i)
			{
				var problems =
					reader.ReadProblems()
						  .Where(p => p.Size == i)
						  .SelectMany(
							  p => reader.ReadTrainProblems(p, true).Concat(reader.ReadTrainProblems(p, false)).Select(tp => Tuple.Create(p, tp)).Take(2))
						  .Take(10);
				foreach (var problemPair in problems)
				{
					var tp = problemPair.Item2;
					var programm = Expr.ParseFunction(tp.challenge).GetUnified().ToBinExp().ToArray();

					var values = keys.ToDictionary(k => k, k => programm.Eval(k));
					Func<byte[], bool> check = tree => values.All(kv => tree.Eval(kv.Key) == kv.Value);

					var solutions = new BinaryBruteForcer(new Mask(values.Values), tp.operators).Enumerate(tp.size - 1);
					int count = 0;
					int cutted = 0;
					foreach (var solution in solutions)
					{
						++count;
						if (!check(solution))
							++cutted;
					}
					Console.WriteLine(String.Format("{0}\t{1}\t{2}\t{3}", count, cutted, tp.size, tp));
				}
			}
		}


		protected UInt64[] GenerateValuesRANDOM()
		{
			Random random = new Random(1234567890);
			return Enumerable.Range(1, 256).Select(e => random.NextUInt64()).ToArray();
		}

		protected UInt64[] GenerateValuesAllInMask()
		{
			List<UInt64> keys = new List<ulong>();
			List<int> bits = new List<int>();
			for (int i = 0; i < 64; ++i)
				bits.Add(i);




			Random rnd = new Random(1234567890);
			for (int set = 0; set < 16; ++set)
			{
				int[] bitInds = new int[4];
				for (int i = 0; i < 4; ++i)
				{
					int num = rnd.Next(bits.Count);
					bitInds[i] = bits[num];
					bits.RemoveAt(num);
				}

				ulong src = rnd.Next() % 2 == 0 ? 0 : ulong.MaxValue;
				ulong[] setvalues = new ulong[16];
				for (int i = 0; i < 16; ++i)
					setvalues[i] = src;


				for (ulong i = 0; i < 16; ++i)
					for (int j = 0; j < 4; ++j)
					{
						var bitInd = bitInds[j];
						var bitValue = i.Bit(j);
						if (bitValue)
							setvalues[i].SetBit(bitInd);
						else
							setvalues[i].UnsetBit(bitInd);
					}
				keys.AddRange(setvalues);
			}


			HashSet<ulong> resultKeys = new HashSet<ulong>();
			foreach (var k in keys)
				if (!resultKeys.Contains(k))
					resultKeys.Add(k);
			while (resultKeys.Count < 256)
			{
				var k = rnd.NextUInt64();
				if (!resultKeys.Contains(k))
					resultKeys.Add(k);
			}
			return resultKeys.ToArray();

		}

		protected UInt64[] GenerateFirstSamples()
		{
			var samples = new UInt64[256];
			samples[0] = UInt64.MinValue;
			samples[1] = UInt64.MaxValue;
			samples[2] = UInt64.Parse("00000000FFFFFFFF", NumberStyles.AllowHexSpecifier);
			samples[3] = UInt64.Parse("FFFFFFFF00000000", NumberStyles.AllowHexSpecifier);
			samples[4] = UInt64.Parse("0F0F0F0F0F0F0F0F", NumberStyles.AllowHexSpecifier);
			samples[5] = UInt64.Parse("F0F0F0F0F0F0F0F0", NumberStyles.AllowHexSpecifier);
			samples[6] = UInt64.Parse("5555555555555555", NumberStyles.AllowHexSpecifier);
			samples[7] = UInt64.Parse("AAAAAAAAAAAAAAAA", NumberStyles.AllowHexSpecifier);
			var random = new Random(1234567890);
			for (int k = 1; k < 32; k++)
			{
				for (int i = 0; i < 4; i++)
				{
					UInt64 sample = 0;
					var onesCount = 0;
					while (onesCount < k)
					{
						var onePosition = random.Next(64);
						if ((sample & (((UInt64)1) << onePosition)) == 0)
						{
							sample += ((UInt64)1) << onePosition;
							onesCount++;
						}
					}
					samples[k * 8 + i * 2] = sample;
					samples[k * 8 + i * 2 + 1] = sample ^ UInt64.MaxValue;
				}
			}
			return samples;
		}

		[Test]
		public void TestFirstSamples()
		{
			var samples = GenerateFirstSamples();
		}
	}
}