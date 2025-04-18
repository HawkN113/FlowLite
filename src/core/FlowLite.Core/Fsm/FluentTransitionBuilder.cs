using FlowLite.Core.Abstractions.Fsm;
namespace FlowLite.Core.Fsm;

public sealed class FluentTransitionBuilder<TState, TTrigger, TKey, TEntity>(
    StateFlowMachine<TState, TTrigger, TKey, TEntity> machine,
    TState lastToState): IFluentTransitionBuilder<TState, TTrigger, TKey, TEntity>  where TState : struct
    where TTrigger : struct
    where TEntity : class
    where TKey : notnull
{
    public IFluentTransitionBuilder<TState, TTrigger, TKey, TEntity> AsFinal()
    {
        machine.MarkFinalState(lastToState);
        return this;
    }

    public IFluentTransitionBuilder<TState, TTrigger, TKey, TEntity> AddTransition(
        TState fromState,
        TTrigger trigger,
        TState toState,
        Func<Func<TState, TTrigger?, ValueTask>, ITransitionContext<TEntity?>, ValueTask> onTransition)
    {
        return machine.AddTransition(fromState, trigger, toState, onTransition);
    }
}