using System.Text.RegularExpressions;

namespace FlowLite.Diag.Helpers;

public static partial class RegExpressions
{
    [GeneratedRegex(@"--(?<keyId>\w+)(?:[\s](?<value>.+))?", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex ArgsLongRegex();
    
    [GeneratedRegex(@"-(?<keyId>\w+)(?:[\s](?<value>.+))?", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex ArgsShortRegex();

    [GeneratedRegex(@"^(?:[a-zA-Z]:\\|\/|\\\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex ProjectPathRegex();
}