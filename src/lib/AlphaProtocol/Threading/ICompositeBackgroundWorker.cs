using JetBrains.Annotations;

namespace Diadoc.Threading
{
	public interface ICompositeBackgroundWorker : IBackgroundWorker
	{
		void AddWorkers([NotNull] params IBackgroundWorker[] workersToAdd);
		void AddWorkers<TWorker>([NotNull] params TWorker[] workersToAdd) where TWorker : IBackgroundWorker;
	}
}