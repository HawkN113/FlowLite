using FlowLite.Diag.Export;
using FlowLite.Diag.Models;

namespace FlowLite.Diag.Tests.Export;

public class MermaidExporterTests
{
    [Fact]
    public void Export_ShouldSupportMultipleBuildersPerFile()
    {
        // Arrange
        var transitions = new List<FlowTransitionBuilderEntry>
        {
            new()
            {
                ClassName = "Builder1"
            },
            new()
            {
                ClassName = "Builder2"
            }
        };
        transitions[0].Transitions.Add(new FlowTransitionBuilderTransition()
            { FromState = "Start", ToState = "Next", Trigger = "Go" });
        transitions[1].Transitions.Add(new FlowTransitionBuilderTransition()
            { FromState = "Alpha", ToState = "Omega", Trigger = "End", IsFinal = true });
            

        var data = new List<(string FilePath, IEnumerable<FlowTransitionBuilderEntry>)>
        {
            ("Workflow.cs", transitions)
        };

        var exporter = new MermaidExporter();

        // Act
        var output = exporter.Export(data);

        // Assert
        Assert.Contains("Name: Builder1", output);
        Assert.Contains("Start --> Next: Go", output);

        Assert.Contains("Name: Builder2", output);
        Assert.Contains("Alpha --> [Omega] : End", output);
    }

    [Fact]
    public void Export_ShouldReturnNoDiagrams_WhenEmptyTransitions()
    {
        // Arrange
        var data = new List<(string FilePath, IEnumerable<FlowTransitionBuilderEntry>)>
        {
            ("Empty.cs", Enumerable.Empty<FlowTransitionBuilderEntry>())
        };

        var exporter = new MermaidExporter();

        // Act
        var output = exporter.Export(data);

        // Assert
        Assert.Contains("File: Empty.cs", output);
        Assert.Contains("No found diagrams", output);
    }
}