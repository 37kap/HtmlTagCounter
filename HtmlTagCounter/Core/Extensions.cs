using System;
using System.Threading;
using System.Threading.Tasks;

namespace HtmlTagCounter.Core
{
    /// <summary>
    /// Extensions methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Adds the ability to cancel the Task through a cancellation token.
        /// </summary>
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken, Action action, bool useSynchronizationContext = true)
        {
            using (cancellationToken.Register(action, useSynchronizationContext))
            {
                try
                {
                    return await task;
                }
                catch (Exception ex)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(ex.Message, ex, cancellationToken);
                    }

                    throw;
                }
            }
        }

    }
}
