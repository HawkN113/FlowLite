namespace FlowLite.Core.Abstractions.Models;

/// <summary>
/// Represents the result of an operation, encapsulating success or failure.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public sealed class Result<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }
    
    /// <summary>
    /// Gets the value of the operation if it was successful; otherwise, it is <c>null</c>.
    /// </summary>
    public T? Value { get; }
    
    /// <summary>
    /// Gets the error message if the operation failed; otherwise, it is an empty string.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="value">The result value if successful.</param>
    /// <param name="errorMessage">The error message if the operation failed.</param>
    private Result(bool isSuccess, T? value, string errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Creates a successful result containing a value.
    /// </summary>
    /// <param name="value">The value to return.</param>
    /// <returns>A successful <see cref="Result{T}"/> instance.</returns>
    public static Result<T> Success(T value) => new (true, value, string.Empty);
    
    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    /// <param name="errorMessage">The error message describing the failure.</param>
    /// <returns>A failed <see cref="Result{T}"/> instance.</returns>
    public static Result<T> Failure(string errorMessage) => new (false, default(T), errorMessage);
}