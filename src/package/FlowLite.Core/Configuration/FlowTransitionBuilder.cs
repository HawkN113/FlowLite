using FlowLite.Core.Abstractions.Configuration;
using FlowLite.Core.Abstractions.Fsm;
using FlowLite.Core.Templates;
using FlowLite.Core.Validators;
namespace FlowLite.Core.Configuration;

public sealed class FlowTransitionBuilder<TState, TTrigger, TEntity>: IFlowTransitionBuilder<TState, TTrigger, TEntity>
    where TState : struct
    where TTrigger : struct
    where TEntity : class
{
    private readonly List<TransitionConfig<TState, TTrigger, TEntity>> _transitions = [];
    
    public IReadOnlyList<TransitionConfig<TState, TTrigger, TEntity>> Transitions => _transitions.AsReadOnly();

    public IFlowTransitionStep<TState, TTrigger, TEntity> AddTransition(
        TState fromState,
        TTrigger trigger,
        TState toState,
        Func<Func<TState, TTrigger?, ValueTask>, ITransitionContext<TEntity?>, ValueTask> onTransition)
    {
        if (TransitionConfigValidator.IdentifyCycleStates(fromState, toState, _transitions))
            throw new InvalidOperationException(
                string.Format(ErrorTemplates.CycleDetectedTemplate, 
                    fromState,
                toState));
        
        var exception = _transitions.Where(transition => _transitions.Count(s =>
                EqualityComparer<TState>.Default.Equals(s.FromState, transition.FromState) &&
                EqualityComparer<TTrigger>.Default.Equals(s.Trigger, transition.Trigger) &&
                EqualityComparer<TState>.Default.Equals(s.ToState, transition.ToState)) > 1)
            .Select(transition =>
                new InvalidOperationException(
                    string.Format(
                        ErrorTemplates.DuplicateTransitionTemplate, 
                        transition.FromState, 
                        transition.Trigger, 
                        transition.ToState)))
            .FirstOrDefault();
        if (exception != null)
            throw exception;

        var config = new TransitionConfig<TState, TTrigger, TEntity>
        {
            FromState = fromState,
            Trigger = trigger,
            ToState = toState,
            OnTransition = onTransition
        };
        _transitions.Add(config);
        return new FlowTransitionStep<TState, TTrigger, TEntity>(this, config);
    }
}
