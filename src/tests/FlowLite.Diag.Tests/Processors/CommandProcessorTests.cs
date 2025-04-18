using FlowLite.Diag.Models;
using FlowLite.Diag.Processors;

namespace FlowLite.Diag.Tests.Processors;

public class CommandProcessorTests
{
    private readonly CommandProcessor _processor = new();

    [Fact]
    public async Task ParseArgsAsync_Should_Parse_Long_Args_Correctly()
    {
        // Arrange
        string[] args = ["--source", "src", "--format", "mermaid"];
        
        // Act
        var result = await _processor.ParseArgsAsync<ArgsOptions>(args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("src", result.SourcePath);
        Assert.Equal("mermaid", result.FormatType);
        Assert.True(_processor.IsValidated);
    }

    [Fact]
    public async Task ParseArgsAsync_Should_Parse_Short_Args_Correctly()
    {
        // Arrange
        string[] args = ["-s", "src", "-f", "dot"];

        // Act
        var result = await _processor.ParseArgsAsync<ArgsOptions>(args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("src", result.SourcePath);
        Assert.Equal("dot", result.FormatType);
        Assert.True(_processor.IsValidated);
    }

    [Fact]
    public async Task ParseArgsAsync_Should_Fallback_To_Help_When_Empty()
    {
        await using var output = new StringWriter();
        Console.SetOut(output);
        
        var args = Array.Empty<string>();

        // Act
        var result = await _processor.ParseArgsAsync<ArgsOptions>(args);

        // Assert
        var outputText = output.ToString();
        Assert.Contains("Generates a diagnostic graph for FlowLite", outputText);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ShowHelpOptionsAsync_Should_Print_Options()
    {
        await using var output = new StringWriter();
        Console.SetOut(output);

        // Act
        await _processor.ShowHelpOptionsAsync<ArgsOptions>();

        // Assert
        var outputText = output.ToString();
        Assert.Contains("--source", outputText);
        Assert.Contains("--format", outputText);
    }

    [Fact]
    public async Task ShowVersionAsync_Should_Print_Version()
    {
        await using var output = new StringWriter();
        Console.SetOut(output);

        // Act
        await _processor.ShowVersionAsync();

        // Assert
        var outputText = output.ToString();
        Assert.Contains("8.0.0.0", outputText);
    }
}