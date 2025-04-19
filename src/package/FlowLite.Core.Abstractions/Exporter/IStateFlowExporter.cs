namespace FlowLite.Core.Abstractions.Exporter;

/// <summary>
/// Provides export functions for the transition map.
/// </summary>
internal interface IStateFlowExporter
{
    /// <summary>
    /// Exports the data as a Mermaid diagram in text format.
    /// Mermaid is a JavaScript-based diagramming and charting tool.
    /// </summary>
    /// <returns>
    /// A string representing the diagram in Mermaid syntax.
    /// </returns>
    string ExportAsMermaid();
    /// <summary>
    /// Exports the data as a DOT graph description.
    /// DOT is a graph description language used by Graphviz for visualizing graphs.
    /// </summary>
    /// <returns>
    /// A string representing the graph in DOT format.
    /// </returns>
    string ExportAsDot();
}