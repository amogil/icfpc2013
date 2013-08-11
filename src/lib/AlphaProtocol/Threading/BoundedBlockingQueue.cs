using System.Collections.Concurrent;

namespace Diadoc.Threading
{
	public class BoundedBlockingQueue<T> : BlockingCollection<T>
	{
		public BoundedBlockingQueue(int maxQueueSize)
			: base(new ConcurrentQueue<T>(), maxQueueSize)
		{
		}
	}
}