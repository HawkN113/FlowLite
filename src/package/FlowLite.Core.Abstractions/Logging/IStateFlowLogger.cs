namespace FlowLite.Core.Abstractions.Logging;

internal interface IStateFlowLogger
{
    void Write(LogLevel level, string message);
    void Write(LogLevel level, string messageTemplate, params object[] args);
    IEnumerable<(DateTime Timestamp, LogLevel Level, string Message)> GetLogs(LogLevel? level = null);
}