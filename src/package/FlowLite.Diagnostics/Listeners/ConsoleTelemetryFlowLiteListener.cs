using FlowLite.Core.Abstractions.Fsm;
using FlowLite.Diagnostics.Abstractions;
using FlowLite.Diagnostics.Common;
namespace FlowLite.Diagnostics.Listeners;

internal sealed class ConsoleTelemetryFlowLiteListener : IDiagnosticsFlowLiteListener
{
    public void OnAttached<TState, TTrigger, TKey, TEntity>(
        IStateFlowMachine<TState, TTrigger, TKey, TEntity> fsm)
        where TState : struct
        where TTrigger : struct
        where TEntity : class
        where TKey : notnull
    {
        StateHandler.RegisterEvents(fsm);
    }
}