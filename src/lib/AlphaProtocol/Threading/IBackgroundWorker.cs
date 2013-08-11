using System;

namespace Diadoc.Threading
{
	public interface IBackgroundWorker
	{
		BackgroundWorkerState State { get; }
		void Run();
		void Stop();
		void WaitForCompletion();
		event Action<Exception> WorkCompleted;
	}
}