using JetBrains.Annotations;

namespace Diadoc.Threading
{
	public interface ITaskScheduler<in TKey>
	{
		int ScheduleTask([NotNull] TKey taskKey);
		void DoneTask([NotNull] TKey taskKey);
	}
}