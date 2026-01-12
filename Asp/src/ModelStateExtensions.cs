namespace FunctionalDdd.Asp;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Immutable;

/// <summary>
/// Extension methods for converting ASP.NET Core ModelState to Railway Oriented Programming Result types.
/// </summary>
public static class ModelStateExtensions
{
    /// <summary>
    /// Converts ModelState validation results to a Result, allowing Railway Oriented Programming
    /// with automatic model binding validation.
    /// </summary>
    /// <param name="modelState">The ModelStateDictionary to validate.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item>Success with Unit if ModelState is valid</item>
    /// <item>Failure with ValidationError containing all field errors if ModelState is invalid</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method enables Railway Oriented Programming patterns with automatic model binding.
    /// When using value objects in request DTOs with automatic binding, model binding validates
    /// the value objects and adds errors to ModelState. This method converts those errors to
    /// a Result so you can chain with Bind/Map operations.
    /// </para>
    /// <para>
    /// This prevents calling domain logic (like User.TryCreate) when model binding has already
    /// identified validation errors, avoiding potential null reference or invalid state issues.
    /// </para>
    /// </remarks>
    /// <example>
    /// Basic usage with automatic value object binding:
    /// <code>
    /// public record CreateUserRequest(
    ///     FirstName FirstName,  // Automatically validated
    ///     LastName LastName,    // Automatically validated
    ///     EmailAddress Email    // Automatically validated
    /// );
    /// 
    /// [HttpPost]
    /// public ActionResult&lt;User&gt; Register([FromBody] CreateUserRequest request) =>
    ///     ModelState.ToResult()  // ✅ Check validation first
    ///         .Bind(_ =&gt; User.TryCreate(request.FirstName, request.LastName, request.Email))
    ///         .ToActionResult(this);
    /// 
    /// // If FirstName is invalid, ModelState.ToResult() returns Failure
    /// // User.TryCreate is never called (Railway pattern - stays on failure track)
    /// </code>
    /// </example>
    /// <example>
    /// With additional domain validation:
    /// <code>
    /// [HttpPost]
    /// public ActionResult&lt;User&gt; Register([FromBody] CreateUserRequest request) =>
    ///     ModelState.ToResult()
    ///         .Bind(_ =&gt; User.TryCreate(request.FirstName, request.LastName, request.Email))
    ///         .Ensure(user =&gt; !_repository.ExistsByEmail(user.Email), 
    ///                 Error.Conflict("Email already exists"))
    ///         .Tap(user =&gt; _repository.Add(user))
    ///         .ToActionResult(this);
    /// </code>
    /// </example>
    public static Result<Unit> ToResult(this ModelStateDictionary modelState)
    {
        if (modelState.IsValid)
            return Result.Success();

        var fieldErrors = modelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .Select(kvp => new ValidationError.FieldError(
                kvp.Key,
                kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()))
            .ToImmutableArray();

        return Error.Validation(fieldErrors);
    }
}
