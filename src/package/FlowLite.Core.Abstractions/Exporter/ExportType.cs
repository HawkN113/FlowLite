namespace FlowLite.Core.Abstractions.Exporter;

/// <summary>
/// Defines the supported export formats for the state machine visualization.
/// </summary>
public enum ExportType
{
    /// <summary>
    /// Export the state machine as a Mermaid.js diagram (stateDiagram-v2).
    /// Suitable for Markdown files and documentation.
    /// </summary>
    Mermaid = 0,

    /// <summary>
    /// Export the state machine as a DOT graph.
    /// Suitable for tools like Graphviz.
    /// </summary>
    Dot = 1
}