using FlowLite.Diag.Attributes;

namespace FlowLite.Diag.Models;

[CommandHelp(
    "Generates a diagnostic graph for FlowLite-based finite state machines (FSM).",
    "flowlite-diag --source \"<path-to-source-code>\" --format \"dot|mermaid\"\nflowlite-diag -s \"<path-to-source-code>\" -f \"dot|mermaid\""
)]
public sealed class ArgsOptions
{
    [CommandOption("source","s","Specifies the path to the project directory.", "<empty>")]
    public string SourcePath { get; init; } = string.Empty;
    
    [CommandOption("format","f","Specifies the export format of the output. Supported values: dot, mermaid.", "dot")]
    public string FormatType { get; init; } = "dot";
}