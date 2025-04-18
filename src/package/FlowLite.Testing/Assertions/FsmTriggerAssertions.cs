using FlowLite.Core.Abstractions.Fsm;
namespace FlowLite.Testing.Assertions;

/// <summary>
/// Provides fluent-style trigger assertions for a finite state machine (FSM).
/// Useful for verifying whether specific triggers are allowed or disallowed from the current state.
/// </summary>
/// <typeparam name="TState">The type representing states.</typeparam>
/// <typeparam name="TTrigger">The type representing triggers.</typeparam>
/// <typeparam name="TKey">The type representing the key or identifier of the FSM entity.</typeparam>
/// <typeparam name="TEntity">The type representing the entity handled by the FSM.</typeparam>
public sealed class FsmTriggerAssertions<TState, TTrigger, TKey, TEntity>(
    IStateFlowMachine<TState, TTrigger, TKey, TEntity> fsm)
    where TState : struct
    where TTrigger : struct
    where TEntity : class
    where TKey : notnull
{
    /// <summary>
    /// Asserts that the specified trigger is allowed in the current FSM state.
    /// </summary>
    /// <param name="trigger">The trigger to test.</param>
    public void Allow(TTrigger trigger)
    {
        var result = fsm.TryFireAsync(trigger).GetAwaiter().GetResult();
        MinimalAssert.IsTrue(result.IsSuccess, $"Expected trigger '{trigger}' to be allowed.");
    }

    /// <summary>
    /// Asserts that the specified trigger is not allowed in the current FSM state.
    /// </summary>
    /// <param name="trigger">The trigger to test.</param>
    public void NotAllow(TTrigger trigger)
    {
        var result = fsm.TryFireAsync(trigger).GetAwaiter().GetResult();
        MinimalAssert.IsFalse(result.IsSuccess, $"Expected trigger '{trigger}' to be rejected.");
    }
}