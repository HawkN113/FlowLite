namespace FlowLite.Diag.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class CommandOptionAttribute(string aliasName, string shortAliasName, string description, string defaultValue) : Attribute
{
    public string AliasName { get; } = aliasName;
    public string ShortAliasName { get; } = shortAliasName;
    public string Description { get; } = description;
    public string? DefaultValue { get; } = defaultValue;
}