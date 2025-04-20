using FlowLite.Core.Abstractions.Storage;
using FlowLite.Core.Storage;
using FlowLite.Core.Templates;
using Microsoft.Extensions.DependencyInjection;
namespace FlowLite.Core.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a storage implementation for FlowLite (Singleton).
    /// A separate instance should be created for each entity type.
    /// </summary>
    /// <param name="services">The service collection to register the storage in.</param>
    /// <param name="storageType">The type of storage to use.</param>
    /// <param name="storageDirectory">The directory where storage files will be saved (if applicable).</param>
    /// <typeparam name="TState">The type representing the entity state.</typeparam>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddFlowLiteStorage<TState, TKey, TEntity>(this IServiceCollection services,
        StorageType storageType, string storageDirectory = "")
        where TState : struct where TEntity : class where TKey : notnull
    {
        var storage = StorageFactory.Create<TState, TKey, TEntity>(storageType, storageDirectory);
        services.AddSingleton(storage);
        return services;
    }

    /// <summary>
    /// Retrieves the FlowLite storage instance from the service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve the storage instance.</param>
    /// <typeparam name="TState">The type representing the entity state.</typeparam>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>The storage instance for the specified entity type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the storage instance is not found.</exception>
    public static IEntityStateStorage<TState, TKey, TEntity>
        GetFlowLiteStorage<TState, TKey, TEntity>(this IServiceProvider serviceProvider) where TState : struct
        where TEntity : class
        where TKey : notnull
    {
        var storage = serviceProvider.GetRequiredService<IEntityStateStorage<TState, TKey, TEntity>>();
        if (storage is null)
            throw new InvalidOperationException(ErrorTemplates.StorageNotFoundTemplate);
        return storage;
    }
}