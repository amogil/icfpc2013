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

		public string Solve(string problemId, int size, string[] ops)
		{
			return Solve(problemId, size, ops, values => new BinaryBruteForcer(new Mask(values), ops).Enumerate(size - 1));
		}

		public string Solve(string problemId, int size, string[] ops, Func<List<ulong>, IEnumerable<byte[]>> getGuesses)
		{
			log.DebugFormat("Solving problem {0}: size={1}, ops={2}", problemId, size, string.Join(", ", ops));

			var wallTime = Stopwatch.StartNew();
			var correctValues = WithTimer(() => gsc.Eval(problemId, args), "gsc.Eval()");

			var nextGuessSw = Stopwatch.StartNew();
			foreach (var guess in getGuesses(correctValues))
			{
				if (!IsGood(guess, correctValues))
					continue;
				log.DebugFormat("findNextGuess() took: {0} ms", nextGuessSw.ElapsedMilliseconds);

				var program = FormatProgram(guess);
				var wrongAnswer = WithTimer(() => gsc.Guess(problemId, program), "gsc.Guess()");
				if (wrongAnswer == null)
				{
					log.DebugFormat("SOLVED: {0}", problemId);
					return program;
				}
				log.Debug(string.Format("WRONG_ANSWER: {0}", wrongAnswer));
				args.Add(wrongAnswer.Arg);
				correctValues.Add(wrongAnswer.CorrectValue);

				if (wallTime.Elapsed.TotalSeconds > 300)
					log.Debug(string.Format("TIME_LIMIT: {0}", wallTime.ElapsedMilliseconds));

				nextGuessSw.Reset();
			}

			log.Debug(string.Format("SOLUTION_NOT_FOUND: {0}", wallTime.ElapsedMilliseconds));
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
			log.DebugFormat("{0} took: {1} ms", funcName, sw.ElapsedMilliseconds);
			return result;
		}

		private static string FormatProgram(byte[] solution)
		{
			return string.Format("(lambda (x) {0})", solution.ToSExpr().Item1);
		}
	}
}