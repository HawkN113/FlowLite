using FlowLite.Diag.Models;
namespace FlowLite.Diag.Analysis.Abstraction;

public interface IFlowLiteInspector
{
    Task ScanAsync(string folderPath);
    IReadOnlyList<(string FilePath, IEnumerable<FlowTransitionBuilderEntry> Entries)> Results { get; }
}