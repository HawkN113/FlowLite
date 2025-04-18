namespace FlowLite.Testing.Assertions;

/// <summary>
/// Minimal dependency-free assertion helpers for FlowLite FSM testing.
/// Provides lightweight and self-contained methods for validating test expectations.
/// </summary>
public static class MinimalAssert
{
    /// <summary>
    /// Asserts that the specified condition is true. Throws an exception if it's false.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="message">The message to include in the exception if the assertion fails.</param>
    public static void IsTrue(bool condition, string message)
    {
        if (!condition)
            throw new AssertionException(message);
    }
    
    /// <summary>
    /// Asserts that the specified condition is false. Throws an exception if it's true.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="message">The message to include in the exception if the assertion fails.</param>
    public static void IsFalse(bool condition, string message)
    {
        if (condition)
            throw new AssertionException(message);
    }

    /// <summary>
    /// Asserts that two values are equal. Throws an exception if they are not equal.
    /// </summary>
    /// <typeparam name="T">The type of values to compare.</typeparam>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The actual value to verify.</param>
    /// <param name="message">The message to include in the exception if the assertion fails.</param>
    public static void AreEqual<T>(T expected, T actual, string message)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new AssertionException($"{message}. Expected: {expected}, Actual: {actual}");
    }

    /// <summary>
    /// Asserts that the given value is not null. Throws an exception if it is null.
    /// </summary>
    /// <param name="value">The object to check for null.</param>
    /// <param name="message">The message to include in the exception if the assertion fails.</param>
    public static void IsNotNull(object? value, string message)
    {
        if (value is null)
            AssertionException.ThrowIfNull(value, message);
    }

    /// <summary>
    /// Asserts that a sequence contains at least one element matching the predicate.
    /// Throws an exception if no such element is found.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="predicate">The condition to evaluate.</param>
    /// <param name="message">The message to include in the exception if the assertion fails.</param>
    public static void Contains<T>(IEnumerable<T> source, Func<T, bool> predicate, string message)
    {
        if (!source.Any(predicate))
            throw new AssertionException(message);
    }

    /// <summary>
    /// Asserts that two sequences are equal. Throws an exception if they are not equal.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequences.</typeparam>
    /// <param name="expected">The expected sequence.</param>
    /// <param name="actual">The actual sequence to verify.</param>
    /// <param name="message">The message to include in the exception if the assertion fails.</param>
    public static void SequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, string message)
    {
        if (!expected.SequenceEqual(actual))
            throw new AssertionException(message);
    }
}