using FlowLite.Core.Abstractions.Fsm;

namespace FlowLite.Core.Abstractions.Configuration;

/// <summary>
/// Represents a fluent builder interface for defining state transitions in a finite state machine (FSM).
/// </summary>
/// <typeparam name="TState">The type representing the FSM states (must be a struct/enum).</typeparam>
/// <typeparam name="TTrigger">The type representing the triggers (must be a struct/enum).</typeparam>
/// <typeparam name="TEntity">The entity type that the FSM operates on (must be a class).</typeparam>

public interface IFlowTransitionBuilder<TState, TTrigger, TEntity>
    where TState : struct
    where TTrigger : struct
    where TEntity : class
{
    /// <summary>
    /// Gets a read-only list of all configured state transitions.
    /// </summary>
    IReadOnlyList<TransitionConfig<TState, TTrigger, TEntity>> Transitions { get; }

    /// <summary>
    /// Adds a new transition to the FSM configuration.
    /// </summary>
    /// <param name="fromState">The starting state of the transition.</param>
    /// <param name="trigger">The trigger that causes the transition.</param>
    /// <param name="toState">The resulting state after the transition.</param>
    /// <param name="onTransition">The asynchronous action executed during the transition.</param>
    /// <returns>An interface allowing further configuration of the transition (e.g., marking it as final).</returns>
    IFlowTransitionStep<TState, TTrigger, TEntity> AddTransition(
        TState fromState,
        TTrigger trigger,
        TState toState,
        Func<Func<TState, TTrigger?, ValueTask>, ITransitionContext<TEntity?>, ValueTask> onTransition);
}