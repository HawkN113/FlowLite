using FlowLite.Diagnostics.Abstractions;
namespace FlowLite.Diagnostics.Options;

/// <summary>
/// Global configuration for FlowLite diagnostics and observability.
/// </summary>
public sealed class DiagnosticsOptions
{
    /// <summary>
    /// Automatically attach diagnostics (FSM lifecycle hooks) to all created FSM instances.
    /// </summary>
    public bool EnableGlobalDiagnostics { get; set; } = true;

    /// <summary>
    /// Controls telemetry counters using OpenTelemetry / System.Diagnostics.Metrics.
    /// </summary>
    public TelemetryOptions Telemetry { get; set; } = new();

    /// <summary>
    /// Controls all logging behavior and built-in FSM console or ILogger logs.
    /// </summary>
    public LoggingOptions Logging { get; set; } = new();
    
    /// <summary>
    /// Options for emitting FSM events via DiagnosticListener for tracing and debugging tools.
    /// </summary>
    public DiagnosticObserverOptions DiagnosticObserver { get; set; } = new();

    /// <summary>
    /// Optional list of custom diagnostics listeners to attach globally.
    /// </summary>
    public List<IDiagnosticsFlowLiteListener> CustomListeners { get; } = [];
}