using System.Text;
using FlowLite.Diag.Export.Abstraction;
using FlowLite.Diag.Models;
namespace FlowLite.Diag.Export;

internal sealed class MermaidExporter : IMermaidExporter
{
    public string Export(
        IEnumerable<(string FilePath, IEnumerable<FlowTransitionBuilderEntry> Transitions)> groupedTransitions)
    {
        var sb = new StringBuilder();
        foreach (var (filePath, transitions) in groupedTransitions)
        {
            sb.AppendLine();
            sb.AppendLine($"File: {filePath}");
            sb.AppendLine();
            if (transitions is null || !transitions.Any())
            {
                sb.AppendLine("No found diagrams\n");
                return sb.ToString();
            }
            foreach (var transition in transitions)
            {
                sb.AppendLine($"Name: {transition.ClassName}");
                sb.AppendLine("stateDiagram-v2");
                foreach (var t in transition.Transitions)
                    sb.AppendLine(t.IsFinal
                        ? string.Format("    {0} --> [{1}] : {2}", t.FromState, t.ToState, t.Trigger)
                        : string.Format("    {0} --> {1}: {2}", t.FromState, t.ToState, t.Trigger));
            }
        }
        return sb.ToString();
    }
}