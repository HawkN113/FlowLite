using System.Reflection;
using System.Text;
using FlowLite.Diag.Attributes;
using FlowLite.Diag.Processors.Abstraction;
namespace FlowLite.Diag.Processors;

internal sealed class CommandProcessor : ICommandProcessor
{
    private const string Version = "8.0.0.0";
    private bool _isValid;
    private bool _useLongOptionSyntax = true;

    public async Task<T> ParseArgsAsync<T>(string[] args) where T : class, new()
    {
        var result = new Dictionary<string, string>();
        if (args.Length == 0)
            return await ShowHelpAsync<T>();

        var joinedArgs = string.Join(" ", args);
        _useLongOptionSyntax = joinedArgs.Contains("--");
        
        var regex = _useLongOptionSyntax
            ? Helpers.RegExpressions.ArgsLongRegex()
            : Helpers.RegExpressions.ArgsShortRegex();

        var delimiter = _useLongOptionSyntax ? "--" : "-";

        var groupedArgs = joinedArgs
            .Split([delimiter], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => delimiter + x.Trim())
            .ToArray();

        foreach (var arg in groupedArgs)
        {
            var matches = regex.Matches(arg);
            foreach (var match in matches.Select(s => s.Groups))
            {
                var key = match["keyId"].Value;
                var value = match["value"].Value;
                result[key] = value;
            }
        }

        return await Task.FromResult(PopulateOptions<T>(result));
    }

    public async Task ShowVersionAsync()
    {
        var versionBuilder = new StringBuilder();
        versionBuilder.AppendLine(Version);
        await Console.Out.WriteLineAsync(versionBuilder.ToString());
    }

    public async Task ShowHelpOptionsAsync<T>() where T : class, new()
    {
        await Task.FromResult(ShowHelpAsync<T>());
    }

    public bool IsValidated => _isValid;

    private T PopulateOptions<T>(Dictionary<string, string> data) where T : new()
    {
        var options = new T();
        var type = typeof(T);
        foreach (var prop in type.GetProperties())
        {
            var aliasAttr = prop.GetCustomAttribute<CommandOptionAttribute>();
            var propertyName = _useLongOptionSyntax ? aliasAttr!.AliasName : aliasAttr!.ShortAliasName;

            if (!data.TryGetValue(propertyName, out var value)) continue;

            var convertedValue = Convert.ChangeType(value, prop.PropertyType);
            prop.SetValue(options, convertedValue);
        }

        _isValid = true;
        return options;
    }

    private static async Task<T> ShowHelpAsync<T>() where T : new()
    {
        var helpBuilder = new StringBuilder();
        var options = new T();
        var type = typeof(T);

        var commandHelp = type.GetCustomAttribute<CommandHelpAttribute>();
        if (commandHelp != null)
        {
            var usages = commandHelp.CommandUsage.Split('\n');
            helpBuilder.AppendLine($"{commandHelp.CommandDescription}");
            helpBuilder.AppendLine($"Usage: {string.Join("\n       ",usages)}");
            helpBuilder.AppendLine("Options:");
        }

        foreach (var prop in type.GetProperties())
        {
            var optionAttribute = prop.GetCustomAttribute<CommandOptionAttribute>();
            if (optionAttribute != null)
                helpBuilder.AppendLine(
                    $"       --{optionAttribute.AliasName},-{optionAttribute.ShortAliasName}: {optionAttribute.Description} (Default: {optionAttribute.DefaultValue})");
        }

        await Console.Out.WriteLineAsync(helpBuilder.ToString());
        return options;
    }
}