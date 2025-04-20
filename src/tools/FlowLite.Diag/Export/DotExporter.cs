using System.Text;
using FlowLite.Diag.Export.Abstraction;
using FlowLite.Diag.Models;
namespace FlowLite.Diag.Export;

internal sealed class DotExporter : IDotExporter
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
            if (!transitions.Any())
            {
                sb.AppendLine("No found diagrams\n");
                return sb.ToString();
            }

            foreach (var transition in transitions)
            {
                sb.AppendLine($"digraph \"{SanitizeGraphName(transition.ClassName)}\" {{");
                sb.AppendLine("    rankdir=LR;");
                foreach (var t in transition.Transitions)
                {
                    var from = Sanitize(t.FromState);
                    var to = Sanitize(t.ToState);
                    var trigger = Sanitize(t.Trigger);
                    if (t.IsFinal)
                        sb.AppendLine($"    \"{from}\" -> \"{to}\" [ label = \"{trigger}\" , style = dashed ];");
                    else
                        sb.AppendLine($"    \"{from}\" -> \"{to}\" [ label = \"{trigger}\" ];");
                }

                sb.AppendLine("}");
            }
        }

        return sb.ToString();
    }

    private static string Sanitize(string input)
    {
        return input.Replace("\"", "").Replace(".", "_");
    }

    private static string SanitizeGraphName(string input)
    {
        return input.Replace("-", "_").Replace(".", "_").Replace(" ", "_");
    }
}