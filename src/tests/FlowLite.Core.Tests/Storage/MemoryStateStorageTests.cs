using FlowLite.Core.Storage;
namespace FlowLite.Core.Tests.Storage;

public class MemoryStateStorageTests
{
    private record TestEntity(int Id, string Name);
    
    [Fact]
    public async Task SaveStateAsync_ShouldSaveStateCorrectly()
    {
        // Arrange
        var storage = new MemoryStateStorage<int, string, string>();
        var key = "testKey";
        var state = 42;
        var entity = "TestEntity";

        // Act
        await storage.SaveStateAsync(key, state, entity);

        // Assert
        var savedState = await storage.LoadStateAsync(key);
        var savedEntity = await storage.LoadEntityAsync(key);

        Assert.Equal(state, savedState);
        Assert.Equal(entity, savedEntity);
    }

    [Fact]
    public async Task LoadStateAsync_ShouldReturnDefault_WhenKeyDoesNotExist()
    {
        // Arrange
        var storage = new MemoryStateStorage<int, string, string>();

        // Act
        var result = await storage.LoadStateAsync("nonExistentKey");

        // Assert
        Assert.Equal(default, result);
    }

    [Fact]
    public async Task LoadEntityAsync_ShouldReturnNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var storage = new MemoryStateStorage<int, string, string>();

        // Act
        var result = await storage.LoadEntityAsync("nonExistentKey");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SaveStateAsync_ShouldOverwriteExistingState()
    {
        // Arrange
        var storage = new MemoryStateStorage<int, string, string>();
        var key = "testKey";
        var initialState = 1;
        var initialEntity = "Entity1";
        var newState = 42;
        var newEntity = "Entity2";

        // Act
        await storage.SaveStateAsync(key, initialState, initialEntity);
        await storage.SaveStateAsync(key, newState, newEntity);

        // Assert
        var savedState = await storage.LoadStateAsync(key);
        var savedEntity = await storage.LoadEntityAsync(key);

        Assert.Equal(newState, savedState);
        Assert.Equal(newEntity, savedEntity);
    }

    [Fact]
    public async Task LoadStateAsync_ShouldReturnDefault_WhenNoStateWasSaved()
    {
        // Arrange
        var storage = new MemoryStateStorage<int, string, string>();
        var key = "newKey";

        // Act
        var state = await storage.LoadStateAsync(key);

        // Assert
        Assert.Equal(default, state);
    }

    [Fact]
    public async Task LoadEntityAsync_ShouldReturnNull_WhenNoEntityWasSaved()
    {
        // Arrange
        var storage = new MemoryStateStorage<int, string, string>();
        var key = "newKey";

        // Act
        var entity = await storage.LoadEntityAsync(key);

        // Assert
        Assert.Null(entity);
    }
    
     [Fact]
    public async Task ExistEntryByIdAsync_ShouldReturnTrue_WhenEntryExists()
    {
        // Arrange
        var storage = new MemoryStateStorage<string, Guid, TestEntity>();
        var key = Guid.NewGuid();
        var state = "Initialized";
        var entity = new TestEntity(1, "Test");

        await storage.SaveStateAsync(key, state, entity);

        // Act
        var exists = await storage.ExistEntryByIdAsync(key);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistEntryByIdAsync_ShouldReturnFalse_WhenEntryDoesNotExist()
    {
        // Arrange
        var storage = new MemoryStateStorage<string, Guid, TestEntity>();
        var missingKey = Guid.NewGuid();

        // Act
        var exists = await storage.ExistEntryByIdAsync(missingKey);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteEntryAsync_ShouldReturnTrue_AndRemoveEntry_WhenEntityExists()
    {
        // Arrange
        var storage = new MemoryStateStorage<string, Guid, TestEntity>();
        var key = Guid.NewGuid();
        var entity = new TestEntity(2, "DeleteMe");

        await storage.SaveStateAsync(key, "AnyState", entity);

        // Act
        var deleted = await storage.DeleteEntryAsync(key);
        var exists = await storage.ExistEntryByIdAsync(key);

        // Assert
        Assert.True(deleted);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteEntryAsync_ShouldReturnFalse_WhenEntityIsNull()
    {
        // Arrange
        var storage = new MemoryStateStorage<string, Guid, TestEntity>();
        var key = Guid.NewGuid();

        await storage.SaveStateAsync(key, "SomeState", null);

        // Act
        var deleted = await storage.DeleteEntryAsync(key);
        var exists = await storage.ExistEntryByIdAsync(key);

        // Assert
        Assert.False(deleted);
        Assert.True(exists);
    }

    [Fact]
    public async Task DeleteEntryAsync_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        // Arrange
        var storage = new MemoryStateStorage<string, Guid, TestEntity>();
        var key = Guid.NewGuid();

        // Act
        var result = await storage.DeleteEntryAsync(key);

        // Assert
        Assert.False(result);
    }
}