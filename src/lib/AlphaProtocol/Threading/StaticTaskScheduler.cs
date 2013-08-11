using JetBrains.Annotations;

namespace Diadoc.Threading
{
	public class StaticTaskScheduler<TKey> : ITaskScheduler<TKey>
	{
		private readonly int workersCount;

		public StaticTaskScheduler(int workersCount)
		{
			this.workersCount = workersCount;
		}

		public int ScheduleTask([NotNull] TKey taskKey)
		{
			unchecked
			{
				return (int)(((uint)taskKey.GetHashCode()) % workersCount);
			}
		}

		public void DoneTask([NotNull] TKey taskKey)
		{
		}
	}
}