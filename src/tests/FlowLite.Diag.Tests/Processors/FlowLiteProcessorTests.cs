using FlowLite.Diag.Analysis.Abstraction;
using FlowLite.Diag.Export.Abstraction;
using FlowLite.Diag.Models;
using FlowLite.Diag.Processors;
using Moq;

namespace FlowLite.Diag.Tests.Processors;

public class FlowLiteProcessorTests
{
    private readonly Mock<IDotExporter> _dotExporterMock = new();
    private readonly Mock<IMermaidExporter> _mermaidExporterMock = new();
    private readonly Mock<IFlowLiteInspector> _inspectorMock = new();

    private readonly FlowLiteProcessor _processor;

    public FlowLiteProcessorTests()
    {
        _processor = new FlowLiteProcessor(
            _dotExporterMock.Object,
            _mermaidExporterMock.Object,
            _inspectorMock.Object
        );
    }

    [Fact]
    public async Task AnalyzeAsync_Should_InvokeInspector_AndUpdateResults()
    {
        // Arrange
        var transitions = new List<FlowTransitionBuilderEntry>
        {
            new()
            {
                ClassName = "A",
                Transitions =
                {
                    new FlowTransitionBuilderTransition()
                        { FromState = "From", ToState = "To", Trigger = "Trigger", IsFinal = false }
                }
            }
        };

        _inspectorMock.Setup(i => i.ScanAsync(It.IsAny<string>()))
                      .Returns(Task.CompletedTask);

        _inspectorMock.SetupGet(i => i.Results)
                      .Returns(new List<(string, IEnumerable<FlowTransitionBuilderEntry>)>
                      {
                          ("Program.cs", transitions)
                      });

        // Act
        await _processor.AnalyzeAsync("some/folder");

        // Assert
        _inspectorMock.Verify(i => i.ScanAsync("some/folder"), Times.Once);
    }

    [Fact]
    public async Task Print_Should_Use_MermaidExporter()
    {
        // Arrange
        var transitions = new List<FlowTransitionBuilderEntry>
        {
            new()
            {
                ClassName = "A",
                Transitions =
                {
                    new FlowTransitionBuilderTransition()
                        { FromState = "From", ToState = "To", Trigger = "Trigger", IsFinal = false }
                }
            }
        };
        _inspectorMock.SetupGet(i => i.Results)
            .Returns(new List<(string, IEnumerable<FlowTransitionBuilderEntry>)> { ("file.cs", transitions) });

        _mermaidExporterMock.Setup(e => e.Export(It.IsAny<IEnumerable<(string, IEnumerable<FlowTransitionBuilderEntry>)>>()))
            .Returns("mermaid output");

        await _processor.AnalyzeAsync("folder");

        using var output = new StringWriter();
        Console.SetOut(output);

        // Act
        _processor.Print("mermaid");

        // Assert
        var text = output.ToString();
        Assert.Contains("mermaid output", text);
        Assert.Contains("Exported diagram for 1 file(s)", text);
    }

    [Fact]
    public async Task Print_Should_Use_DotExporter()
    {
        // Arrange
        var transitions = new List<FlowTransitionBuilderEntry>
        {
            new()
            {
                ClassName = "A",
                Transitions =
                {
                    new FlowTransitionBuilderTransition()
                        { FromState = "From", ToState = "To", Trigger = "Trigger", IsFinal = false }
                }
            }
        };
        _inspectorMock.SetupGet(i => i.Results)
            .Returns(new List<(string, IEnumerable<FlowTransitionBuilderEntry>)> { ("file2.cs", transitions) });

        _dotExporterMock.Setup(e => e.Export(It.IsAny<IEnumerable<(string, IEnumerable<FlowTransitionBuilderEntry>)>>()))
            .Returns("dot output");

        await _processor.AnalyzeAsync("folder");

        await using var output = new StringWriter();
        Console.SetOut(output);

        // Act
        _processor.Print("dot");

        // Assert
        var text = output.ToString();
        Assert.Contains("dot output", text);
        Assert.Contains("Exported diagram for 1 file(s)", text);
    }

    [Fact]
    public void Print_Should_Throw_If_Format_Invalid()
    {
        // Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            _processor.Print("invalid"));

        Assert.Equal("Invalid format. Use 'dot' or 'mermaid'.", ex.Message);
    }
}