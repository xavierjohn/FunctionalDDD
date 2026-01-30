namespace FunctionalDdd.Analyzers;

using Microsoft.CodeAnalysis;

/// <summary>
/// Diagnostic descriptors for FunctionalDDD analyzers.
/// </summary>
public static class DiagnosticDescriptors
{
    private const string Category = "FunctionalDDD";
    private const string HelpLinkBase = "https://xavierjohn.github.io/FunctionalDDD/analyzers/";

    /// <summary>
    /// FDDD001: Result return value is not handled.
    /// </summary>
    public static readonly DiagnosticDescriptor ResultNotHandled = new(
        id: "FDDD001",
        title: "Result return value is not handled",
        messageFormat: "The Result returned by '{0}' is not handled. Error information may be lost.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Result<T> return values should be handled to ensure errors are not silently ignored. " +
                     "Use Bind, Map, Match, or assign to a variable.",
        helpLinkUri: HelpLinkBase + "FDDD001");

    /// <summary>
    /// FDDD002: Use Bind instead of Map when the lambda returns a Result.
    /// </summary>
    public static readonly DiagnosticDescriptor UseBindInsteadOfMap = new(
        id: "FDDD002",
        title: "Use Bind instead of Map when lambda returns Result",
        messageFormat: "The lambda returns a Result<T>. Use Bind instead of Map to avoid Result<Result<T>>.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "When the transformation function returns a Result<T>, use Bind (flatMap) instead of Map. " +
                     "Map will produce Result<Result<T>> which is likely not intended.",
        helpLinkUri: HelpLinkBase + "FDDD002");

    /// <summary>
    /// FDDD003: Accessing Result.Value without checking IsSuccess.
    /// </summary>
    public static readonly DiagnosticDescriptor UnsafeResultValueAccess = new(
        id: "FDDD003",
        title: "Unsafe access to Result.Value",
        messageFormat: "Accessing Result.Value without checking IsSuccess may throw an exception",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Result.Value throws an InvalidOperationException if the Result is in a failure state. " +
                     "Check IsSuccess first, use TryGetValue, or use pattern matching with Match/MatchError.",
        helpLinkUri: HelpLinkBase + "FDDD003");

    /// <summary>
    /// FDDD004: Accessing Result.Error without checking IsFailure.
    /// </summary>
    public static readonly DiagnosticDescriptor UnsafeResultErrorAccess = new(
        id: "FDDD004",
        title: "Unsafe access to Result.Error",
        messageFormat: "Accessing Result.Error without checking IsFailure may throw an exception",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Result.Error throws an InvalidOperationException if the Result is in a success state. " +
                     "Check IsFailure first, use TryGetError, or use pattern matching with Match/MatchError.",
        helpLinkUri: HelpLinkBase + "FDDD004");

    /// <summary>
    /// FDDD005: Consider using MatchError for error type discrimination.
    /// </summary>
    public static readonly DiagnosticDescriptor UseMatchErrorForDiscrimination = new(
        id: "FDDD005",
        title: "Consider using MatchError for error type discrimination",
        messageFormat: "Consider using MatchError instead of switch/if on error types for exhaustive handling",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "MatchError provides type-safe pattern matching on specific error types " +
                     "(ValidationError, NotFoundError, etc.) with a fallback for unhandled types.",
        helpLinkUri: HelpLinkBase + "FDDD005");

    /// <summary>
    /// FDDD006: Accessing Maybe.Value without checking HasValue.
    /// </summary>
    public static readonly DiagnosticDescriptor UnsafeMaybeValueAccess = new(
        id: "FDDD006",
        title: "Unsafe access to Maybe.Value",
        messageFormat: "Accessing Maybe.Value without checking HasValue may throw an exception",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Maybe.Value throws an InvalidOperationException if the Maybe has no value. " +
                     "Check HasValue first, use TryGetValue, GetValueOrDefault, or convert to Result with ToResult.",
        helpLinkUri: HelpLinkBase + "FDDD006");

    /// <summary>
    /// FDDD007: Using .Value on TryCreate result instead of Create method.
    /// </summary>
    public static readonly DiagnosticDescriptor UseCreateInsteadOfTryCreateValue = new(
        id: "FDDD007",
        title: "Use Create instead of TryCreate().Value",
        messageFormat: "Using TryCreate().Value is unclear. Use '{0}.Create(...)' when you expect the value to be valid, or handle the Result properly.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "For scalar value objects implementing IScalarValue, TryCreate().Value is unnecessary. " +
                     "Use Create() for clearer intent when you expect success, or properly handle the Result returned by TryCreate(). " +
                     "Both throw the same exception on invalid input, but Create() provides clearer intent.",
        helpLinkUri: HelpLinkBase + "FDDD007");
}
