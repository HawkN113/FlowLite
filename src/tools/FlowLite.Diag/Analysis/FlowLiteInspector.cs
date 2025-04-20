using FlowLite.Diag.Analysis.Abstraction;
using FlowLite.Diag.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace FlowLite.Diag.Analysis;

internal sealed class FlowLiteInspector : IFlowLiteInspector
{
    private const string AddTransitionCommandName = "AddTransition";
    private const string AsFinalCommandName = "AsFinal";
    private const string TransitionBuilderClassName = "FlowTransitionBuilder";
    private const string SearchPattern = "*.cs";

    private List<(string FilePath, IEnumerable<FlowTransitionBuilderEntry> Entries)> _results = [];

    public async Task ScanAsync(string folderPath)
    {
        var csFiles = Directory.GetFiles(folderPath, SearchPattern, SearchOption.AllDirectories);
        var results = new List<(string FilePath, IEnumerable<FlowTransitionBuilderEntry> Entries)>();

        foreach (var file in csFiles)
        {
            var code = await File.ReadAllTextAsync(file);
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = await tree.GetRootAsync();

            var builderMap = new Dictionary<SyntaxNode, FlowTransitionBuilderEntry>();
            var builderStringMap = new Dictionary<string, FlowTransitionBuilderEntry>();

            var variableAssignments = GetVariableAssignments(root);
            ProcessVariableAssignments(variableAssignments, builderMap, builderStringMap);

            var returnStatements = root.DescendantNodes()
                .OfType<ReturnStatementSyntax>()
                .Where(r => r.Expression is InvocationExpressionSyntax or ObjectCreationExpressionSyntax);

            foreach (var returnStmt in returnStatements)
            {
                var expr = returnStmt.Expression;
                if (FindObjectCreation(expr) is not { Type: GenericNameSyntax g }) continue;
                if (g.Identifier.Text != TransitionBuilderClassName) continue;

                var line = returnStmt.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                var entry = new FlowTransitionBuilderEntry
                    { ClassName = $"return_{TransitionBuilderClassName.ToLower()}_{line}" };

                var invocations = expr.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().Reverse();

                foreach (var invocation in invocations)
                {
                    if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) continue;

                    var method = memberAccess.Name.Identifier.Text;
                    if (method == AddTransitionCommandName && invocation.ArgumentList.Arguments.Count >= 3)
                    {
                        var from = GetStateName(invocation.ArgumentList.Arguments[0]);
                        var trigger = GetStateName(invocation.ArgumentList.Arguments[1]);
                        var to = GetStateName(invocation.ArgumentList.Arguments[2]);

                        entry.Transitions.Add(new FlowTransitionBuilderTransition
                        {
                            FromState = from,
                            Trigger = trigger,
                            ToState = to
                        });
                    }
                    else if (method == AsFinalCommandName && entry.Transitions.Count > 0)
                    {
                        entry.Transitions[^1].IsFinal = true;
                    }
                }

                if (entry.Transitions.Count > 0)
                    results.Add((file, [entry]));
            }

            var invocationsAll = root.DescendantNodes().OfType<InvocationExpressionSyntax>().Reverse();
            ProcessInvocations(invocationsAll, builderMap, builderStringMap);
            MergeBuilderMaps(builderMap, builderStringMap);

            if (builderMap.Any())
                results.Add((file, builderMap.Values));
        }

