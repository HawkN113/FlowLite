namespace FlowLite.Core.Abstractions.Helper;

internal sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
{
    private bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        semaphore.Release();
        _disposed = true;
    }
}