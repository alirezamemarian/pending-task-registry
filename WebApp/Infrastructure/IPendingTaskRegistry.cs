namespace WebApp.Infrastructure
{
    /// <summary>
    /// Registry for managing pending tasks identified by a key.
    /// </summary>
    public interface IPendingTaskRegistry<TKey, TResult> where TKey : struct
    {
        /// <summary>
        /// Waits asynchronously for a response associated with the specified key.
        /// </summary>
        Task<TResult> WaitForResponseAsync(TKey key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Waits asynchronously for a response associated with the specified key with a timeout.
        /// </summary>
        Task<TResult> WaitForResponseAsync(TKey key, TimeSpan timeout, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to set the result for the specified key.
        /// </summary>
        bool TrySetResult(TKey key, TResult result);
    }
}
