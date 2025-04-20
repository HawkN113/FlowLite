using System.Diagnostics;
using FlowLite.Core.Abstractions.Fsm;
namespace FlowLite.Diagnostics.Observability;

internal sealed class DiagnosticsFlowLiteObserver<TState, TTrigger, TKey, TEntity>(string sourceName)
    where TState : struct
    where TTrigger : struct
    where TEntity : class
    where TKey : notnull
{
    private readonly DiagnosticListener _listener = new(sourceName);
    private const string StateChangedName = "StateChanged";
    private const string EntityChangedName = "EntityChanged";
    private const string EntityDeletedName = "EntityDeleted";
    private const string TransitionFailedName = "TransitionFailed";

    public void Attach(IStateFlowMachine<TState, TTrigger, TKey, TEntity> fsm)
    {
        fsm.OnStateChanged += (state, trigger) =>
            _listener.Write(StateChangedName, new { State = state.ToString(), Trigger = trigger.ToString() });
        fsm.OnEntityChanged += entity => { _listener.Write(EntityChangedName, new { Entity = entity }); };
        fsm.OnEntityDeleted += key => { _listener.Write(EntityDeletedName, new { Key = key?.ToString() }); };
        fsm.OnTransitionFailed += (state, trigger, ex) =>
        {
            _listener.Write(TransitionFailedName,
                new
                {
                    State = state.ToString(), Trigger = trigger.ToString(), Error = ex.Message, Source = ex.Source
                });
        };
    }
}