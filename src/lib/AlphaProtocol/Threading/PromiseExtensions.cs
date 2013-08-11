using System;
using JetBrains.Annotations;

namespace Diadoc.Threading
{
	public static class PromiseExtensions
	{
		[NotNull]
		public static TResult GetResult<TResult>([NotNull] this Promise<TResult> promise)
		{
			var result = promise.Result;
			if (ReferenceEquals(result, null))
				throw new InvalidOperationException(string.Format("Result is null for promise: {0}", promise));
			return result;
		}
	}
}