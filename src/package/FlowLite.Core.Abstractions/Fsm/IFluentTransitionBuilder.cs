namespace FlowLite.Core.Abstractions.Fsm;

/// <summary>
/// Defines a fluent API for configuring state transitions in a state machine with support for marking final states.
/// </summary>
/// <typeparam name="TState">The type representing the state.</typeparam>
/// <typeparam name="TTrigger">The type representing the transition trigger.</typeparam>
/// <typeparam name="TKey">The type of the entity's key (e.g., int, Guid).</typeparam>
/// <typeparam name="TEntity">The type of the entity whose lifecycle is managed by the state machine.</typeparam>
public interface IFluentTransitionBuilder<in TState, TTrigger, TKey, out TEntity> 
    where TState : struct
    where TTrigger : struct
    where TEntity : class
    where TKey : notnull
{
    /// <summary>
    /// Marks the previously added transition as final.
    /// Final states do not allow further transitions.
    /// </summary>
    /// <returns>The current transition builder for chaining.</returns>
    IFluentTransitionBuilder<TState, TTrigger, TKey, TEntity> AsFinal();

    /// <summary>
    /// Adds a new transition from one state to another using the specified trigger and transition logic.
    /// </summary>
    /// <param name="fromState">The initial state of the transition.</param>
    /// <param name="trigger">The trigger that causes the transition.</param>
    /// <param name="toState">The resulting state after the transition.</param>
    /// <param name="onTransition">An asynchronous function that defines custom logic to be executed during the transition.
    /// The function receives a delegate to perform internal state transitions, and a transition context containing the entity.</param>
    /// <returns>The current transition builder for chaining.</returns>
    IFluentTransitionBuilder<TState, TTrigger, TKey, TEntity> AddTransition(
        TState fromState,
        TTrigger trigger,
        TState toState,
        Func<Func<TState, TTrigger?, ValueTask>, ITransitionContext<TEntity?>, ValueTask> onTransition);
}