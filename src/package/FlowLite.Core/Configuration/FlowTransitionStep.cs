using FlowLite.Core.Abstractions.Configuration;
using FlowLite.Core.Abstractions.Fsm;

namespace FlowLite.Core.Configuration;

public sealed class FlowTransitionStep<TState, TTrigger, TEntity>(
    FlowTransitionBuilder<TState, TTrigger, TEntity> builder,
    TransitionConfig<TState, TTrigger, TEntity> config) :IFlowTransitionStep<TState, TTrigger, TEntity>
    where TState : struct
    where TTrigger : struct
    where TEntity : class
{
    /// <summary>
    /// Marks a specific state as a final state. Once reached, no transitions should occur from it.
    /// The state to mark as final.
    /// </summary>
    public IFlowTransitionStep<TState, TTrigger, TEntity> AsFinal()
    {
        config.IsFinal = true;
        return this;
    }

    /// <summary>
    /// Adds another transition after this one.
    /// </summary>
    public IFlowTransitionStep<TState, TTrigger, TEntity> AddTransition(
        TState fromState,
        TTrigger trigger,
        TState toState,
        Func<Func<TState, TTrigger?, ValueTask>, ITransitionContext<TEntity?>, ValueTask> onTransition)
    {
        return builder.AddTransition(fromState, trigger, toState, onTransition);
    }

    /// <summary>
    /// Returns the builder if you need to access the full API.
    /// </summary>
    public IFlowTransitionBuilder<TState, TTrigger, TEntity> Build() => builder;
}
