using FlowLite.Core.Abstractions.Fsm;
namespace FlowLite.Testing.Assertions;

/// <summary>
/// Provides fluent-style assertions for verifying FSM behavior such as allowed triggers,
/// current state, entity state, logs, and history.
/// </summary>
/// <typeparam name="TState">The type representing FSM states.</typeparam>
/// <typeparam name="TTrigger">The type representing FSM triggers.</typeparam>
/// <typeparam name="TKey">The type of the entity key.</typeparam>
/// <typeparam name="TEntity">The entity type handled by the FSM.</typeparam>
public sealed class FsmShouldAssertions<TState, TTrigger, TKey, TEntity>(
    IStateFlowMachine<TState, TTrigger, TKey, TEntity> fsm)
    where TState : struct
    where TTrigger : struct
    where TEntity : class
    where TKey : notnull
{
    /// <summary>
    /// Asserts that the given trigger is allowed in the current FSM state.
    /// </summary>
    public FsmShouldAssertions<TState, TTrigger, TKey, TEntity> Allow(TTrigger trigger)
    {
        var result = fsm.TryFireAsync(trigger).GetAwaiter().GetResult();
        MinimalAssert.IsTrue(result.IsSuccess, $"Expected FSM to allow trigger '{trigger}', but it failed.");
        return this;
    }

    /// <summary>
    /// Asserts that the given trigger is not allowed in the current FSM state.
    /// </summary>
    public FsmShouldAssertions<TState, TTrigger, TKey, TEntity> NotAllow(TTrigger trigger)
    {
        var result = fsm.TryFireAsync(trigger).GetAwaiter().GetResult();
        MinimalAssert.IsFalse(result.IsSuccess, $"Expected FSM to reject trigger '{trigger}', but it was allowed.");
        return this;
    }

    /// <summary>
    /// Asserts that the FSM is currently in the specified state.
    /// </summary>
    public FsmShouldAssertions<TState, TTrigger, TKey, TEntity> BeIn(TState expected)
    {
        MinimalAssert.AreEqual(expected, fsm.CurrentState, $"Expected FSM to be in state '{expected}'");
        return this;
    }

    /// <summary>
    /// Asserts that the FSM entity has been deleted (i.e., it is null).
    /// </summary>
    public FsmShouldAssertions<TState, TTrigger, TKey, TEntity> DeleteEntity()
    {
        MinimalAssert.IsTrue(fsm.CurrentEntity is null, "Expected entity to be deleted.");
        return this;
    }
    
    /// <summary>
    /// Executes a custom assertion against the current FSM entity.
    /// </summary>
    public FsmShouldAssertions<TState, TTrigger, TKey, TEntity> HaveEntity(Action<TEntity?> assert)
    {
        assert(fsm.CurrentEntity);
        return this;
    }
    
    /// <summary>
    /// Asserts that the current entity associated with the FSM is not null.
    /// Use this to verify that the FSM retained the entity after transitions.
    /// </summary>
    public FsmShouldAssertions<TState, TTrigger, TKey, TEntity> NotNullEntity()
    {
        MinimalAssert.IsNotNull(fsm.CurrentEntity, "Expected entity to not be null.");
        return this;
    }

    /// <summary>
    /// Returns an assertion helper for validating FSM transition history.
    /// </summary>
    public FsmHistoryAssertions<TState, TTrigger, TKey, TEntity> History()
    {
        return new FsmHistoryAssertions<TState, TTrigger, TKey, TEntity>(fsm);
    }

    /// <summary>
    /// Returns an assertion helper for validating FSM logs.
    /// </summary>
    public FsmLogsAssertions<TState, TTrigger, TKey, TEntity> Logs()
    {
        return new FsmLogsAssertions<TState, TTrigger, TKey, TEntity>(fsm);
    }
}