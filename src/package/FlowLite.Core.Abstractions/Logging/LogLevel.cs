namespace FlowLite.Core.Abstractions.Logging;

/// <summary>
/// Represents the severity level of log messages.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Indicates informational messages that are used for tracking general progress or state.
    /// </summary>
    Info,
    /// <summary>
    /// Indicates potential issues or situations that may require attention.
    /// </summary>
    Warning,
    /// <summary>
    /// Indicates errors or failures that need immediate attention or intervention.
    /// </summary>
    Error
}