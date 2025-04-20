using FlowLite.Core.Abstractions.Fsm;
using FlowLite.Diagnostics.Abstractions;
using FlowLite.Diagnostics.Observability;
namespace FlowLite.Diagnostics.Listeners;

internal sealed class DiagnosticObserverFlowLiteListener(string sourceName) : IDiagnosticsFlowLiteListener
{
    public void OnAttached<TState, TTrigger, TKey, TEntity>(
        IStateFlowMachine<TState, TTrigger, TKey, TEntity> fsm)
        where TState : struct
        where TTrigger : struct
        where TEntity : class
        where TKey : notnull
    {
        var observer = new DiagnosticsFlowLiteObserver<TState, TTrigger, TKey, TEntity>(sourceName);
        observer.Attach(fsm);
    }
}