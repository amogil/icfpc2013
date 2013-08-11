using System;
using System.Threading;
using JetBrains.Annotations;
using log4net;

namespace Diadoc.Threading
{
	public abstract class BoundedBlockingTaskQueueWorker<TTask> : BackgroundThreadWorker
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(BoundedBlockingTaskQueueWorker<>));
		private const long samplingInterval = 100000;
		private readonly BoundedBlockingQueue<TTask> taskQueue;

		protected BoundedBlockingTaskQueueWorker([NotNull] string workerName, int maxQueueSize)
			: base(workerName)
		{
			taskQueue = new BoundedBlockingQueue<TTask>(maxQueueSize);
		}

		protected sealed override void DoWork()
		{
			long tasksCount = 0;
			while (!taskQueue.IsCompleted)
			{
				TTask task;
				if (taskQueue.TryTake(out task, Timeout.Infinite))
				{
					if (++tasksCount % samplingInterval == 0 && taskQueue.Count > 0)
						Log.InfoFormat("ResultQueueSize: {0}, TasksProcessed: {1}", taskQueue.Count, tasksCount);
					ProcessTask(task);
				}
			}
		}

		protected sealed override void DoStop()
		{
			taskQueue.CompleteAdding();
		}

		private void ProcessTask([NotNull] TTask task)
		{
			try
			{
				DoProcessTask(task);
			}
			catch (Exception e)
			{
				Log.Error(string.Format("Failed to process task {0}({1})", task.GetType().Name, task), e);
			}
		}

		protected void AddTask([NotNull] TTask task)
		{
			taskQueue.Add(task);
		}

		protected abstract void DoProcessTask([NotNull] TTask task);
	}
}