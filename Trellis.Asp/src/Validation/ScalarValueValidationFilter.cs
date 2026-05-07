namespace Trellis.Asp;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Trellis;
using Trellis.Asp.Validation;

/// <summary>
/// An action filter that checks for validation errors collected during JSON deserialization
/// and validates <see cref="IScalarValue{TSelf, TPrimitive}"/> route/query parameters.
/// Returns a 400 Bad Request response with validation problem details when validation fails.
/// </summary>
/// <remarks>
/// <para>
/// This filter works in conjunction with ValidatingJsonConverterFactory to provide
/// automatic validation of scalar values in request DTOs. The converter collects validation errors
/// during deserialization, and this filter checks for errors before the action executes.
/// </para>
/// <para>
/// Additionally, this filter validates route and query string parameters that are
/// <see cref="IScalarValue{TSelf, TPrimitive}"/> types. When model binding fails for these types
/// (resulting in a null parameter), the filter returns a validation error.
/// </para>
/// <para>
/// If validation errors are detected:
/// <list type="bullet">
/// <item>The action is short-circuited (not executed)</item>
/// <item>A 400 Bad Request response with validation problem details is returned</item>
/// <item>The response format matches ASP.NET Core's standard validation error format</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// The filter is typically registered globally in Program.cs:
/// <code>
/// builder.Services.AddControllers(options =>
/// {
///     options.Filters.Add&lt;ScalarValueValidationFilter&gt;();
/// });
/// </code>
/// </example>
public sealed class ScalarValueValidationFilter : IActionFilter, IOrderedFilter
{
    /// <summary>
    /// Gets the order value for filter execution. This filter runs early to catch validation errors
    /// before other filters or the action execute.
    /// </summary>
    public int Order => -2000; // Run early, before most other filters

    /// <inheritdoc />
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // First, check for validation errors from JSON deserialization that landed in
        // the per-request collection scope (Trellis scalar VO converters that fail
        // gracefully — e.g., MaybeScalarValueJsonConverter / ValidatingJsonConverter).
        var validationError = ValidationErrorsContext.GetUnprocessableContent();
        if (validationError is not null)
        {
            HandleJsonValidationErrors(context, validationError);
            return;
        }

        // Second, check for structured errors that propagated as exceptions through MVC's
        // input formatter and landed in ModelState. Composite VO converters throw
        // TrellisJsonValidationException with UnprocessableContent attached; the JSON input
        // formatter catches it (as JsonException), records the message + JsonException.Path
        // verbatim in ModelState, and the body parameter ends up null — which then prompts
        // the model binder to add a parameter-name "request": ["The request field is required."]
        // entry. Both shapes are wrong: the first collapses per-leaf violations into a joined
        // string, and the second is binding-pipeline noise that duplicates the same logical
        // condition under a key the client cannot act on. Replace both with per-leaf entries
        // built from the structured payload.
        if (TryHandleStructuredModelStateErrors(context))
            return;

