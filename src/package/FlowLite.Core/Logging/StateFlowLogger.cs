using System.Collections.Concurrent;
using FlowLite.Core.Abstractions.Logging;
namespace FlowLite.Core.Logging;

internal sealed class StateFlowLogger : IStateFlowLogger
{
    private const int MaxLogSize = 1000;
    private readonly ConcurrentQueue<(DateTime Timestamp, LogLevel Level, string Message)> _logs = new();
    private int _logCount;

    public void Write(LogLevel level, string message)
    {
        var logEntry = (DateTime.UtcNow, level, message);
        _logs.Enqueue(logEntry);

        if (Interlocked.Increment(ref _logCount) <= MaxLogSize) return;
        _logs.TryDequeue(out _);
        Interlocked.Decrement(ref _logCount);
    }

    public void Write(LogLevel level, string messageTemplate, params object[] args)
    {
        var message = string.Format(messageTemplate, args);
        var logEntry = (DateTime.UtcNow, level, message);
        _logs.Enqueue(logEntry);

        if (Interlocked.Increment(ref _logCount) <= MaxLogSize) return;
        _logs.TryDequeue(out _);
        Interlocked.Decrement(ref _logCount);
    }

    public IEnumerable<(DateTime Timestamp, LogLevel Level, string Message)> GetLogs(LogLevel? level = null)
    {
        return level is null ? _logs : _logs.Where(log => log.Level == level);
    }
}