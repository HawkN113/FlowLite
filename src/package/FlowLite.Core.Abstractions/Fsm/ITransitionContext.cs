namespace FlowLite.Core.Abstractions.Fsm;

/// <summary>
/// Represents the transition context passed during a state change in the state machine,
/// providing access to the associated entity and operations such as marking it for deletion.
/// </summary>
/// <typeparam name="TEntity">The type of the entity managed by the state machine.</typeparam>
public interface ITransitionContext<out TEntity>
{
    /// <summary>
    /// Gets the current entity associated with the state transition.
    /// </summary>
    TEntity? Entity { get; }
    /// <summary>
    /// Marks the current entity for deletion from persistent storage on the next state update.
    /// This should be called within a transition when the entity needs to be removed,
    /// such as when transitioning to a terminal (e.g., Deleted or Archived) state.
    /// </summary>
    void MarkForDeletion();
}