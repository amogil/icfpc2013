using System;

namespace Diadoc.Threading
{
	public class InvalidWorkerStateException : Exception
	{
		public InvalidWorkerStateException(string workerName, BackgroundWorkerState state)
			: base(string.Format("Worker {0} is in invalid state: {1}", workerName, state))
		{
		}
	}
}