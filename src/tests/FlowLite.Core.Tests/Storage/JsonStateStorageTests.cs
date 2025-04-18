using FlowLite.Core.Storage;
namespace FlowLite.Core.Tests.Storage;

public sealed class JsonStateStorageTests: IDisposable
{
    private readonly string _storageDir = "/mocked/storage";
    private readonly string _tempDirectory;

    private enum SampleState { New, Processed }

    private class SampleEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    
    public JsonStateStorageTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenDirectoryIsNull()
    {
        Assert.Throws<ArgumentException>(() => new JsonStateStorage<string, string, object>(null!));
    }

    [Fact]
    public void Constructor_ShouldCreateStorage_WhenFileDoesNotExist()
    {
        var storage = new JsonStateStorage<string, string, object>(_storageDir);
        Assert.NotNull(storage);
    }

    [Fact]
    public async Task LoadStateAsync_ShouldReturnDefault_WhenKeyDoesNotExist()
    {
        var storage = new JsonStateStorage<string, string, object>(_storageDir);
        var state = await storage.LoadStateAsync("missingKey");
        Assert.Null(state);
    }

    [Fact]
    public async Task LoadEntityAsync_ShouldReturnNull_WhenKeyDoesNotExist()
    {
        var storage = new JsonStateStorage<string, string, object>(_storageDir);
        var entity = await storage.LoadEntityAsync("missingKey");
        Assert.Null(entity);
    }
    
    [Fact]
    public async Task SaveStateAsync_ShouldPersistStateAndEntity()
    {
        // Arrange
        var storage = new JsonStateStorage<SampleState, int, SampleEntity>(_tempDirectory);
        var entity = new SampleEntity { Id = 1, Name = "Test" };
        var state = SampleState.Processed;

        // Act
        await storage.SaveStateAsync(entity.Id, state, entity);

        // Assert
        var loadedState = await storage.LoadStateAsync(entity.Id);
        var loadedEntity = await storage.LoadEntityAsync(entity.Id);

        Assert.Equal(state, loadedState);
        Assert.NotNull(loadedEntity);
        Assert.Equal(entity.Id, loadedEntity!.Id);
        Assert.Equal(entity.Name, loadedEntity.Name);
    }

    [Fact]
    public async Task ExistEntryByIdAsync_ShouldReturnTrue_WhenEntryExists()
    {
        // Arrange
        var storage = new JsonStateStorage<SampleState, int, SampleEntity>(_tempDirectory);
        var entity = new SampleEntity { Id = 2, Name = "ExistsCheck" };
        var state = SampleState.New;

        await storage.SaveStateAsync(entity.Id, state, entity);

        // Act
        var exists = await storage.ExistEntryByIdAsync(entity.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistEntryByIdAsync_ShouldReturnFalse_WhenEntryDoesNotExist()
    {
        // Arrange
        var storage = new JsonStateStorage<SampleState, int, SampleEntity>(_tempDirectory);

        // Act
        var exists = await storage.ExistEntryByIdAsync(999);

        // Assert
        Assert.False(exists);
    }
    
    [Fact]
    public async Task DeleteEntryAsync_ShouldRemoveEntry_WhenEntityExists()
    {
        // Arrange
        var storage = new JsonStateStorage<string, Guid, SampleEntity>(_tempDirectory);
        var key = Guid.NewGuid();
        var state = "Active";
        var entity = new SampleEntity() { Id = 1, Name = "Test" };

        await storage.SaveStateAsync(key, state, entity);
        var existsBefore = await storage.ExistEntryByIdAsync(key);
        Assert.True(existsBefore);

        // Act
        var deleted = await storage.DeleteEntryAsync(key);

        // Assert
        Assert.True(deleted);
        var existsAfter = await storage.ExistEntryByIdAsync(key);
        Assert.False(existsAfter);
    }

    [Fact]
    public async Task DeleteEntryAsync_ShouldReturnFalse_WhenEntityIsNull()
    {
        // Arrange
        var storage = new JsonStateStorage<string, Guid, SampleEntity>(_tempDirectory);
        var key = Guid.NewGuid();
        await storage.SaveStateAsync(key, "State", null);

        // Act
        var deleted = await storage.DeleteEntryAsync(key);

        // Assert
        Assert.False(deleted);
    }

    [Fact]
    public async Task DeleteEntryAsync_ShouldReturnFalse_WhenEntryDoesNotExist()
    {
        // Arrange
        var storage = new JsonStateStorage<string, Guid, SampleEntity>(_tempDirectory);
        var nonExistentKey = Guid.NewGuid();

        // Act
        var deleted = await storage.DeleteEntryAsync(nonExistentKey);

        // Assert
        Assert.False(deleted);
    }
    
    private bool _disposed;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing && Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
        _disposed = true;
    }
}
