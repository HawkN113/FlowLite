using System.Diagnostics.CodeAnalysis;
using System.Text;
using FlowLite.Diag.Grammar;
using FlowLite.Diag.Models;

namespace FlowLite.Diag;

public class FlowTransitionMermaidVisitor : FlowTransitionBaseVisitor<object>
{
    private readonly Dictionary<string, List<FlowTransitionBuilderEntry>> _transitions = new();
    private string _currentConfigName = string.Empty;

    public override object VisitFlowBuilderAssignment([NotNull] FlowTransitionParser.FlowBuilderAssignmentContext context)
    {
        _currentConfigName = context.IDENTIFIER().GetText();
        if (!_transitions.ContainsKey(_currentConfigName))
            _transitions[_currentConfigName] = [];
        return base.VisitFlowBuilderAssignment(context);
    }

    public override object VisitMethodChain([NotNull] FlowTransitionParser.MethodChainContext context)
    {
        if (context.GetText().StartsWith(".AddTransition"))
        {
            var argsCtx = context.transitionArgs();
            if (argsCtx != null)
            {
                var stateFrom = ExtractIdentifier(argsCtx.expression(0));
                var trigger = ExtractIdentifier(argsCtx.expression(1));
                var stateTo = ExtractIdentifier(argsCtx.expression(2));

                var isFinal = context.GetText().Contains(".AsFinal");

                _transitions[_currentConfigName]
                    .Add(new FlowTransitionBuilderEntry(stateFrom, stateTo, trigger, isFinal));
            }
        }

        return base.VisitMethodChain(context);
    }

    private string ExtractIdentifier(FlowTransitionParser.ExpressionContext ctx)
    {
        return ctx.GetText().Split('.').Last();
    }

    public Dictionary<string, string> GetMermaidDiagrams()
    {
        var result = new Dictionary<string, string>();
        foreach (var config in _transitions)
        {
            var sb = new StringBuilder();
            sb.AppendLine("stateDiagram-v2");
            foreach (var transition in config.Value)
            {
                var toState = transition.IsFinal ? $"[{transition.ToState}]" : transition.ToState;
                sb.AppendLine($"\t{transition.FromState} --> {toState} : {transition.Trigger}");
            }
            result[config.Key] = sb.ToString();
        }

        return result;
    }
}