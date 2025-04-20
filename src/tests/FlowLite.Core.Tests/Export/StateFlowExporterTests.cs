using FlowLite.Core.Export;
using FlowLite.Core.Fsm;
namespace FlowLite.Core.Tests.Export;

public class StateFlowExporterTests
{
    private enum TestState { Start, Middle, End }
    private enum TestTrigger { Begin, Proceed, Finish }

    [Fact]
    public void ExportAsMermaid_ShouldReturnValidDiagram()
    {
        // Arrange
        var finalTransition = new StateTransition<TestState, TestTrigger, object>(TestState.End, async (_, _) => await Task.CompletedTask).AsFinal();
        var transitions = new Dictionary<StateTriggerKey<TestState, TestTrigger>, StateTransition<TestState, TestTrigger, object>>
        {
            [new StateTriggerKey<TestState, TestTrigger>(TestState.Start, TestTrigger.Begin)] = new(TestState.Middle, async (_, _) => await Task.CompletedTask),
            [new StateTriggerKey<TestState, TestTrigger>(TestState.Middle, TestTrigger.Proceed)] = finalTransition
        };

        var exporter = new StateFlowExporter<TestState, TestTrigger>(transitions);

        // Act
        var output = exporter.ExportAsMermaid();

        // Assert
        Assert.Contains("stateDiagram-v2", output);
        Assert.Contains("Start --> Middle : Begin", output);
        Assert.Contains("Middle --> [End] : Proceed", output);
    }

    [Fact]
    public void ExportAsDot_ShouldReturnValidGraph()
    {
        // Arrange
        var finalTransition = new StateTransition<TestState, TestTrigger, object>(TestState.End, async (_, _) => await Task.CompletedTask).AsFinal();
        var transitions = new Dictionary<StateTriggerKey<TestState, TestTrigger>, StateTransition<TestState, TestTrigger, object>>
        {
            [new StateTriggerKey<TestState, TestTrigger>(TestState.Start, TestTrigger.Begin)] = new(TestState.Middle, async (_, _) => await Task.CompletedTask),
            [new StateTriggerKey<TestState, TestTrigger>(TestState.Middle, TestTrigger.Proceed)] = finalTransition
        };

        var exporter = new StateFlowExporter<TestState, TestTrigger>(transitions);

        // Act
        var output = exporter.ExportAsDot();

        // Assert
        Assert.Contains("digraph StateMachine", output);
        Assert.Contains("\"Start\" -> \"Middle\" [ label = \"Begin\" ];", output);
        Assert.Contains("\"Middle\" -> \"End\" [ label = \"Proceed\" , style = dashed ];", output);
    }
}