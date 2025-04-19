using FlowLite.Core.Fsm;
namespace FlowLite.Core.Validators;

internal static class TransitionValidator
{
    public static bool IdentifyCycleStates<TState, TTrigger, TEntity>(
        TState startState,
        TState targetState,
        Dictionary<StateTriggerKey<TState, TTrigger>, StateTransition<TState, TTrigger, TEntity>> transitions)
        where TState : struct 
        where TTrigger : struct 
        where TEntity : class
    {
        return startState.Equals(targetState) || HasCycles(startState, targetState, transitions);
    }

    public static bool IdentifyCycleStates<TState, TTrigger, TEntity>(
        TState startState,
        Dictionary<StateTriggerKey<TState, TTrigger>, StateTransition<TState, TTrigger, TEntity>> transitions)
        where TState : struct 
        where TTrigger : struct 
        where TEntity : class
    {
        return HasCycles(startState, default, transitions);
    }

    private static bool HasCycles<TState, TTrigger, TEntity>(
        TState startState,
        TState targetState,
        Dictionary<StateTriggerKey<TState, TTrigger>, StateTransition<TState, TTrigger, TEntity>> transitions)
        where TState : struct
        where TTrigger : struct
        where TEntity : class
    {
        var visited = new HashSet<TState>();
        var stack = new Stack<TState>();
        stack.Push(startState);

        while (stack.TryPop(out var currentState))
        {
            if (!visited.Add(currentState))
                continue;

            var nextStates = transitions.Values
                .Where(t => EqualityComparer<TState>.Default.Equals(t.ToState, currentState))
                .Select(t => t.ToState);

            foreach (var nextState in nextStates)
            {
                if (EqualityComparer<TState>.Default.Equals(nextState, targetState))
                    return true;

                if (!visited.Contains(nextState))
                    stack.Push(nextState);
            }
        }
        return false;
    }
}