using System.Text;
using FlowLite.Core.Abstractions.Exporter;
using FlowLite.Core.Fsm;
namespace FlowLite.Core.Export;

internal sealed class StateFlowExporter<TState, TTrigger>(
    Dictionary<StateTriggerKey<TState, TTrigger>, StateTransition<TState, TTrigger, object>> transitions)
    : IStateFlowExporter
    where TState : struct
    where TTrigger : struct
{
    public string ExportAsMermaid()
    {
        var sb = new StringBuilder();
        sb.AppendLine("stateDiagram-v2");

        foreach (var (key, transition) in transitions)
        {
            var from = key.State.ToString();
            var to = transition.ToState.ToString() ?? "null";
            var trigger = key.Trigger.ToString();

            if (transition.IsFinal)
                sb.AppendLine($"    {from} --> [{to}] : {trigger}");
            else
                sb.AppendLine($"    {from} --> {to} : {trigger}");
        }

        return sb.ToString();
    }

    public string ExportAsDot()
    {
        var sb = new StringBuilder();
        sb.AppendLine("digraph StateMachine {");
        sb.AppendLine("    rankdir=LR;");

        foreach (var (key, transition) in transitions)
        {
            var from = key.State.ToString();
            var to = transition.ToState.ToString() ?? "null";
            var trigger = key.Trigger.ToString();

            if (transition.IsFinal)
                sb.AppendLine($"    \"{from}\" -> \"{to}\" [ label = \"{trigger}\" , style = dashed ];");
            else
                sb.AppendLine($"    \"{from}\" -> \"{to}\" [ label = \"{trigger}\" ];");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }
}