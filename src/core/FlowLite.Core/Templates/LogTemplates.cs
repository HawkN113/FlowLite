namespace FlowLite.Core.Templates;

internal static class LogTemplates
{
    public const string InvalidTransitionTemplate = "Invalid transition: {0} -- ({1}) -> ?";
    public const string InvalidTriggerTemplate = "TryFireAsync failed for {0}: {1}";
    public const string InitializedStateTemplate = "Initialized with state: {0}";
    public const string ChangedStateTemplate = "State changed to: {0}";
    public const string FinalStateReachedTemplate = "FSM reached final state: {0}";
}