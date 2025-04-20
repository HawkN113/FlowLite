using FlowLite.Core.Abstractions.Fsm;
namespace FlowLite.Diagnostics.Abstractions;

public interface IDiagnosticsFlowLiteListener
{
    void OnAttached<TState, TTrigger, TKey, TEntity>(
        IStateFlowMachine<TState, TTrigger, TKey, TEntity> fsm)
        where TState : struct
        where TTrigger : struct
        where TEntity : class
        where TKey : notnull;
}