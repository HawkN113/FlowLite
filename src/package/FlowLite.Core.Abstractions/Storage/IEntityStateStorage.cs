namespace FlowLite.Core.Abstractions.Storage;

/// <summary>
/// Defines a storage mechanism for saving and retrieving entity states in a finite state machine.
/// </summary>
/// <typeparam name="TState">The type representing the state.</typeparam>
/// <typeparam name="TKey">The type of the entity key (e.g., Guid or int).</typeparam>
/// <typeparam name="TEntity">The type of the entity associated with the state.</typeparam>
public interface IEntityStateStorage<TState, in TKey, TEntity> where TKey : notnull
{
    /// <summary>
    /// Saves the current state and entity to storage.
    /// </summary>
    /// <param name="key">The unique key identifying the entity.</param>
    /// <param name="state">The state to save.</param>
    /// <param name="entity">The entity to save.</param>
    Task SaveStateAsync(TKey key, TState? state, TEntity? entity);

    /// <summary>
    /// Loads the state of an entity from storage.
    /// </summary>
    /// <param name="key">The unique key identifying the entity.</param>
    /// <returns>The state associated with the given key.</returns>
    Task<TState?> LoadStateAsync(TKey key);

    /// <summary>
    /// Loads the entity from storage.
    /// </summary>
    /// <param name="key">The unique key identifying the entity.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<TEntity?> LoadEntityAsync(TKey key);
    
    /// <summary>
    /// Check key in the storage
    /// </summary>
    /// <param name="key">The unique key identifying the entity.</param>
    /// <returns>The entry if found; otherwise, false.</returns>
    Task<bool> ExistEntryByIdAsync(TKey key);
    
    /// <summary>
    /// Delete the entry from storage.
    /// </summary>
    /// <param name="key">The unique key identifying the entity.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<bool> DeleteEntryAsync(TKey key);
}