using System.Diagnostics.Metrics;
namespace FlowLite.Diagnostics.Telemetry;

/// <summary>
/// Emits telemetry metrics for FSM transitions, durations, and lifecycle events.
/// </summary>
internal sealed class StateTelemetryEmitter
{
    private readonly Counter<int> _transitions;
    private readonly Counter<int> _failures;
    private readonly Counter<int> _stateChanges;
    private readonly Counter<int> _entityChanges;
    private readonly Counter<int> _entityDeletions;
    private readonly Counter<int> _transitionsFailed;
    private const string StateName = "state";
    private const string TriggerName = "trigger";
    private const string UnitName = "count";

    public StateTelemetryEmitter(string source)
    {
        var meter = new Meter(source, "1.0.0");
        _transitions = meter.CreateCounter<int>("fsm_transitions_total", unit: UnitName,
            description: "Total number of FSM transitions");
        _failures = meter.CreateCounter<int>("fsm_failures_total", unit: UnitName,
            description: "Total number of failed FSM transitions");
        _stateChanges = meter.CreateCounter<int>("fsm_state_changed_total", unit: UnitName,
            description: "Total number of state changes");
        _entityChanges = meter.CreateCounter<int>("fsm_entity_changed_total", unit: UnitName,
            description: "Total number of entity updates");
        _entityDeletions = meter.CreateCounter<int>("fsm_entity_deleted_total", unit: UnitName,
            description: "Total number of entity deletions");
        _transitionsFailed = meter.CreateCounter<int>("fsm_transition_failed_total", unit: UnitName,
            description: "Total number of failed transitions");
    }

    public void RecordTransition(string state, string trigger)
    {
        _transitions.Add(1, new KeyValuePair<string, object?>(StateName, state),
            new KeyValuePair<string, object?>(TriggerName, trigger));
    }

    public void RecordFailure(string state, string trigger)
    {
        _failures.Add(1, new KeyValuePair<string, object?>(StateName, state),
            new KeyValuePair<string, object?>(TriggerName, trigger));
    }

    public void RecordStateChanged(string state)
    {
        _stateChanges.Add(1, new KeyValuePair<string, object?>(StateName, state));
    }

    public void RecordEntityChanged()
    {
        _entityChanges.Add(1);
    }

    public void RecordEntityDeleted()
    {
        _entityDeletions.Add(1);
    }
    
    public void RecordTransitionFailed()
    {
        _transitionsFailed.Add(1);
    }
}