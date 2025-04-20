using FlowLite.Diag.Models;

namespace FlowLite.Diag.Export.Abstraction;

public interface IExporter
{
    string Export(
        IEnumerable<(string FilePath, IEnumerable<FlowTransitionBuilderEntry> Transitions)> groupedTransitions);
}