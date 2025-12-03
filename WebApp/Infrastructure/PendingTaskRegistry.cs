using System.Collections.Concurrent;

namespace WebApp.Infrastructure
{
    internal class PendingTaskRegistry<TKey, TResult> : IPendingTaskRegistry<TKey, TResult> where TKey : struct
    {
        private readonly ConcurrentDictionary<TKey, TaskCompletionSource<TResult>> _pendingRequests = new();

        public Task<TResult> WaitForResponseAsync(TKey key, CancellationToken cancellationToken = default)
            => WaitForResponseAsync(key, TimeSpan.FromSeconds(30), cancellationToken);

        public async Task<TResult> WaitForResponseAsync(TKey key, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<TResult>();

            if (!_pendingRequests.TryAdd(key, tcs))
            {
                throw new InvalidOperationException("A pending request with the same key already exists.");
            }

            try
            {
                var delayTask = Task.Delay(timeout, cancellationToken);

                var completedTask = await Task.WhenAny(tcs.Task, delayTask);

                if (completedTask == tcs.Task)
                {
                    return tcs.Task.Result;
                }
                else if (completedTask == delayTask)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    throw new TimeoutException("The operation has timed out while waiting for a response.");
                }

                // Should not happen if cancellation token is handled correctly in Task.Delay

                throw new InvalidOperationException("Unexpected task completion.");
            }
            finally
            {
                _pendingRequests.TryRemove(key, out _);
            }
        }

        public bool TrySetResult(TKey key, TResult result)
        {
            if (_pendingRequests.TryGetValue(key, out var tcs))
            {
                tcs.SetResult(result);
                return true;
            }

            return false;
        }
    }
}
