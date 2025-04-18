using FlowLite.Diag.Analysis.Abstraction;
using FlowLite.Diag.Export.Abstraction;
using FlowLite.Diag.Models;
using FlowLite.Diag.Processors.Abstraction;
namespace FlowLite.Diag.Processors;

internal sealed class FlowLiteProcessor(
    IDotExporter dotExporter, 
    IMermaidExporter mermaidExporter,
    IFlowLiteInspector inspector): IFlowLiteProcessor
{
    private readonly List<(string FilePath, IEnumerable<FlowTransitionBuilderEntry> Transitions)> _results = [];

    public async Task AnalyzeAsync(string folderPath)
    {
        await inspector.ScanAsync(folderPath);
        _results.Clear();
        _results.AddRange(inspector.Results);
    }
    
    public void Print(string format)
    {
        var output = format.ToLowerInvariant() switch
        {
            "dot" => dotExporter.Export(_results),
            "mermaid" => mermaidExporter.Export(_results),
            _ => throw new InvalidOperationException("Invalid format. Use 'dot' or 'mermaid'.")
        };
        Console.Out.WriteLine(output);
        Console.Out.WriteLine($"Exported diagram for {_results.Count} file(s). Format: {format}");
        _results.Clear();
    }
}