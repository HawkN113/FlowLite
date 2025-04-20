namespace FlowLite.Diag.Processors.Abstraction;

public interface ICommandProcessor
{
    /// <summary>
    /// Parse command arguments in the application
    /// </summary>
    /// <param name="args"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<T> ParseArgsAsync<T>(string[] args) where T : class, new();

    /// <summary>
    /// Show current version of the tool
    /// </summary>
    /// <returns></returns>
    Task ShowVersionAsync();
    
    /// <summary>
    /// Show help 
    /// </summary>
    /// <returns></returns>
    Task ShowHelpOptionsAsync<T>() where T : class, new();

    /// <summary>
    /// Check options validation
    /// </summary>
    bool IsValidated { get; }
}