        // Third, check for null IScalarValue route/query parameters (binding failures)
        ValidateScalarValueParameters(context);
    }

    private static bool TryHandleStructuredModelStateErrors(ActionExecutingContext context)
    {
        // Find any ModelState entry whose error carries a TrellisJsonValidationException.
        // Both shapes are handled:
        //   - Structured: UnprocessableContent has at least one FieldViolation -> emit one
        //     wire entry per violation under <parent>.<leaf> keys.
        //   - Unstructured: no UnprocessableContent (e.g. missing required property,
        //     unsupported primitive type, JSON shape mismatch) -> emit a single entry at the
        //     translated JSON path with the exception's curated message. Without this branch
        //     the message would be lost: ModelStateDictionary.TryAddModelError stores an empty
        //     ErrorMessage when the recorded exception isn't an InputFormatterException, and
        //     ValidationProblemDetails would render a generic placeholder.
        TrellisJsonValidationException? trellisEx = null;
        string? entryParentPath = null;
        var trellisEntryKeys = new System.Collections.Generic.List<string>();

        foreach (var (key, entry) in context.ModelState)
        {
            foreach (var error in entry.Errors)
            {
                if (error.Exception is TrellisJsonValidationException tjx)
                {
                    // First match wins — additional Trellis exceptions in the same request
                    // are treated as duplicates (the converter throws on first failure).
                    trellisEx ??= tjx;
                    entryParentPath ??= ScalarValueValidationMiddleware.JsonPathToMvcKey(tjx.Path);
                    trellisEntryKeys.Add(key);
                    break;
                }
            }
        }

        if (trellisEx is null)
            return false;

        var freshModelState = new ModelStateDictionary();
        if (trellisEx.UnprocessableContent is { Fields.Length: > 0 } structured)
        {
            foreach (var fv in structured.Fields)
            {
                var leafKey = JsonPointerToMvc.Translate(fv.Field.Path);
                var combined = ScalarValueValidationMiddleware.CombineMvcKeys(entryParentPath ?? string.Empty, leafKey);
                var detail = !string.IsNullOrEmpty(fv.Detail) ? fv.Detail : fv.ReasonCode;
                freshModelState.AddModelError(combined, detail);
            }
        }
        else
        {
            freshModelState.AddModelError(entryParentPath ?? string.Empty, trellisEx.Message);
        }

        // Carry forward any other ModelState errors that were neither the Trellis-exception
        // entry nor the phantom body-parameter entry. The phantom entry is identified strictly
        // by key match against an action [FromBody] parameter name — we do NOT filter on the
        // "X field is required." text globally, because that would silently drop legitimate
        // required errors from query/route/form parameters and other DataAnnotations failures.
        var bodyParameterNames = GetBodyParameterNames(context);
        foreach (var (key, entry) in context.ModelState)
        {
            if (trellisEntryKeys.Contains(key))
                continue;

            if (bodyParameterNames.Contains(key))
                continue;

            foreach (var error in entry.Errors)
            {
                if (string.IsNullOrEmpty(error.ErrorMessage))
                    continue;

                freshModelState.AddModelError(key, error.ErrorMessage);
            }
        }

        var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problemDetails = factory.CreateValidationProblemDetails(context.HttpContext, freshModelState, statusCode: 400);
        context.Result = new BadRequestObjectResult(problemDetails);
        return true;
    }

    private static System.Collections.Generic.HashSet<string> GetBodyParameterNames(ActionExecutingContext context)
    {
        var names = new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal);
        foreach (var parameter in context.ActionDescriptor.Parameters)
        {
            if (parameter.BindingInfo?.BindingSource is { Id: "Body" } && parameter.Name is { Length: > 0 })
                names.Add(parameter.Name);
        }

        return names;
    }

    private static void HandleJsonValidationErrors(ActionExecutingContext context, Error.UnprocessableContent validationError)
    {
        // Create a fresh ModelStateDictionary to avoid key casing issues.
        // MVC's model validation adds errors with PascalCase C# property names (e.g., "State").
        // ModelStateDictionary's internal trie preserves the original key casing even after
        // Remove + re-Add with different casing. Using a fresh dictionary ensures our
        // camelCase field names (matching JSON property names) are preserved correctly.
        var modelState = new ModelStateDictionary();
        foreach (var fieldViolation in validationError.Fields)
        {
            modelState.AddModelError(JsonPointerToMvc.Translate(fieldViolation.Field.Path), fieldViolation.Detail ?? fieldViolation.ReasonCode);
        }

        var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problemDetails = factory.CreateValidationProblemDetails(context.HttpContext, modelState, statusCode: 400);
        context.Result = new BadRequestObjectResult(problemDetails);
    }

    private static BadRequestObjectResult CreateValidationProblemResult(ActionExecutingContext context)
    {
        var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problemDetails = factory.CreateValidationProblemDetails(context.HttpContext, context.ModelState, statusCode: 400);
        return new BadRequestObjectResult(problemDetails);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2072:Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.",
        Justification = "The type check for IScalarValue interfaces is safe - we only check interface implementation, not instantiate or invoke members.")]
    private static void ValidateScalarValueParameters(ActionExecutingContext context)
    {
        var actionParameters = context.ActionDescriptor.Parameters;

        foreach (var parameter in actionParameters)
        {
            var parameterType = parameter.ParameterType;

            // Handle nullable types (e.g., OrderState?)
            var underlyingType = Nullable.GetUnderlyingType(parameterType) ?? parameterType;

            // Skip if not an IScalarValue type
            if (!ScalarValueTypeHelper.IsScalarValue(underlyingType))
                continue;

            // Check if the parameter value is null (indicates binding failure for non-nullable IScalarValue)
            if (context.ActionArguments.TryGetValue(parameter.Name!, out var value) && value is null)
            {
                // Check whether a raw value was actually provided in the request.
                // If no raw value exists in route data or query string, the parameter
                // was simply not provided (e.g., optional query param like OrderState? state).
                // Only validate when a raw value is present but binding produced null (invalid input).
                var rawValue = GetRawParameterValue(context, parameter.Name!);
                if (rawValue is null)
                    continue;

                if (ShouldTreatEmptyQueryValueAsMissing(context, parameter, rawValue))
                    continue;

                // Call TryCreate to get the real validation error (e.g., with valid values list)
                var errors = ScalarValueTypeHelper.GetValidationErrors(underlyingType, rawValue, parameter.Name!);

                if (errors is not null)
                {
                    foreach (var (fieldName, details) in errors)
                        foreach (var detail in details)
                            context.ModelState.AddModelError(fieldName, detail);
                }
                else
                {
                    // Fallback if TryCreate method wasn't found
                    var typeName = underlyingType.Name;
                    var errorMessage = string.IsNullOrEmpty(rawValue)
                        ? $"'{parameter.Name}' is required."
                        : $"'{rawValue}' is not a valid {typeName}.";

                    context.ModelState.AddModelError(parameter.Name!, errorMessage);
                }
            }
        }

        // Return validation problem if any errors were added
        if (!context.ModelState.IsValid)
            context.Result = CreateValidationProblemResult(context);
    }

    private static string? GetRawParameterValue(ActionExecutingContext context, string parameterName)
    {
        // Try to get the raw value from route data
        if (context.RouteData.Values.TryGetValue(parameterName, out var routeValue))
            return routeValue?.ToString();

        // Try to get from query string
        if (context.HttpContext.Request.Query.TryGetValue(parameterName, out var queryValue))
            return queryValue.ToString();

        return null;
    }

    private static bool ShouldTreatEmptyQueryValueAsMissing(
        ActionExecutingContext context,
        ParameterDescriptor parameter,
        string rawValue)
    {
        if (!string.IsNullOrEmpty(rawValue))
            return false;

        if (context.RouteData.Values.ContainsKey(parameter.Name!))
            return false;

        return IsNullableReferenceParameter(parameter);
    }

    private static bool IsNullableReferenceParameter(ParameterDescriptor parameter)
    {
        if (parameter is not ControllerParameterDescriptor controllerParameter)
            return false;

        if (controllerParameter.ParameterType.IsValueType)
            return Nullable.GetUnderlyingType(controllerParameter.ParameterType) is not null;

        // NullabilityInfoContext is documented as not thread-safe, so we instantiate per call
        // rather than caching a shared static instance that could be accessed concurrently
        // by parallel requests. See: https://learn.microsoft.com/dotnet/api/system.reflection.nullabilityinfocontext
        var nullabilityContext = new NullabilityInfoContext();
        var nullability = nullabilityContext.Create(controllerParameter.ParameterInfo);
        return nullability.ReadState == NullabilityState.Nullable;
    }

    /// <inheritdoc />
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed after execution
    }
}