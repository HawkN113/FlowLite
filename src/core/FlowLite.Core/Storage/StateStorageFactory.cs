using FlowLite.Core.Abstractions.Storage;
using FlowLite.Core.Templates;
namespace FlowLite.Core.Storage;

/// <summary>
/// Factory for creating instances of entity state storage.
/// </summary>
public static class StorageFactory
{
    /// <summary>
    /// Creates an instance of an entity state storage based on the specified storage type.
    /// </summary>
    /// <typeparam name="TState">The type representing the state.</typeparam>
    /// <typeparam name="TKey">The type of the entity key (e.g., Guid or int).</typeparam>
    /// <typeparam name="TEntity">The type of the entity associated with the state.</typeparam>
    /// <param name="storageType">The type of storage to use (e.g., JSON or in-memory).</param>
    /// <param name="storageDirectory">The directory path for storage (applicable to JSON storage).</param>
    /// <returns>An instance of <see cref="IEntityStateStorage{TState, TKey, TEntity}"/> based on the selected storage type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an unknown storage type is provided.</exception>
    public static IEntityStateStorage<TState, TKey, TEntity> Create<TState, TKey, TEntity>(
        StorageType storageType, string storageDirectory = "")
        where TState : struct 
        where TEntity : class 
        where TKey : notnull
    {
        return storageType switch
        {
            StorageType.Json => new JsonStateStorage<TState, TKey, TEntity>(storageDirectory),
            StorageType.Memory => new MemoryStateStorage<TState, TKey, TEntity>(),
            _ => throw new InvalidOperationException(ErrorTemplates.UnknownStorageTypeTemplate)
        };
    }
}