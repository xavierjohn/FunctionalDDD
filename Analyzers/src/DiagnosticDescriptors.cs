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
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Using TryCreate().Value is unclear and provides poor error messages when validation fails. " +
                     "Use Create() when you expect success - it throws InvalidOperationException with the validation error details included. " +
                     "TryCreate().Value throws the same exception type but with a generic message, losing the validation error information. " +
                     "Or properly handle the Result returned by TryCreate() to avoid exceptions entirely.",
        helpLinkUri: HelpLinkBase + "FDDD007");

    /// <summary>
    /// FDDD008: Result is double-wrapped as Result&lt;Result&lt;T&gt;&gt;.
    /// </summary>
    public static readonly DiagnosticDescriptor ResultDoubleWrapping = new(
        id: "FDDD008",
        title: "Result is double-wrapped",
        messageFormat: "Result<Result<{0}>> detected. Use Bind instead of Map, or avoid wrapping an existing Result.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Result should not be wrapped inside another Result. This creates Result<Result<T>> which is almost always unintended. " +
                     "If combining Results, use Bind instead of Map. If wrapping a value, ensure it's not already a Result.",
        helpLinkUri: HelpLinkBase + "FDDD008");

    /// <summary>
    /// FDDD009: Blocking on async Result or accessing properties incorrectly.
    /// </summary>
    public static readonly DiagnosticDescriptor AsyncResultMisuse = new(
        id: "FDDD009",
        title: "Incorrect async Result usage",
        messageFormat: "Use 'await' with Task<Result<{0}>> instead of blocking or accessing Task properties",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Task<Result<T>> should be awaited, not blocked with .Result or .Wait(). " +
                     "Blocking can cause deadlocks and prevents proper async execution. Use await instead.",
        helpLinkUri: HelpLinkBase + "FDDD009");

    /// <summary>
    /// FDDD010: Using Error base class directly instead of specific error types.
    /// </summary>
    public static readonly DiagnosticDescriptor UseSpecificErrorType = new(
        id: "FDDD010",
        title: "Use specific error type instead of base Error class",
        messageFormat: "Use Error.Validation(), Error.NotFound(), or other specific error types instead of instantiating Error directly",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Using specific error types (ValidationError, NotFoundError, etc.) enables type-safe error handling with MatchError. " +
                     "Avoid instantiating the base Error class directly.",
        helpLinkUri: HelpLinkBase + "FDDD010");

    /// <summary>
    /// FDDD011: Maybe is double-wrapped as Maybe&lt;Maybe&lt;T&gt;&gt;.
    /// </summary>
    public static readonly DiagnosticDescriptor MaybeDoubleWrapping = new(
        id: "FDDD011",
        title: "Maybe is double-wrapped",
        messageFormat: "Maybe<Maybe<{0}>> detected. Avoid wrapping an existing Maybe inside another Maybe.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Maybe should not be wrapped inside another Maybe. This creates Maybe<Maybe<T>> which is almost always unintended. " +
                     "Avoid using Map when the transformation function returns a Maybe, as this creates double wrapping. " +
                     "Consider converting to Result with ToResult() for better composability.",
        helpLinkUri: HelpLinkBase + "FDDD011");

    /// <summary>
    /// FDDD012: Consider using Result.Combine for multiple Result checks.
    /// </summary>
    public static readonly DiagnosticDescriptor UseResultCombine = new(
        id: "FDDD012",
        title: "Consider using Result.Combine",
        messageFormat: "Consider using Result.Combine() for combining multiple Results instead of manual checks",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "When combining multiple Result<T> values, Result.Combine() provides a cleaner and more maintainable approach " +
                     "than manually checking IsSuccess on each result.",
        helpLinkUri: HelpLinkBase + "FDDD012");

    /// <summary>
    /// FDDD013: Consider using GetValueOrDefault or Match instead of ternary.
    /// </summary>
    public static readonly DiagnosticDescriptor UseFunctionalValueOrDefault = new(
        id: "FDDD013",
        title: "Consider using GetValueOrDefault or Match",
        messageFormat: "Consider using GetValueOrDefault() or Match() instead of ternary operator for Result value extraction",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "The pattern 'result.IsSuccess ? result.Value : default' can be replaced with GetValueOrDefault() or Match() " +
                     "for more idiomatic and safer code.",
        helpLinkUri: HelpLinkBase + "FDDD013");

    /// <summary>
    /// FDDD014: Using async lambda with synchronous Map/Bind instead of async variant.
    /// </summary>
    public static readonly DiagnosticDescriptor UseAsyncMethodVariant = new(
        id: "FDDD014",
        title: "Use async method variant for async lambda",
        messageFormat: "Use '{0}' instead of '{1}' when the lambda is async",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "When using an async lambda with Map, Bind, Tap, or Ensure, use the async variant (MapAsync, BindAsync, etc.) " +
                     "to properly handle the async operation. Using sync methods with async lambdas causes the Task to not be awaited.",
        helpLinkUri: HelpLinkBase + "FDDD014");

    /// <summary>
    /// FDDD015: Throwing exception inside Result chain instead of returning failure.
    /// </summary>
    public static readonly DiagnosticDescriptor ThrowInResultChain = new(
        id: "FDDD015",
        title: "Don't throw exceptions in Result chains",
        messageFormat: "Don't throw exceptions inside '{0}'. Return a failure Result instead to maintain Railway Oriented Programming semantics.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Throwing exceptions inside Bind, Map, Tap, or Ensure lambdas defeats the purpose of Railway Oriented Programming. " +
                     "Return Result.Failure<T>() to signal errors and keep the error on the failure track.",
        helpLinkUri: HelpLinkBase + "FDDD015");

    /// <summary>
    /// FDDD016: Empty or missing error message.
    /// </summary>
    public static readonly DiagnosticDescriptor EmptyErrorMessage = new(
        id: "FDDD016",
        title: "Error message should not be empty",
        messageFormat: "Error message should not be empty. Provide a meaningful message for debugging.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Error messages should provide context for debugging and user feedback. " +
                     "Empty error messages make it difficult to diagnose issues.",
        helpLinkUri: HelpLinkBase + "FDDD016");

    /// <summary>
    /// FDDD017: Comparing Result or Maybe to null.
    /// </summary>
    public static readonly DiagnosticDescriptor ComparingToNull = new(
        id: "FDDD017",
        title: "Don't compare Result or Maybe to null",
        messageFormat: "Don't compare {0} to null. Use '{1}' instead.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Result<T> and Maybe<T> are structs and cannot be null. " +
                     "Use IsSuccess/IsFailure for Result, or HasValue/HasNoValue for Maybe.",
        helpLinkUri: HelpLinkBase + "FDDD017");

    /// <summary>
    /// FDDD018: Using .Value in LINQ without checking success state.
    /// </summary>
    public static readonly DiagnosticDescriptor UnsafeValueInLinq = new(
        id: "FDDD018",
        title: "Unsafe access to Value in LINQ expression",
        messageFormat: "Accessing '{0}' in LINQ without filtering by {1} first may throw exceptions",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "When using LINQ on collections of Result<T> or Maybe<T>, filter by IsSuccess/HasValue first, " +
                     "or use methods like Select with Match to safely extract values.",
        helpLinkUri: HelpLinkBase + "FDDD018");

    /// <summary>
    /// FDDD019: Combine chain exceeds maximum supported tuple size.
    /// </summary>
    public static readonly DiagnosticDescriptor CombineChainTooLong = new(
        id: "FDDD019",
        title: "Combine chain exceeds maximum supported tuple size",
        messageFormat: "Combine chain produces a {0}-element tuple, but the maximum supported is 9. Group related validations into sub-objects.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Combine supports up to 9 elements. Downstream methods (Bind, Map, Tap, Match) also only support tuples up to 9 elements. " +
                     "Group related fields into intermediate value objects or sub-results, then combine those groups.",
        helpLinkUri: HelpLinkBase + "FDDD019");
}