using FlowLite.Core.Abstractions.Fsm;

namespace FlowLite.Core.Abstractions.Configuration;

public interface IFlowTransitionStep<TState, TTrigger, TEntity>
    where TState : struct
    where TTrigger : struct
    where TEntity : class
{
    /// <summary>
    /// Marks a specific state as a final state. Once reached, no transitions should occur from it.
    /// The state to mark as final.
    /// </summary>
    IFlowTransitionStep<TState, TTrigger, TEntity> AsFinal();

    /// <summary>
    /// Adds another transition after this one.
    /// </summary>
    IFlowTransitionStep<TState, TTrigger, TEntity> AddTransition(
        TState fromState,
        TTrigger trigger,
        TState toState,
        Func<Func<TState, TTrigger?, ValueTask>, ITransitionContext<TEntity?>, ValueTask> onTransition);

    /// <summary>
    /// Returns the builder if you need to access the full API.
    /// </summary>
    IFlowTransitionBuilder<TState, TTrigger, TEntity> Build();
}
