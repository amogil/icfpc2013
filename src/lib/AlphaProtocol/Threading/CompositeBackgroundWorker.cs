using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;

namespace Diadoc.Threading
{
	public class CompositeBackgroundWorker : BackgroundWorkerBase, ICompositeBackgroundWorker
	{
		private readonly ManualResetEvent workCompletedEvent = new ManualResetEvent(false);
		private readonly List<IBackgroundWorker> workers = new List<IBackgroundWorker>();
		private readonly List<Exception> workerErrors = new List<Exception>();
		private int completedWorkersCount;

		public CompositeBackgroundWorker([NotNull] string workerName)
			: base(workerName)
		{
		}

		public void AddWorkers([NotNull] params IBackgroundWorker[] workersToAdd)
		{
			AddWorkers<IBackgroundWorker>(workersToAdd);
		}

		public void AddWorkers<TWorker>([NotNull] params TWorker[] workersToAdd) where TWorker : IBackgroundWorker
		{
			if (workersToAdd.Length == 0)
				throw new InvalidOperationException("workersToAdd list is empty");
			BeginPreparation();
			foreach (var worker in workersToAdd)
			{
				var workerIndex = workers.Count;
				workers.Add(worker);
				worker.WorkCompleted += e => OnSingleWorkerCompletion(workerIndex, e);
				workerErrors.Add(null);
			}
			EndPreparation();
		}

		private void OnSingleWorkerCompletion(int workerIndex, [CanBeNull] Exception error)
		{
			workerErrors[workerIndex] = error;
			if (error != null)
				DoStop();
			if (Interlocked.Increment(ref completedWorkersCount) == workers.Count)
			{
				OnWorkCompletion(workerErrors.FindAll(e => e != null).ToArray());
				workCompletedEvent.Set();
			}
		}

		protected override void DoRun()
		{
			foreach (var worker in workers)
				worker.Run();
		}

		protected override void DoStop()
		{
			foreach (var worker in workers)
				worker.Stop();
		}

		protected sealed override void DoWaitForCompletion()
		{
			workCompletedEvent.WaitOne();
		}
	}
}