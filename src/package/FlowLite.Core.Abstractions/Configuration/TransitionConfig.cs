using FlowLite.Core.Abstractions.Fsm;

namespace FlowLite.Core.Abstractions.Configuration;

/// <summary>
/// Represents a configuration for a state transition in a state machine.
/// Defines the source state, trigger, target state, and optional transition handlers.
/// </summary>
/// <typeparam name="TState">The type representing states.</typeparam>
/// <typeparam name="TTrigger">The type representing transition triggers.</typeparam>
/// <typeparam name="TEntity">The entity type associated with the state transition.</typeparam>
public sealed class TransitionConfig<TState, TTrigger, TEntity>
    where TState : struct
    where TTrigger : struct
    where TEntity : class
{
    /// <summary>
    /// The state from which the transition starts.
    /// </summary>
    public required TState FromState { get; set; }
    
    /// <summary>
    /// The trigger that causes the transition.
    /// </summary>
    public required TTrigger Trigger { get; set; }
    
    /// <summary>
    /// The state to which the transition leads. 
    /// </summary>
    public TState ToState { get; set; }
    
    /// <summary>
    /// Get final state flag
    /// </summary>
    public bool IsFinal { get; set; }
    
    /// <summary>
    /// Delegate that executes during the transition.
    /// The provided function allows checking conditions before completing the transition.
    /// </summary>
    public Func<Func<TState, TTrigger?, ValueTask>, ITransitionContext<TEntity?>, ValueTask>? OnTransition { get; set; }
}