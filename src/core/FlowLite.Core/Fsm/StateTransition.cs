using FlowLite.Core.Abstractions.Fsm;
namespace FlowLite.Core.Fsm;

/// <summary>
/// Represents a state transition in a state machine, including the target state, 
/// the transition logic, and additional configuration options such as auto-saving 
/// and whether the state is the final state in the machine.
/// </summary>
/// <typeparam name="TState">The type representing the states in the state machine.</typeparam>
/// <typeparam name="TTrigger">The type representing the triggers that cause state transitions.</typeparam>
/// <typeparam name="TEntity">The type representing the entity on which the transition occurs.</typeparam>
public sealed class StateTransition<TState, TTrigger, TEntity>(
    TState toState,
    Func<Func<TState, TTrigger?, ValueTask>, ITransitionContext<TEntity?>, ValueTask> onTransition)
    where TState : struct
    where TTrigger : struct
    where TEntity : class
{
    /// <summary>
    /// Gets the target state to transition to.
    /// </summary>
    public TState ToState { get; } = toState;
    
    /// <summary>
    /// Get final state flag
    /// </summary>
    public bool IsFinal { get; private set; }

    /// <summary>
    /// Gets the function that defines the transition logic.
    /// </summary>
    public Func<Func<TState, TTrigger?, ValueTask>, ITransitionContext<TEntity?>, ValueTask> OnTransition { get; } = onTransition;
    
    /// <summary>
    /// Marks a specific state as a final state. Once reached, no transitions should occur from it.
    /// The state to mark as final.
    /// </summary>
    public StateTransition<TState, TTrigger, TEntity> AsFinal()
    {
        IsFinal = true;
        return this;
    }
}