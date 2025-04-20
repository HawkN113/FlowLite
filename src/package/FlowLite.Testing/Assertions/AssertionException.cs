namespace FlowLite.Testing.Assertions;

/// <summary>
/// Exception used by the minimal assertion engine.
/// </summary>
public sealed class AssertionException(string message) : Exception($"[FlowLite.Assert] {message}")
{
    public static void ThrowIfNull(object? argument, string message)
    {
        if (argument is null)
            throw new AssertionException(message);
    }
}