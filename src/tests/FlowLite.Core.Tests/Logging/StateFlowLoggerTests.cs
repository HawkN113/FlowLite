using FlowLite.Core.Abstractions.Logging;
using FlowLite.Core.Logging;
namespace FlowLite.Core.Tests.Logging;

public class StateFlowLoggerTests
{
    [Fact]
    public void Write_ShouldAddLogEntry()
    {
        // Arrange
        var logger = new StateFlowLogger();

        // Act
        logger.Write(LogLevel.Info, "Test message");

        // Assert
        var logs = logger.GetLogs().ToList();
        Assert.Single(logs);
        Assert.Equal("Test message", logs[0].Message);
        Assert.Equal(LogLevel.Info, logs[0].Level);
    }

    [Fact]
    public void Write_ShouldNotExceedMaxLogSize()
    {
        // Arrange
        var logger = new StateFlowLogger();

        // Act
        for (var i = 0; i < 1010; i++)
        {
            logger.Write(LogLevel.Info, $"Message {i}");
        }

        // Assert
        var logs = logger.GetLogs().ToList();
        Assert.Equal(1000, logs.Count);
        Assert.Contains(logs, log => log.Message == "Message 10");
        Assert.DoesNotContain(logs, log => log.Message == "Message 0");
    }

    [Fact]
    public void GetLogs_ShouldFilterByLevel()
    {
        // Arrange
        var logger = new StateFlowLogger();
        logger.Write(LogLevel.Info, "Info log");
        logger.Write(LogLevel.Warning, "Warning log");
        logger.Write(LogLevel.Error, "Error log");

        // Act
        var warnings = logger.GetLogs(LogLevel.Warning).ToList();

        // Assert
        Assert.Single(warnings);
        Assert.Equal("Warning log", warnings[0].Message);
    }

    [Fact]
    public void GetLogs_ShouldReturnEmpty_WhenNoMatchingLevel()
    {
        // Arrange
        var logger = new StateFlowLogger();
        logger.Write(LogLevel.Info, "Info log");

        // Act
        var errorLogs = logger.GetLogs(LogLevel.Error).ToList();

        // Assert
        Assert.Empty(errorLogs);
    }

    [Fact]
    public async Task Write_ShouldBeThreadSafe()
    {
        // Arrange
        var logger = new StateFlowLogger();
        var totalLogs = 5000;

        // Act
        await Task.WhenAll(Enumerable.Range(0, totalLogs).Select(_ =>
            Task.Run(() => logger.Write(LogLevel.Info, "Thread-safe test"))));

        // Assert
        var logs = logger.GetLogs().ToList();
        Assert.Equal(1000, logs.Count);
    }
}