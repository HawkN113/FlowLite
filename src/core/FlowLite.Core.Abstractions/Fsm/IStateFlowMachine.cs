using FlowLite.Core.Abstractions.Configuration;
using FlowLite.Core.Abstractions.Exporter;
using FlowLite.Core.Abstractions.Logging;
using FlowLite.Core.Abstractions.Models;
namespace FlowLite.Core.Abstractions.Fsm;

/// <summary>
/// Defines a Finite State Machine (FSM) for managing entity states.
/// </summary>
/// <typeparam name="TState">The type representing states.</typeparam>
/// <typeparam name="TTrigger">The type representing triggers (events that cause state transitions).</typeparam>
/// <typeparam name="TKey">The type of the entity key (e.g., Guid or int).</typeparam>
/// <typeparam name="TEntity">The type of the entity associated with the FSM.</typeparam>
public interface IStateFlowMachine<TState, TTrigger, TKey, TEntity> : IDisposable
    where TState : struct
    where TTrigger : struct
    where TEntity : class
    where TKey : notnull
{
    /// <summary>
    /// Gets the current state of the finite state machine.
    /// </summary>
    TState? CurrentState { get; }

    /// <summary>
    /// Gets the current entity associated with the finite state machine.
    /// </summary>
    TEntity? CurrentEntity { get; }

    /// <summary>
    /// Retrieves the history of state transitions.
    /// </summary>
    /// <returns>A list of tuples containing the trigger and target state.</returns>
    IReadOnlyList<(TTrigger? Trigger, TState State)> GetTransitionHistory();

    /// <summary>
    /// Retrieves a list of logs related to state processing.
    /// </summary>
    /// <param name="level">Filter by log level (if null, retrieves all logs).</param>
    /// <returns>A list of log entries with timestamps and log levels.</returns>
    IReadOnlyList<(DateTime Timestamp, LogLevel Level, string Message)> GetLogs(LogLevel? level = null);

    /// <summary>
    /// Event triggered when the state changes.
    /// </summary>
    event Action<TState, TTrigger>? OnStateChanged;
    
    /// <summary>
    /// Triggered when an unhandled exception occurs during transition execution in FireAsync.
    /// </summary>
    event Action<TState, TTrigger, Exception>? OnTransitionFailed;

    /// <summary>
    /// Event triggered when the entity changes.
    /// </summary>
    event Action<TEntity>? OnEntityChanged;
    
    /// <summary>
    /// Event triggered when the entity deletes.
    /// </summary>
    event Action<TKey>? OnEntityDeleted;

    /// <summary>
    /// Adds a state transition to the FSM.
    /// </summary>
    /// <param name="fromState">The initial state.</param>
    /// <param name="trigger">The trigger that initiates the transition.</param>
    /// <param name="toState">The target state after the transition.</param>
    /// <param name="onTransition">A function executed during the transition.</param>
    /// <returns>The FSM instance for method chaining.</returns>
    IFluentTransitionBuilder<TState, TTrigger, TKey, TEntity> AddTransition(
        TState fromState,
        TTrigger trigger,
        TState toState,
        Func<Func<TState, TTrigger?, ValueTask>, ITransitionContext<TEntity?>, ValueTask> onTransition);

    /// <summary>
    /// Adds a state transition configuration.
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    IStateFlowMachine<TState, TTrigger, TKey, TEntity> ConfigureTransitions(
        IFlowTransitionBuilder<TState, TTrigger, TEntity> config);

    /// <summary>
    /// Attempts to fire a trigger to transition to the next state.
    /// </summary>
    /// <param name="trigger">The trigger to fire.</param>
    /// <returns>A boolean indicating whether the transition was successful.</returns>
    ValueTask<bool> FireAsync(TTrigger trigger);

    /// <summary>
    /// Tries to fire a trigger and returns a result indicating success or failure.
    /// </summary>
    /// <param name="trigger">The trigger to fire.</param>
    /// <returns>A Result object containing success or failure information.</returns>
    ValueTask<Result<bool>> TryFireAsync(TTrigger trigger);

    /// <summary>
    /// Exports the data as a graph description or diagram.
    /// </summary>
    /// <param name="type">Available types:
    /// - Mermaid
    /// - Dot
    /// </param>
    /// <returns>
    /// A string representing the graph in DOT format.
    /// Or a string representing the diagram in Mermaid syntax.
    /// </returns>
    string Export(ExportType type);
}