namespace FlowLite.Diagnostics.Options;

/// <summary>
/// Configuration options for FSM DiagnosticListener integration.
/// </summary>
public sealed class DiagnosticObserverOptions
{
    /// <summary>Enable FSM event publishing via DiagnosticListener.</summary>
    public bool Enabled { get; set; }
    /// <summary>Event source name used in diagnostics (default: FlowLite.Diagnostics).</summary>
    public string Source { get; set; } = "FlowLite.Diagnostics";
}