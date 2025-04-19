namespace FlowLite.Core.Templates;

internal static class ErrorTemplates
{
    public const string ExceptionTransitionTemplate = "Exception during transition: {0}";
    public const string InvalidTransitionConfigurationTemplate = "Either use '.ConfigureTransitions(...)' method or add transitions manually use '.AddTransition(...)' method.";
    public const string NoTransitionsDefinedTemplate = "No transitions defined.";
    public const string CycleDetectedTemplate = "Cycle detected: {0} -> {1}";
    public const string DuplicateTransitionTemplate = "Duplicate transition: {0} -- ({1}) -> {2}";
    public const string UnknownStorageTypeTemplate = "Unknown storage type";
    public const string StorageNotFoundTemplate = "Storage not found";
    public const string UnknownExportTypeTemplate = "Export type '{0}' is not supported.";
}