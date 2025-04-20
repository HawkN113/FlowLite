namespace FlowLite.Diag.Models;

public class FlowTransitionBuilderTransition
{
    public string FromState { get; set; } = string.Empty;
    public string Trigger { get; set; } = string.Empty;
    public string ToState { get; set; } = string.Empty;
    public bool IsFinal { get; set; }
}