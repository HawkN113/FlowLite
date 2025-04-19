namespace FlowLite.Core.Fsm;

internal readonly struct StateTriggerKey<TState, TTrigger>(TState state, TTrigger trigger)
    where TState : struct
    where TTrigger : struct
{
    public override int GetHashCode() =>
        HashCode.Combine(state, trigger);

    public override string ToString() =>
        $"{state.ToString()!.ToLowerInvariant()}-{trigger.ToString()!.ToLowerInvariant()}";
    
    public TState State => state;
    public TTrigger Trigger => trigger;
}