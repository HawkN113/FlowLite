namespace FlowLite.Diag.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CommandHelpAttribute(string commandDescription, string commandUsage) : Attribute
{
    public string CommandDescription { get; } = commandDescription;
    public string CommandUsage { get; } = commandUsage;
}