namespace FlowLite.Core.Abstractions.Storage;

/// <summary>
/// Defines the available storage types for storing entity states.
/// </summary>
public enum StorageType
{
    /// <summary>
    /// In-memory storage that keeps data only during the application's runtime.
    /// </summary>
    Memory,
    /// <summary>
    /// JSON-based storage that persists data in JSON files.
    /// </summary>
    Json
}