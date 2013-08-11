using System;
using System.Threading;
using JetBrains.Annotations;
using log4net;

namespace Diadoc.Threading
{
	public abstract class BackgroundThreadWorker : BackgroundWorkerBase
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(BackgroundThreadWorker));

		private readonly Thread workerThread;

		protected BackgroundThreadWorker([NotNull] string workerName, bool isBackground = true)
			: base(workerName)
		{
			workerThread = new Thread(RunWorker)
			{
				Name = workerName,
				IsBackground = isBackground,
			};
			EndPreparation();
		}

		protected sealed override void DoRun()
		{
			workerThread.Start();
		}

		protected sealed override void DoWaitForCompletion()
		{
			workerThread.Join();
		}

		private void RunWorker()
		{
			try
			{
				DoWork();
				OnWorkCompletion();
			}
			catch (Exception e)
			{
				Log.Fatal(string.Format("Unhandled exception on worker thread {0}", workerName), e);
				OnWorkCompletion(e);
			}
		}

		protected abstract void DoWork();
	}
}