namespace FlowLite.Diag.Processors.Abstraction;

public interface IFlowLiteProcessor
{
    /// <summary>
    /// Analyze the transitions in the bulders
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    Task AnalyzeAsync(string folderPath);
    
    /// <summary>
    /// Show the diagram with the format
    /// </summary>
    /// <param name="format"></param>
    void Print(string format);
}