using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;

namespace Diadoc.Threading
{
	public abstract class BackgroundWorkerBase : IBackgroundWorker
	{
		protected readonly string workerName;
		private volatile Exception workerError;
		private int state;

		protected BackgroundWorkerBase([NotNull] string workerName)
		{
			this.workerName = workerName;
			state = (int)BackgroundWorkerState.Created;
		}

		public event Action<Exception> WorkCompleted = e => { };

		public BackgroundWorkerState State
		{
			get { return CompareExchangeState(BackgroundWorkerState.Created, BackgroundWorkerState.Created); }
		}

		protected void BeginPreparation()
		{
			var oldState = CompareExchangeState(BackgroundWorkerState.Preparing, BackgroundWorkerState.Created);
			if (oldState == BackgroundWorkerState.Created)
				return;
			if (oldState == BackgroundWorkerState.Prepared)
			{
				oldState = CompareExchangeState(BackgroundWorkerState.Preparing, BackgroundWorkerState.Prepared);
				if (oldState == BackgroundWorkerState.Prepared)
					return;
			}
			throw new InvalidWorkerStateException(workerName, oldState);
		}

		protected void EndPreparation()
		{
			var oldState = CompareExchangeState(BackgroundWorkerState.Prepared, BackgroundWorkerState.Created);
			if (oldState == BackgroundWorkerState.Created)
				return;
			if (oldState == BackgroundWorkerState.Preparing)
			{
				oldState = CompareExchangeState(BackgroundWorkerState.Prepared, BackgroundWorkerState.Preparing);
				if (oldState == BackgroundWorkerState.Preparing)
					return;
			}
			throw new InvalidWorkerStateException(workerName, oldState);
		}

		public void Run()
		{
			var oldState = CompareExchangeState(BackgroundWorkerState.Running, BackgroundWorkerState.Prepared);
			if (oldState != BackgroundWorkerState.Prepared)
				throw new InvalidWorkerStateException(workerName, oldState);
			DoRun();
		}

		public void Stop()
		{
			var oldState = CompareExchangeState(BackgroundWorkerState.Stopping, BackgroundWorkerState.Running);
			if (oldState == BackgroundWorkerState.Running)
			{
				DoStop();
				return;
			}
			if (oldState == BackgroundWorkerState.Stopping || oldState == BackgroundWorkerState.Stopped)
				return;
			throw new InvalidWorkerStateException(workerName, oldState);
		}

		public void WaitForCompletion()
		{
			var oldState = State;
			if (oldState == BackgroundWorkerState.Running || oldState == BackgroundWorkerState.Stopping)
			{
				DoWaitForCompletion();
				if (workerError != null)
					throw workerError;
				return;
			}
			if (oldState == BackgroundWorkerState.Stopped)
			{
				if (workerError != null)
					throw workerError;
				return;
			}
			throw new InvalidWorkerStateException(workerName, oldState);
		}

		protected void OnWorkCompletion([NotNull] params Exception[] errors)
		{
			HandleWorkCompletion(errors);
			var oldState = ExchangeState(BackgroundWorkerState.Stopped);
			if (oldState != BackgroundWorkerState.Running && oldState != BackgroundWorkerState.Stopping)
				throw new InvalidWorkerStateException(workerName, oldState);
		}

		private void HandleWorkCompletion([NotNull] IEnumerable<Exception> errors)
		{
			var allErrors = new List<Exception>(errors);
			try
			{
				DoCompleteWork();
			}
			catch (Exception e)
			{
				allErrors.Add(e);
			}
			workerError = TryGetWorkerError(allErrors);
			WorkCompleted(workerError);
		}

		[CanBeNull]
		private static Exception TryGetWorkerError([NotNull] IList<Exception> allErrors)
		{
			switch (allErrors.Count)
			{
				case 0:
					return null;
				case 1:
					return allErrors[0];
				default:
					return new AggregateException(allErrors);
			}
		}

		protected virtual void DoCompleteWork()
		{
		}

		private BackgroundWorkerState ExchangeState(BackgroundWorkerState newState)
		{
			return (BackgroundWorkerState)Interlocked.Exchange(ref state, (int)newState);
		}

		private BackgroundWorkerState CompareExchangeState(BackgroundWorkerState newState, BackgroundWorkerState comparandState)
		{
			return (BackgroundWorkerState)Interlocked.CompareExchange(ref state, (int)newState, (int)comparandState);
		}

		protected abstract void DoRun();
		protected abstract void DoStop();
		protected abstract void DoWaitForCompletion();
	}
}