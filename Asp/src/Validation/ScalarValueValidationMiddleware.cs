namespace FunctionalDdd;

using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Middleware that creates a validation error collection scope for each request.
/// This enables ValidatingJsonConverter to collect validation errors
/// across the entire request deserialization process.
/// </summary>
/// <remarks>
/// <para>
/// This middleware should be registered early in the pipeline, before any middleware
/// that might deserialize JSON request bodies.
/// </para>
/// <para>
/// For each request:
/// <list type="bullet">
/// <item>Creates a new validation error collection scope</item>
/// <item>Allows the request to proceed through the pipeline</item>
/// <item>Catches <see cref="BadHttpRequestException"/> for parameter binding failures and returns validation problem</item>
/// <item>Cleans up the scope when the request completes</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Registering the middleware in Program.cs:
/// <code>
/// app.UseScalarValueValidation();
/// // ... other middleware
/// app.MapControllers();
/// </code>
/// </example>
public sealed partial class ScalarValueValidationMiddleware
{
    private readonly RequestDelegate _next;

    // Regex to parse: Failed to bind parameter "TypeName paramName" from "value".
    [GeneratedRegex("""^Failed to bind parameter "(\w+)\s+(\w+)" from "([^"]*)".$""", RegexOptions.Compiled)]
    private static partial Regex ParameterBindingFailedRegex();

    /// <summary>
    /// Creates a new instance of <see cref="ScalarValueValidationMiddleware"/>.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public ScalarValueValidationMiddleware(RequestDelegate next) =>
        _next = next;

    /// <summary>
    /// Invokes the middleware, wrapping the request in a validation scope.
    /// </summary>
    /// <param name="context">The HTTP context for the request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        using (ValidationErrorsContext.BeginScope())
        {
            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (BadHttpRequestException ex) when (ex.Message.StartsWith("Failed to bind parameter", StringComparison.Ordinal))
            {
                // Handle parameter binding failures (e.g., invalid route parameters for value objects)
                // Convert to validation problem response with structured errors
                await WriteValidationProblemAsync(context, ex.Message).ConfigureAwait(false);
            }
        }
    }

    private static async Task WriteValidationProblemAsync(HttpContext context, string exceptionMessage)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        // Parse the exception message to extract parameter info
        // Format: Failed to bind parameter "TypeName paramName" from "value".
        var (parameterName, typeName, invalidValue) = ParseBindingFailureMessage(exceptionMessage);

        var errorMessage = string.IsNullOrEmpty(invalidValue)
            ? $"'{parameterName}' is required."
            : $"'{invalidValue}' is not a valid {typeName}.";

        var errors = new Dictionary<string, string[]>
        {
            [parameterName] = [errorMessage]
        };

        // Try to use Results.ValidationProblem for consistent response format
        var result = Results.ValidationProblem(errors);
        await result.ExecuteAsync(context).ConfigureAwait(false);
    }

    private static (string ParameterName, string TypeName, string InvalidValue) ParseBindingFailureMessage(string message)
    {
        // Try to parse: Failed to bind parameter "TypeName paramName" from "value".
        var match = ParameterBindingFailedRegex().Match(message);
        if (match.Success)
        {
            var typeName = match.Groups[1].Value;
            var paramName = match.Groups[2].Value;
            var invalidValue = match.Groups[3].Value;
            return (paramName, typeName, invalidValue);
        }

        // Fallback if regex doesn't match
        return ("parameter", "value", string.Empty);
    }
}