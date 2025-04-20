namespace FlowLite.Diag.Models;

public class FlowTransitionBuilderEntry
{
    public string ClassName { get; set; } = string.Empty;
    public List<FlowTransitionBuilderTransition> Transitions { get; } = [];
}