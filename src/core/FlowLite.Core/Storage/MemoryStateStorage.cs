using System.Collections.Concurrent;
using FlowLite.Core.Abstractions.Storage;
namespace FlowLite.Core.Storage;

internal sealed class MemoryStateStorage<TState, TKey, TEntity> :
    IEntityStateStorage<TState, TKey, TEntity> 
    where TKey : notnull 
    where TEntity : class
{
    private readonly ConcurrentDictionary<TKey, (TState? State, TEntity? Entity)> _storage = new();

    public Task SaveStateAsync(TKey key, TState? state, TEntity? entity)
    {
        _storage[key] = (state, entity);
        return Task.CompletedTask;
    }

    public Task<TState?> LoadStateAsync(TKey key)
    {
        return Task.FromResult(_storage.TryGetValue(key, out var entry) ? entry.State : default!);
    }

    public Task<TEntity?> LoadEntityAsync(TKey key)
    {
        return Task.FromResult(_storage.TryGetValue(key, out var entry) ? entry.Entity : default);
    }

    public async Task<bool> ExistEntryByIdAsync(TKey key)
    {
        return await Task.FromResult(_storage.TryGetValue(key, out _));
    }

    public async Task<bool> DeleteEntryAsync(TKey key)
    {
        _storage.TryGetValue(key, out var entry);
        if (entry.Entity is null) return await Task.FromResult(false);
        _storage.TryRemove(key, out _);
        return await Task.FromResult(true);
    }
}