using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Diadoc.Threading;
using JetBrains.Annotations;
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
			return Solve(problemId, size, ops, (inputs, values) => new SmartGenerator(inputs, values, ops).Enumerate(size - 1, size - 1, false).Print(t => t.Printable(), 10));
		}

		public string Solve(string problemId, int size, string[] ops, Func<List<ulong>, List<ulong>, IEnumerable<byte[]>> getGuesses)
		{
			log.DebugFormat("Solving problem {0}: size={1}, ops={2}", problemId, size, string.Join(", ", ops));

			var wallTime = Stopwatch.StartNew();
			var correctValues = WithTimer(() => gsc.Eval(problemId, args), "gsc.Eval()");

			string answer = null;
			var solvedSignal = new ManualResetEvent(false);
			var argToValue = new ConcurrentDictionary<ulong, ulong>();
			for (int i = 0; i < args.Count; i++)
				argToValue[args[i]] = correctValues[i];
			var controller = new SolverController("solver", problemId, gsc, argToValue, wallTime);
			controller.Solved += program =>
				{
					solvedSignal.Set();
					answer = program;
				};
			controller.Run();

		    int c = 0;
			foreach (var guess in getGuesses(args, correctValues))
			{
				controller.Solve(guess);
				if (solvedSignal.WaitOne(0))
				{
					controller.Stop();
					controller.WaitForCompletion();
					return answer;
				}
			}

			log.DebugFormat("SOLUTION_NOT_FOUND: {0}, WS: {1}", wallTime.ElapsedMilliseconds, Environment.WorkingSet);
			return null;
		}

		private static T WithTimer<T>(Func<T> getResult, string funcName)
		{
			var sw = Stopwatch.StartNew();
			var result = getResult();
			sw.Stop();
			log.DebugFormat("{0} took: {1} ms, WS: {2}", funcName, sw.ElapsedMilliseconds, Environment.WorkingSet);
			return result;
		}
	}

	public class SolverController : CompositeBackgroundWorker
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(Solver));
		private readonly string problemId;
		private readonly GameServerClient gsc;
		private readonly ConcurrentDictionary<ulong, ulong> argToValue;
		private readonly Stopwatch wallTime;
		private readonly SolverTaskReadQueueMultiWorker readWorker;
		private readonly SolverTaskReadResultQueueWorker resultWorker;
		private volatile bool solved;

		public SolverController([NotNull] string workerName, string problemId, GameServerClient gsc, ConcurrentDictionary<ulong, ulong> argToValue, Stopwatch wallTime)
			: base(workerName)
		{
			this.problemId = problemId;
			this.gsc = gsc;
			this.argToValue = argToValue;
			this.wallTime = wallTime;
			const int workersCount = 4;
			const int readQueueSize = 100000;
			const int resultQueueSize = 100000;
			readWorker = new SolverTaskReadQueueMultiWorker(string.Format("{0}-read", workerName), readQueueSize, workersCount);
			resultWorker = new SolverTaskReadResultQueueWorker(string.Format("{0}-readResult", workerName), resultQueueSize, OnGoodGuess);
			AddWorkers(readWorker, resultWorker);
		}

		public event Action<string> Solved = program => { };

		public void Solve(byte[] guess)
		{
			var task = new SolverTask(guess, argToValue, new Promise<bool>());
			readWorker.EnqueueTask(task);
			resultWorker.EnqueueTask(task);
		}

		private void OnGoodGuess(byte[] guess)
		{
			if (solved)
				return;
				var program = FormatProgram(guess);
				var wrongAnswer = WithTimer(() => gsc.Guess(problemId, program), "gsc.Guess()");
				if (wrongAnswer == null)
				{
					log.DebugFormat("SOLVED: {0}, TimeTaken: {1} ms, WS: {2}", problemId, wallTime.ElapsedMilliseconds, Environment.WorkingSet);
				Solved(program);
				solved = true;
				return;
				}
				log.DebugFormat("WRONG_ANSWER: {0}, WS: {1}", wrongAnswer, Environment.WorkingSet);
			argToValue[wrongAnswer.Arg] = wrongAnswer.CorrectValue;

				if (wallTime.Elapsed.TotalSeconds > 300)
				{
					log.DebugFormat("TIME_LIMIT: {0}, WS: {1}", wallTime.ElapsedMilliseconds, Environment.WorkingSet);
				}
		}

		private static string FormatProgram(byte[] solution)
		{
			var s = solution.SkipWhile(b => b == 16).ToArray();
			return string.Format("(lambda (x) {0})", s.ToSExpr().Item1);
			}

		private static T WithTimer<T>(Func<T> getResult, string funcName)
		{
			var sw = Stopwatch.StartNew();
			var result = getResult();
			sw.Stop();
			log.DebugFormat("{0} took: {1} ms, WS: {2}", funcName, sw.ElapsedMilliseconds, Environment.WorkingSet);
			return result;
		}
	}

	public class SolverTaskReadQueueMultiWorker : BoundedBlockingTaskQueueMultiWorker<SolverTask>
		{
		public SolverTaskReadQueueMultiWorker([NotNull] string workerName, int maxReadQueueSize, int workersCount)
			: base(workerName, maxReadQueueSize, workersCount, (q, i) => new SolverTaskReadQueueWorker(string.Format("{0}-w#{1}", workerName, i), q))
			{
		}

		public void EnqueueTask([NotNull] SolverTask task)
		{
			AddTask(task);
		}
	}

	public class SolverTaskReadQueueWorker : BackgroundThreadWorker
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(BoundedBlockingTaskQueueWorker<>));
		private const long samplingInterval = 100000;
		private readonly ManualResetEvent stopSignal = new ManualResetEvent(false);
		private readonly BoundedBlockingQueue<SolverTask> taskQueue;

		public SolverTaskReadQueueWorker([NotNull] string workerName, [NotNull] BoundedBlockingQueue<SolverTask> taskQueue)
			: base(workerName)
		{
			this.taskQueue = taskQueue;
		}

		protected sealed override void DoStop()
		{
			stopSignal.Set();
		}

		protected sealed override void DoWork()
		{
			long tasksCount = 0;
			while (!taskQueue.IsCompleted)
			{
				SolverTask task;
				if (taskQueue.TryTake(out task, TimeSpan.FromMilliseconds(100)))
				{
					task.ResultPromise.SetResult(IsGood(task.Guess, task.ArgToValue));
					if (++tasksCount % samplingInterval == 0 && taskQueue.Count > 0)
						Log.InfoFormat("ReadQueueSize: {0}, TasksProcessed: {1}", taskQueue.Count, tasksCount);
				}
			}
		}

		private static bool IsGood(byte[] guess, ConcurrentDictionary<ulong, ulong> argToValue)
		{
			foreach (var kvp in argToValue)
			{
				if (guess.Eval(kvp.Key) != kvp.Value)
					return false;
			}
			return true;
		}
	}

	public class SolverTaskReadResultQueueWorker : BoundedBlockingTaskQueueWorker<SolverTask>
		{
		private readonly Action<byte[]> callback;

		public SolverTaskReadResultQueueWorker([NotNull] string workerName, int maxQueueSize, [NotNull] Action<byte[]> callback)
			: base(workerName, maxQueueSize)
		{
			this.callback = callback;
		}

		public void EnqueueTask([NotNull] SolverTask task)
		{
			AddTask(task);
		}

		protected override void DoProcessTask([NotNull] SolverTask task)
		{
			var isGood = task.ResultPromise.GetResult();
			if (isGood)
				callback(task.Guess);
	}
	}

	public class SolverTask
	{
		public SolverTask(byte[] guess, ConcurrentDictionary<ulong, ulong> argToValue, [NotNull] Promise<bool> resultPromise)
		{
			Guess = guess;
			ArgToValue = argToValue;
			ResultPromise = resultPromise;
		}

		public byte[] Guess { get; private set; }
		public ConcurrentDictionary<ulong, ulong> ArgToValue { get; private set; }

		[NotNull]
		public Promise<bool> ResultPromise { get; private set; }

		public override string ToString()
		{
			return string.Format("Guess: {0}, ResultPromise: {1}", Guess.ToSExpr().Item1, ResultPromise);
		}
	}
}