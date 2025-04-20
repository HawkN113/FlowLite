using FlowLite.Core.Abstractions.Fsm;
using FlowLite.Diagnostics.Abstractions;
namespace FlowLite.Diagnostics;

internal static class DiagnosticsFlowLiteRegistry
{
    private static readonly List<IDiagnosticsFlowLiteListener> Listeners = [];

    public static void Register(IDiagnosticsFlowLiteListener listener) => Listeners.Add(listener);

    public static void ApplyAll<TState, TTrigger, TKey, TEntity>(
        IStateFlowMachine<TState, TTrigger, TKey, TEntity> fsm)
        where TState : struct
        where TTrigger : struct
        where TEntity : class
        where TKey : notnull
    {
        foreach (var listener in Listeners)
        {
            listener.OnAttached(fsm);
        }
    }
}