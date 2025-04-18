using FlowLite.Core.Abstractions.Storage;
using FlowLite.Core.Extensions;
using FlowLite.Core.Storage;
using Microsoft.Extensions.DependencyInjection;
namespace FlowLite.Core.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddFlowLiteStorage_ShouldAddStorageToServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        const StorageType storageType = StorageType.Memory;

        // Act
        services.AddFlowLiteStorage<int, string, string>(storageType);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var storage = serviceProvider.GetService<IEntityStateStorage<int, string, string>>();

        Assert.NotNull(storage);
        Assert.IsType<MemoryStateStorage<int, string, string>>(storage);
    }

    [Fact]
    public void GetFlowLiteStorage_ShouldReturnRegisteredStorage()
    {
        // Arrange
        var services = new ServiceCollection();
        var storageType = StorageType.Memory;

        // Act
        services.AddFlowLiteStorage<int, string, string>(storageType);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var storage = serviceProvider.GetFlowLiteStorage<int, string, string>();

        // Assert
        Assert.NotNull(storage);
        Assert.IsType<MemoryStateStorage<int, string, string>>(storage);
    }

    [Fact]
    public void GetFlowLiteStorage_ShouldThrowExceptionIfStorageNotFound()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            serviceProvider.GetFlowLiteStorage<int, string, string>());

        Assert.Contains("No service for type", exception.Message);
    }
}