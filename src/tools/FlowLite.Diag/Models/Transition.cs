namespace FlowLite.Diag.Models;

public class Transition
{
    public string From { get; set; }
    public string Trigger { get; set; }
    public string To { get; set; }
    public bool IsFinal { get; set; }
    public string BuilderName { get; set; }
}