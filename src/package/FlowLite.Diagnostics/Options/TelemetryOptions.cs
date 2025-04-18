namespace FlowLite.Diagnostics.Options;

/// <summary>
/// Configuration for FSM telemetry metrics.
/// </summary>
public sealed class TelemetryOptions
{
    /// <summary>Enable OpenTelemetry metrics via System.Diagnostics.Metrics.</summary>
    public bool Enabled { get; set; }

    /// <summary>Telemetry meter source name (default: FlowLite.FSM).</summary>
    public string Source { get; set; } = "FlowLite.FSM";
}