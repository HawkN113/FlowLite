using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FlowLite.Core.Abstractions.Storage;
using FlowLite.Core.Converters;
namespace FlowLite.Core.Storage;

internal sealed class JsonStateStorage<TState, TKey, TEntity> : 
    IEntityStateStorage<TState, TKey, TEntity> 
    where TKey : notnull 
    where TEntity : class
{
    private readonly string _filePath;
    private ConcurrentDictionary<TKey, (TState? State, TEntity? Entity)> _storage = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = false,
        Converters =
        {
            new TupleConverter<TState, TEntity>()
        }
    };

    public JsonStateStorage(string storageDirectory)
    {
        if (string.IsNullOrWhiteSpace(storageDirectory))
            throw new ArgumentException("Storage directory is required", nameof(storageDirectory));
        _filePath = Path.Combine(storageDirectory, $"storage_{typeof(TEntity).Name.ToLower()}.json");
        LoadFromFile();
    }

    public async Task SaveStateAsync(TKey key, TState? state, TEntity? entity)
    {
        _storage[key] = (state, entity);
        await SaveToFile();
    }

    public Task<TState?> LoadStateAsync(TKey key)
    {
        return Task.FromResult(_storage.TryGetValue(key, out var entry) ? entry.State : default!);
    }

    public Task<TEntity?> LoadEntityAsync(TKey key)
    {
        return Task.FromResult(_storage.TryGetValue(key, out var entry) ? entry.Entity : null);
    }

    public async Task<bool> ExistEntryByIdAsync(TKey key)
    {
        return await Task.FromResult(_storage.ContainsKey(key));
    }

    public async Task<bool> DeleteEntryAsync(TKey key)
    {
        _storage.TryGetValue(key, out var entry);
        if (entry.Entity is null) return false;
        _storage.TryRemove(key, out _);
        await SaveToFile();
        return true;
    }

    private void LoadFromFile()
    {
        if (!File.Exists(_filePath)) return;

        try
        {
            var json = File.ReadAllText(_filePath);
            _storage = JsonSerializer.Deserialize<ConcurrentDictionary<TKey, (TState?, TEntity?)>>(json, _jsonOptions)
                       ?? new ConcurrentDictionary<TKey, (TState?, TEntity?)>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"[JsonStateStorage] Load error: {ex.Message}");
        }
    }

    private async Task SaveToFile()
    {
        try
        {
            var json = JsonSerializer.Serialize(_storage, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"[JsonStateStorage] Save error: {ex.Message}");
        }
    }
}