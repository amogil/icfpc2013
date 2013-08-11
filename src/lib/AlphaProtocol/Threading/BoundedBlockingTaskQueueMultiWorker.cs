using System;
using JetBrains.Annotations;

namespace Diadoc.Threading
{
	public abstract class BoundedBlockingTaskQueueMultiWorker<TTask> : CompositeBackgroundWorker
	{
		private readonly BoundedBlockingQueue<TTask> taskQueue;

		protected BoundedBlockingTaskQueueMultiWorker([NotNull] string workerName, int maxQueueSize, int workersCount, [NotNull] Func<BoundedBlockingQueue<TTask>, int, IBackgroundWorker> createWorker)
			: base(workerName)
		{
			taskQueue = new BoundedBlockingQueue<TTask>(maxQueueSize);
			for (var i = 0; i < workersCount; i++)
				AddWorkers(createWorker(taskQueue, i));
		}

		protected sealed override void DoStop()
		{
			taskQueue.CompleteAdding();
			base.DoStop();
		}

		protected sealed override void DoCompleteWork()
		{
			if (taskQueue.Count > 0)
				throw new InvalidOperationException(string.Format("Task queue is not empty in worker {0}", workerName));
		}

		protected void AddTask([NotNull] TTask task)
		{
			taskQueue.Add(task);
		}
	}
}