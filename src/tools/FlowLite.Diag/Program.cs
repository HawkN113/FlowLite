using FlowLite.Diag.Analysis;
using FlowLite.Diag.Analysis.Abstraction;
using FlowLite.Diag.Export;
using FlowLite.Diag.Export.Abstraction;
using FlowLite.Diag.Models;
using FlowLite.Diag.Processors;
using FlowLite.Diag.Processors.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

const string errorPrefix = "Error: ";

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
        logging.AddConsole();
    })
    .ConfigureServices(services =>
    {
        services.AddScoped<IDotExporter, DotExporter>();
        services.AddScoped<IMermaidExporter, MermaidExporter>();
        services.AddScoped<IFlowLiteInspector, FlowLiteInspector>();
        services.AddSingleton<ICommandProcessor, CommandProcessor>();
        services.AddSingleton<IFlowLiteProcessor, FlowLiteProcessor>();
    })
    .Build();

try
{
    var commandProcessor = host.Services.GetRequiredService<ICommandProcessor>();
    var flowLiteProcessor = host.Services.GetRequiredService<IFlowLiteProcessor>();

    if (args.Contains("--version") || args.Contains("-v"))
    {
        await commandProcessor.ShowVersionAsync();
        return;
    }

    if (args.Contains("--help") || args.Contains("-h"))
    {
        await commandProcessor.ShowHelpOptionsAsync<ArgsOptions>();
        return;
    }

    var options = await commandProcessor.ParseArgsAsync<ArgsOptions>(args);
    if (commandProcessor.IsValidated)
    {
        await flowLiteProcessor.AnalyzeAsync(options.SourcePath);
        flowLiteProcessor.Print(options.FormatType);
    }
}
catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or DirectoryNotFoundException)
{
    await ExitWithErrorAsync(ex.Message, GetExitCode(ex));
}
catch (Exception ex)
{
    await ExitWithErrorAsync(ex.Message, -3);
}
return;

static async Task ExitWithErrorAsync(string message, int exitCode)
{
    await Console.Out.WriteLineAsync($"{errorPrefix}{message}");
    Environment.Exit(exitCode);
}

static int GetExitCode(Exception ex) => ex switch
{
    ArgumentException => -1,
    DirectoryNotFoundException => -2,
    InvalidOperationException => -3,
    _ => 0
};