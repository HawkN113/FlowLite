using Microsoft.Extensions.Logging;
namespace FlowLite.Diagnostics.Options;

/// <summary>
/// Configuration for logging FSM events.
/// </summary>
public sealed class LoggingOptions
{
    /// <summary>Enable options</summary>
    public bool Enabled { get; set; }

    /// <summary>Enable console logging for FSM lifecycle events.</summary>
    public bool UseConsole { get; set; }
    
    /// <summary>Enable Microsoft.Extensions.Logging-based FSM logging.</summary>
    public bool UseLogger { get; set; }

    /// <summary>Logger factory instance (only needed if UseLogger is true).</summary>
    public ILoggerFactory? LoggerFactory { get; set; }
}