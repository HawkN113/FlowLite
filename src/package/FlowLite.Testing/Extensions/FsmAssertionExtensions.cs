using FlowLite.Core.Abstractions.Fsm;
using FlowLite.Testing.Assertions;
namespace FlowLite.Testing.Extensions;

/// <summary>
/// Provides extension methods for fluent assertions on <see cref="IStateFlowMachine{TState, TTrigger, TKey, TEntity}"/>.
/// Enables expressive and readable tests for validating FSM behavior.
/// </summary>
public static class FsmAssertionExtensions
{
    /// <summary>
    /// Begins a chain of fluent assertions that validate the internal state, history, logs,
    /// and finalization of the state machine.
    /// </summary>
    /// <typeparam name="TState">The enum type representing states.</typeparam>
    /// <typeparam name="TTrigger">The enum type representing triggers.</typeparam>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The type of the associated entity.</typeparam>
    /// <param name="fsm">The finite state machine to assert against.</param>
    /// <returns>An object for fluent FSM state assertions.</returns>
    public static FsmShouldAssertions<TState, TTrigger, TKey, TEntity> Should<TState, TTrigger, TKey, TEntity>(
        this IStateFlowMachine<TState, TTrigger, TKey, TEntity> fsm)
        where TState : struct
        where TTrigger : struct
        where TEntity : class
        where TKey : notnull
        => new(fsm);

    /// <summary>
    /// Begins a chain of fluent assertions for validating allowed or disallowed trigger transitions.
    /// Use this for checking whether specific triggers are permitted in the current FSM state.
    /// </summary>
    /// <typeparam name="TState">The enum type representing states.</typeparam>
    /// <typeparam name="TTrigger">The enum type representing triggers.</typeparam>
    /// <typeparam name="TKey">The type of the entity key.</typeparam>
    /// <typeparam name="TEntity">The type of the associated entity.</typeparam>
    /// <param name="fsm">The finite state machine to assert against.</param>
    /// <returns>An object for fluent FSM trigger assertions.</returns>
    public static FsmTriggerAssertions<TState, TTrigger, TKey, TEntity> Assert<TState, TTrigger, TKey, TEntity>(
        this IStateFlowMachine<TState, TTrigger, TKey, TEntity> fsm)
        where TState : struct
        where TTrigger : struct
        where TEntity : class
        where TKey : notnull
        => new(fsm);
}