        _results = results;
    }

    public IReadOnlyList<(string FilePath, IEnumerable<FlowTransitionBuilderEntry> Entries)> Results => _results;

    private static List<VariableDeclaratorSyntax> GetVariableAssignments(SyntaxNode root)
    {
        return root.DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .Where(v => v.Initializer != null)
            .Reverse()
            .ToList();
    }

    private void ProcessVariableAssignments(List<VariableDeclaratorSyntax> variableAssignments,
        Dictionary<SyntaxNode, FlowTransitionBuilderEntry> builderMap,
        Dictionary<string, FlowTransitionBuilderEntry> builderStringMap)
    {
        foreach (var v in variableAssignments)
        {
            var initializer = v.Initializer!.Value;
            var creationExpr = FindObjectCreation(initializer);
            if (creationExpr?.Type is not GenericNameSyntax { Identifier.Text: TransitionBuilderClassName }) continue;
            builderMap[initializer] = new FlowTransitionBuilderEntry { ClassName = v.Identifier.Text };
            builderStringMap[v.Identifier.Text] = new FlowTransitionBuilderEntry { ClassName = v.Identifier.Text };
        }
    }

    private static void ProcessInvocations(IEnumerable<InvocationExpressionSyntax> invocations,
        Dictionary<SyntaxNode, FlowTransitionBuilderEntry> builderMap,
        Dictionary<string, FlowTransitionBuilderEntry> builderStringMap)
    {
        foreach (var invocation in invocations)
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) continue;
            var methodName = memberAccess.Name.Identifier.Text;
            if (methodName != AddTransitionCommandName && methodName != AsFinalCommandName) continue;
            var rootExpr = FindRootBuilderExpression(invocation);
            var entry = builderMap.FirstOrDefault(kvp => kvp.Key.Span.Contains(rootExpr.Span)).Value;
            if (entry == null)
            {
                var rootObjectName = GetRootObjectName(memberAccess.Expression);
                if (rootObjectName != null)
                    builderStringMap.TryGetValue(rootObjectName, out entry);
            }
            if (entry != null)
                ProcessMethodInvocation(methodName, invocation, entry);
        }
    }

    private static void ProcessMethodInvocation(string methodName, InvocationExpressionSyntax invocation,
        FlowTransitionBuilderEntry entry)
    {
        switch (methodName)
        {
            case AddTransitionCommandName when invocation.ArgumentList.Arguments.Count >= 3:
            {
                var from = GetStateName(invocation.ArgumentList.Arguments[0]);
                var trigger = GetStateName(invocation.ArgumentList.Arguments[1]);
                var to = GetStateName(invocation.ArgumentList.Arguments[2]);

                entry.Transitions.Add(new FlowTransitionBuilderTransition
                {
                    FromState = from,
                    Trigger = trigger,
                    ToState = to
                });
                break;
            }
            case AsFinalCommandName when entry.Transitions.Count > 0:
                entry.Transitions[^1].IsFinal = true;
                break;
        }
    }

    private static string GetStateName(SyntaxNode argument)
    {
        return argument.ToString()[(argument.ToString().LastIndexOf('.') + 1)..];
    }

    private static void MergeBuilderMaps(Dictionary<SyntaxNode, FlowTransitionBuilderEntry> builderMap,
        Dictionary<string, FlowTransitionBuilderEntry> builderMap1)
    {
        foreach (var (className, newEntry) in builderMap1)
        {
            var existing = builderMap.Values.FirstOrDefault(e => e.ClassName == className);
            if (existing != null)
            {
                existing.Transitions.AddRange(newEntry.Transitions);
            }
            else
            {
                var fakeKey = SyntaxFactory.IdentifierName(className);
                builderMap[fakeKey] = newEntry;
            }
        }
    }

    private static ObjectCreationExpressionSyntax? FindObjectCreation(SyntaxNode node)
    {
        return node switch
        {
            ObjectCreationExpressionSyntax objCreation => objCreation,
            InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess } => FindObjectCreation(
                memberAccess.Expression),
            MemberAccessExpressionSyntax ma => FindObjectCreation(ma.Expression),
            _ => null
        };
    }

    private static SyntaxNode FindRootBuilderExpression(SyntaxNode node)
    {
        while (node is MemberAccessExpressionSyntax ma)
        {
            node = ma.Expression;
        }

        return node;
    }

    private static string? GetRootObjectName(ExpressionSyntax? expression)
    {
        while (expression is not null)
        {
            switch (expression)
            {
                case IdentifierNameSyntax id:
                    return id.Identifier.Text;
                case MemberAccessExpressionSyntax memberAccess:
                    expression = memberAccess.Expression;
                    continue;
                case InvocationExpressionSyntax invocation:
                    expression = invocation.Expression;
                    continue;
                case ConditionalAccessExpressionSyntax conditional:
                    expression = conditional.Expression;
                    continue;
                default:
                    return null;
            }
        }

        return null;
    }
}