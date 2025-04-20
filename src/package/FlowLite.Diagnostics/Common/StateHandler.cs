using System.Text.Json;
using FlowLite.Core.Abstractions.Fsm;
using Microsoft.Extensions.Logging;
namespace FlowLite.Diagnostics.Common;

internal static class StateHandler
{
    public static void RegisterEvents<TState, TTrigger, TKey, TEntity>(IStateFlowMachine<TState, TTrigger, TKey, TEntity> fsm)
        where TState : struct 
        where TTrigger : struct 
        where TEntity : class 
        where TKey : notnull
    {
        fsm.OnStateChanged += (state, trigger) =>
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[State Changed] -> {state} (trigger: {trigger})");
            Console.ResetColor();
        };
        fsm.OnEntityChanged += entity =>
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"[Entity Changed] -> {JsonSerializer.Serialize(entity)}");
            Console.ResetColor();
        };
        fsm.OnEntityDeleted += id =>
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[Entity Deleted] -> Key: {id}");
            Console.ResetColor();
        };
        fsm.OnTransitionFailed += (state, trigger, ex) =>
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Transition Failed] -> {state} (trigger: {trigger})");
            Console.WriteLine($"    Reason: {ex.GetType().Name} - {ex.Message}");
            Console.ResetColor();
        };
    }

    public static void RegisterLoggingEvents<TState, TTrigger, TKey, TEntity>(
        IStateFlowMachine<TState, TTrigger, TKey, TEntity> fsm, ILogger logger)
        where TState : struct
        where TTrigger : struct
        where TEntity : class
        where TKey : notnull
    {
        fsm.OnStateChanged += (state, trigger) =>
            logger.LogInformation("State changed → {State} (trigger: {Trigger})", state, trigger);
        fsm.OnEntityChanged += entity =>
            logger.LogInformation("Entity updated: {Entity}", JsonSerializer.Serialize(entity));
        fsm.OnEntityDeleted += key =>
            logger.LogWarning("🗑Entity deleted (Key: {Key})", key);
        fsm.OnTransitionFailed += (state, trigger, ex) =>
            logger.LogError(ex,
                "Transition failed in state '{State}' using trigger '{Trigger}'. Exception: {Message}",
                state, trigger, ex.Message);
    }
}