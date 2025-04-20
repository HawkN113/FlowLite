using FlowLite.Core.Abstractions.Fsm;
using FlowLite.Diagnostics.Abstractions;
using FlowLite.Diagnostics.Telemetry;
namespace FlowLite.Diagnostics.Listeners;

internal sealed class TelemetryDiagnosticsFlowLiteListener(string sourceName) : IDiagnosticsFlowLiteListener
{
    private readonly StateTelemetryEmitter _emitter = new(sourceName);

    public void OnAttached<TState, TTrigger, TKey, TEntity>(
        IStateFlowMachine<TState, TTrigger, TKey, TEntity> fsm)
        where TState : struct
        where TTrigger : struct
        where TEntity : class
        where TKey : notnull
    {
        fsm.OnStateChanged += (state, trigger) =>
        {
            _emitter.RecordTransition(state.ToString()!, trigger.ToString()!);
            _emitter.RecordStateChanged(state.ToString()!);
        };
        fsm.OnEntityChanged += _ =>
        {
            _emitter.RecordStateChanged(fsm.CurrentState.ToString()!);
            _emitter.RecordEntityChanged();
        };
        fsm.OnTransitionFailed += (state, trigger, key) =>
        {
            _emitter.RecordFailure(state.ToString()!, trigger.ToString()!);
            _emitter.RecordTransitionFailed();
        };
        fsm.OnEntityDeleted += key => _emitter.RecordEntityDeleted();
    }
}