namespace FlowLite.Core.Abstractions.Helper;

internal sealed class AsyncLock
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<IDisposable> LockAsync()
    {
        await _semaphore.WaitAsync();
        return new Releaser(_semaphore);
    }
}