using FlowLite.Core.Abstractions.Fsm;
using FlowLite.Core.Abstractions.Logging;
namespace FlowLite.Testing.Assertions;

/// <summary>
/// Provides fluent assertions for verifying FSM logs.
/// Useful for testing final state, specific log levels, and snapshots.
/// </summary>
/// <typeparam name="TState">The type representing FSM states.</typeparam>
/// <typeparam name="TTrigger">The type representing FSM triggers.</typeparam>
/// <typeparam name="TKey">The type representing the entity key.</typeparam>
/// <typeparam name="TEntity">The type of the FSM entity.</typeparam>
public sealed class FsmLogsAssertions<TState, TTrigger, TKey, TEntity>(
    IStateFlowMachine<TState, TTrigger, TKey, TEntity> fsm)
    where TState : struct
    where TTrigger : struct
    where TEntity : class
    where TKey : notnull
{
    /// <summary>
    /// Asserts that the FSM reached a final state by checking log messages.
    /// </summary>
    public FsmLogsAssertions<TState, TTrigger, TKey, TEntity> BeFinalState()
    {
        var logs = fsm.GetLogs();
        MinimalAssert.Contains(logs, l => l.Message.Contains("FSM reached final state"),
            "Expected FSM to reach final state.");
        return this;
    }
    
    /// <summary>
    /// Asserts that logs contain a message with a specific substring for the given log level.
    /// </summary>
    /// <param name="level">The log level to search.</param>
    /// <param name="contains">The expected substring to be found in the log message.</param>
    public FsmLogsAssertions<TState, TTrigger, TKey, TEntity> Log(LogLevel level, string contains)
    {
        var logs = fsm.GetLogs(level);
        MinimalAssert.Contains(logs, log => log.Message.Contains(contains), $"Expected log with '{contains}'");
        return this;
    }

    /// <summary>
    /// Asserts that logs contain the standard "Final state reached" message.
    /// </summary>
    public FsmLogsAssertions<TState, TTrigger, TKey, TEntity> ContainFinalStateLog()
    {
        var logs = fsm.GetLogs();
        MinimalAssert.Contains(logs, l => l.Message.Contains("Final state reached"), "Expected final state log.");
        return this;
    }

    /// <summary>
    /// Asserts that the log messages match the given expected snapshot exactly.
    /// </summary>
    /// <param name="expectedLogMessages">Expected array of log message strings.</param>
    public FsmLogsAssertions<TState, TTrigger, TKey, TEntity> MatchSnapshot(string[] expectedLogMessages)
    {
        var actual = fsm.GetLogs().Select(l => l.Message).ToArray();
        MinimalAssert.SequenceEqual(expectedLogMessages, actual, "FSM logs did not match snapshot.");
        return this;
    }
}