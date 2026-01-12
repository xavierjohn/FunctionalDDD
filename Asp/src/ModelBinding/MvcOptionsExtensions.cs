namespace FunctionalDdd.Asp.ModelBinding;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Extension methods for configuring MVC options with value object model binding.
/// </summary>
public static class MvcOptionsExtensions
{
    /// <summary>
    /// Adds value object model binding support to MVC.
    /// This enables automatic binding and validation of parameters and DTO properties
    /// that implement <see cref="ITryCreatable{T}"/>.
    /// </summary>
    /// <param name="options">The MVC options to configure.</param>
    /// <returns>The same options instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// After calling this method, you can use value objects directly in:
    /// <list type="bullet">
    /// <item>Controller action parameters</item>
    /// <item>Request DTO properties</item>
    /// <item>[FromQuery], [FromRoute], [FromForm] parameters</item>
    /// <item>Request body properties (via JSON deserialization + model binding)</item>
    /// </list>
    /// </para>
    /// <para>
    /// Validation errors are automatically converted to 400 Bad Request responses
    /// with Problem Details format including field-level error details.
    /// </para>
    /// <para>
    /// The model binder provider is inserted at index 0 to take precedence over default binders.
    /// This ensures value objects are bound correctly before falling back to default behavior.
    /// </para>
    /// <para>
    /// <strong>Note:</strong> This method only handles route, query, and form parameters.
    /// For JSON request bodies, use <see cref="AddValueObjectJsonInputFormatter"/> in addition.
    /// </para>
    /// </remarks>
    /// <example>
    /// Registration in Program.cs:
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// 
    /// builder.Services.AddControllers(options =>
    /// {
    ///     options.AddValueObjectModelBinding();
    /// });
    /// 
    /// var app = builder.Build();
    /// app.MapControllers();
    /// app.Run();
    /// </code>
    /// </example>
    /// <example>
    /// Usage in controller with automatic validation:
    /// <code>
    /// // Route parameter binding
    /// [HttpGet("{id}")]
    /// public ActionResult&lt;User&gt; GetUser(UserId id) =>
    ///     _repository.GetById(id)
    ///         .ToResult(Error.NotFound($"User {id} not found"))
    ///         .ToActionResult(this);
    /// 
    /// // Query parameter binding
    /// [HttpGet("search")]
    /// public ActionResult&lt;IEnumerable&lt;User&gt;&gt; SearchByEmail([FromQuery] EmailAddress email) =>
    ///     _repository.FindByEmail(email)
    ///         .ToActionResult(this);
    /// </code>
    /// </example>
    public static MvcOptions AddValueObjectModelBinding(this MvcOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // Insert at index 0 to take precedence over default binders
        options.ModelBinderProviders.Insert(0, new ValueObjectModelBinderProvider());

        return options;
    }

    /// <summary>
    /// Adds value object JSON input formatter for handling JSON request bodies with value objects.
    /// This is an optional addition that enables automatic validation of value objects in [FromBody] parameters.
    /// </summary>
    /// <param name="options">The MVC options to configure.</param>
    /// <returns>The same options instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// This input formatter solves the limitation where JSON deserialization happens before model binding.
    /// It intercepts JSON deserialization and validates value objects using ITryCreatable.TryCreate
    /// before constructing the DTO.
    /// </para>
    /// <para>
    /// <strong>Usage:</strong> Combine with <see cref="AddValueObjectModelBinding"/> for full support:
    /// <code>
    /// builder.Services.AddControllers(options =>
    /// {
    ///     options.AddValueObjectJsonInputFormatter(); // JSON bodies
    ///     options.AddValueObjectModelBinding();       // Route/query/form
    /// });
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Requirements:</strong>
    /// <list type="bullet">
    /// <item>DTOs must use record-style primary constructors</item>
    /// <item>Properties must be constructor parameters (positional records)</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Performance Note:</strong> Uses reflection for validation. For high-performance scenarios,
    /// consider using manual validation with <c>Combine</c> chains instead.
    /// </para>
    /// </remarks>
    /// <example>
    /// Full registration with JSON support:
    /// <code>
    /// builder.Services.AddControllers(options =>
    /// {
    ///     options.AddValueObjectJsonInputFormatter(); // Enable JSON body validation
    ///     options.AddValueObjectModelBinding();       // Enable route/query parameter validation
    /// });
    /// </code>
    /// </example>
    /// <example>
    /// Usage with JSON request bodies:
    /// <code>
    /// public record CreateUserRequest(
    ///     FirstName FirstName,     // Validated from JSON
    ///     LastName LastName,       // Validated from JSON
    ///     EmailAddress Email       // Validated from JSON
    /// );
    /// 
    /// [HttpPost]
    /// public ActionResult&lt;User&gt; Create([FromBody] CreateUserRequest request) =>
    ///     User.TryCreate(request.FirstName, request.LastName, request.Email)
    ///         .ToActionResult(this);
    /// 
    /// // Invalid JSON returns 400 Bad Request with field-level errors:
    /// // {
    /// //   "errors": {
    /// //     "FirstName": ["First Name cannot be empty."],
    /// //     "Email": ["Email address is not valid."]
    /// //   }
    /// // }
    /// </code>
    /// </example>
    public static MvcOptions AddValueObjectJsonInputFormatter(this MvcOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // Insert at index 0 to take precedence over default JSON formatter
        options.InputFormatters.Insert(0, new ValueObjectJsonInputFormatter());

        return options;
    }
}
