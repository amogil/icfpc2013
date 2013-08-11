using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Diadoc.Threading
{
	public class AffinityTaskScheduler<TKey> : ITaskScheduler<TKey>
	{
		private class Affinity
		{
			public int QueueIndex;
			public int RefCount;
		}

		private readonly object locker = new object();
		private readonly Dictionary<TKey, Affinity> affinities = new Dictionary<TKey, Affinity>();
		private readonly int[] queueSizes;

		public AffinityTaskScheduler(int workersCount)
		{
			queueSizes = new int[workersCount];
		}

		public int ScheduleTask([NotNull] TKey taskKey)
		{
			lock (locker)
			{
				int queueIndex;
				Affinity affinity;
				if (affinities.TryGetValue(taskKey, out affinity))
				{
					affinity.RefCount++;
					queueIndex = affinity.QueueIndex;
				}
				else
				{
					queueIndex = GetLeastLoadedQueueIndex();
					affinities.Add(taskKey, new Affinity { QueueIndex = queueIndex, RefCount = 1 });
				}
				++queueSizes[queueIndex];
				return queueIndex;
			}
		}

		public void DoneTask([NotNull] TKey taskKey)
		{
			lock (locker)
			{
				Affinity affinity;
				if (!affinities.TryGetValue(taskKey, out affinity))
					throw new InvalidOperationException(string.Format("Task was not scheduled: {0}", taskKey));
				if (--affinity.RefCount == 0)
					affinities.Remove(taskKey);
				if (--queueSizes[affinity.QueueIndex] < 0)
					throw new InvalidOperationException(string.Format("Queue was already empty: {0}", affinity.QueueIndex));
			}
		}

		private int GetLeastLoadedQueueIndex()
		{
			var minValue = int.MaxValue;
			var minIndex = 0;
			for (var i = 0; i < queueSizes.Length; i++)
			{
				var minValueCandidate = queueSizes[i];
				if (minValueCandidate < minValue)
				{
					minValue = minValueCandidate;
					minIndex = i;
				}
			}
			return minIndex;
		}
	}
}