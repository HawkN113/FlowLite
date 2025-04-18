using FlowLite.Core.Abstractions.Fsm;
namespace FlowLite.Testing.Assertions;

/// <summary>
/// Provides fluent assertions for verifying FSM transition history.
/// Useful for checking triggers, states, order, and history length.
/// </summary>
/// <typeparam name="TState">The type representing FSM states.</typeparam>
/// <typeparam name="TTrigger">The type representing FSM triggers.</typeparam>
/// <typeparam name="TKey">The type representing the entity key.</typeparam>
/// <typeparam name="TEntity">The type of the FSM entity.</typeparam>
public sealed class FsmHistoryAssertions<TState, TTrigger, TKey, TEntity>(
    IStateFlowMachine<TState, TTrigger, TKey, TEntity> fsm)
    where TState : struct
    where TTrigger : struct
    where TEntity : class
    where TKey : notnull
{
    /// <summary>
    /// Asserts that the history contains a specific trigger and resulting state.
    /// </summary>
    public FsmHistoryAssertions<TState, TTrigger, TKey, TEntity> Contains(TTrigger trigger, TState state)
    {
        var history = fsm.GetTransitionHistory();
        MinimalAssert.Contains(history, h => h.Trigger!.Equals(trigger) && h.State.Equals(state),
            $"The history does not contain the expected state '{state}' and trigger '{trigger}'.");
        return this;
    }

    /// <summary>
    /// Asserts that the sequence of recorded states matches the expected order.
    /// </summary>
    public FsmHistoryAssertions<TState, TTrigger, TKey, TEntity> HaveInHistory(params TState[] expectedStates)
    {
        var history = fsm.GetTransitionHistory().Select(h => h.State).ToArray();
        MinimalAssert.SequenceEqual(expectedStates, history, "FSM transition history mismatch.");
        return this;
    }
    
    /// <summary>
    /// Asserts that the total number of history entries matches the expected count.
    /// </summary>
    public FsmHistoryAssertions<TState, TTrigger, TKey, TEntity> HaveLength(int expected)
    {
        MinimalAssert.AreEqual(expected, fsm.GetTransitionHistory().Count,
            "The history length is not equal to the expected length.");
        return this;
    }

    /// <summary>
    /// Asserts that the first entry in the history matches the expected state.
    /// </summary>
    public FsmHistoryAssertions<TState, TTrigger, TKey, TEntity> StartWith(TState state)
    {
        var first = fsm.GetTransitionHistory().FirstOrDefault();
        MinimalAssert.AreEqual(state, first.State,
            $"Expected state {state} was not found in the history like as first state.");
        return this;
    }
    
    /// <summary>
    /// Asserts that the specified trigger was used in any transition in the history.
    /// </summary>
    public FsmHistoryAssertions<TState, TTrigger, TKey, TEntity> ContainsTrigger(TTrigger trigger)
    {
        MinimalAssert.Contains(fsm.GetTransitionHistory(),
            x => x.Trigger!.Equals(trigger),
            $"Expected trigger {trigger} was not found in the history.");
        return this;
    }
}