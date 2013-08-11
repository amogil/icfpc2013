using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using lib.Brute;
using lib.Lang;
using log4net;

namespace lib.AlphaProtocol
{
	public class Solver
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(Solver));

		private readonly Random rnd;
		private readonly GameServerClient gsc;
		private readonly List<ulong> args;

		public Solver()
		{
			rnd = new Random();
			gsc = new GameServerClient();
			args = new ulong[] { 0, 1, ulong.MaxValue, ulong.MinValue }.Concat(Enumerable.Range(1, 256).Select(e => rnd.NextUInt64())).Take(256).ToList();
		}

		public string SolveBinaryBrute(string problemId, int size, string[] ops)
		{
			return Solve(problemId, size, ops, (inputs, values) => new BinaryBruteForcer(new Mask(values), ops).Enumerate(size - 1));
		}

		public string SolveSmart(string problemId, int size, string[] ops)
		{
			return Solve(problemId, size, ops, (inputs, values) => new SmartGenerator(inputs, values, ops).Enumerate(size - 1, size - 1, true));
		}

		public string Solve(string problemId, int size, string[] ops, Func<List<ulong>, List<ulong>, IEnumerable<byte[]>> getGuesses)
		{
			log.DebugFormat("Solving problem {0}: size={1}, ops={2}", problemId, size, string.Join(", ", ops));

			var wallTime = Stopwatch.StartNew();
			var correctValues = WithTimer(() => gsc.Eval(problemId, args), "gsc.Eval()");

			var nextGuessSw = Stopwatch.StartNew();
			foreach (var guess in getGuesses(args, correctValues))
			{
				if (!IsGood(guess, correctValues))
					continue;
				log.DebugFormat("findNextGuess() took: {0} ms, WS: {1}", nextGuessSw.ElapsedMilliseconds, Environment.WorkingSet);

				var program = FormatProgram(guess);
				var wrongAnswer = WithTimer(() => gsc.Guess(problemId, program), "gsc.Guess()");
				if (wrongAnswer == null)
				{
					log.DebugFormat("SOLVED: {0}, TimeTaken: {1} ms, WS: {2}", problemId, wallTime.ElapsedMilliseconds, Environment.WorkingSet);
					return program;
				}
				log.DebugFormat("WRONG_ANSWER: {0}, WS: {1}", wrongAnswer, Environment.WorkingSet);
				args.Add(wrongAnswer.Arg);
				correctValues.Add(wrongAnswer.CorrectValue);

				if (wallTime.Elapsed.TotalSeconds > 300)
				{
					log.DebugFormat("TIME_LIMIT: {0}, WS: {1}", wallTime.ElapsedMilliseconds, Environment.WorkingSet);
					/*if (wallTime.Elapsed.TotalSeconds > 500)
						return null;*/
				}

				nextGuessSw.Reset();
			}

			log.DebugFormat("SOLUTION_NOT_FOUND: {0}, WS: {1}", wallTime.ElapsedMilliseconds, Environment.WorkingSet);
			return null;
		}

		private bool IsGood(byte[] guess, IList<ulong> correctValues)
		{
			for (var i = 0; i < args.Count; i++)
			{
				if (guess.Eval(args[i]) != correctValues[i])
					return false;
			}
			return true;
		}

		private static T WithTimer<T>(Func<T> getResult, string funcName)
		{
			var sw = Stopwatch.StartNew();
			var result = getResult();
			sw.Stop();
			log.DebugFormat("{0} took: {1} ms, WS: {2}", funcName, sw.ElapsedMilliseconds, Environment.WorkingSet);
			return result;
		}

		private static string FormatProgram(byte[] solution)
		{
			return string.Format("(lambda (x) {0})", solution.ToSExpr().Item1);
		}
	}
}