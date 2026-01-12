namespace FunctionalDdd;

/// <summary>
/// Defines a contract for types that can be created from a string representation with validation.
/// Implementing types must provide a static TryCreate method that returns a Result indicating success or failure.
/// </summary>
/// <typeparam name="T">The type that implements this interface.</typeparam>
/// <remarks>
/// <para>
/// This interface enables Railway Oriented Programming patterns by ensuring that object creation
/// can fail gracefully and return structured error information instead of throwing exceptions.
/// </para>
/// <para>
/// The TryCreate pattern provides:
/// <list type="bullet">
/// <item>Explicit validation at creation time</item>
/// <item>Structured error information via Result type</item>
/// <item>Composable validation chains using Combine and Bind</item>
/// <item>Integration with ASP.NET Core model binding</item>
/// </list>
/// </para>
/// <para>
/// Common implementations include:
/// <list type="bullet">
/// <item>Value objects (EmailAddress, FirstName, LastName)</item>
/// <item>Entity identifiers (UserId, OrderId)</item>
/// <item>Domain primitives with validation rules</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Implementing ITryCreatable for a simple value object:
/// <code>
/// public class EmailAddress : ITryCreatable&lt;EmailAddress&gt;
/// {
///     public string Value { get; }
///     
///     private EmailAddress(string value) => Value = value;
///     
///     public static Result&lt;EmailAddress&gt; TryCreate(string? value, string? fieldName = null)
///     {
///         var field = fieldName ?? "email";
///         
///         if (string.IsNullOrWhiteSpace(value))
///             return Error.Validation("Email address cannot be empty", field);
///             
///         if (!value.Contains('@'))
///             return Error.Validation("Email address is not valid", field);
///             
///         return new EmailAddress(value);
///     }
/// }
/// </code>
/// </example>
/// <example>
/// Using ITryCreatable in validation chains:
/// <code>
/// // Validate multiple inputs together
/// var result = EmailAddress.TryCreate(emailInput)
///     .Combine(FirstName.TryCreate(firstNameInput))
///     .Combine(LastName.TryCreate(lastNameInput))
///     .Bind((email, first, last) => User.Create(email, first, last));
///     
/// // Returns success with User instance, or failure with all validation errors
/// </code>
/// </example>
/// <example>
/// Automatic ASP.NET Core model binding:
/// <code>
/// // In Startup.cs or Program.cs
/// builder.Services.AddControllers(options =>
/// {
///     options.AddValueObjectModelBinding();  // Enable automatic validation
/// });
/// 
/// // In controller
/// [HttpGet("{id}")]
/// public ActionResult&lt;User&gt; GetUser(UserId id)  // Automatically validated
/// {
///     return _repository.GetById(id)
///         .ToResult(Error.NotFound($"User {id} not found"))
///         .ToActionResult(this);
/// }
/// 
/// // Invalid ID in route returns 400 Bad Request automatically
/// </code>
/// </example>
/// <example>
/// Using in DTOs with JSON deserialization:
/// <code>
/// public record CreateUserRequest(
///     EmailAddress Email,      // Validated from JSON
///     FirstName FirstName,     // Validated from JSON
///     LastName LastName        // Validated from JSON
/// );
/// 
/// [HttpPost]
/// public ActionResult&lt;User&gt; Create([FromBody] CreateUserRequest request) =>
///     User.Create(request.Email, request.FirstName, request.LastName)
///         .ToActionResult(this);
///         
/// // Invalid JSON values return 400 with detailed field-level errors
/// </code>
/// </example>
public interface ITryCreatable<T> where T : ITryCreatable<T>
{
    /// <summary>
    /// Attempts to create an instance of <typeparamref name="T"/> from a string value.
    /// </summary>
    /// <param name="value">The string value to parse and validate.</param>
    /// <param name="fieldName">
    /// Optional field name for error messages. If null, implementations should use a default
    /// field name based on the type name (e.g., "emailAddress" for EmailAddress type).
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing either:
    /// <list type="bullet">
    /// <item>Success with the created instance if validation passes</item>
    /// <item>Failure with a <see cref="ValidationError"/> describing what went wrong</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// Implementations should:
    /// <list type="bullet">
    /// <item>Return a ValidationError for null, empty, or invalid values</item>
    /// <item>Use the provided fieldName in error messages, or generate one from the type name</item>
    /// <item>Never throw exceptions - always return Result</item>
    /// <item>Trim whitespace if appropriate for the domain type</item>
    /// <item>Normalize values if needed (e.g., lowercase email addresses)</item>
    /// </list>
    /// </para>
    /// <para>
    /// The method must be static to enable creation without an existing instance.
    /// This pattern is compatible with both imperative and functional programming styles.
    /// </para>
    /// </remarks>
    /// <example>
    /// Basic implementation with validation:
    /// <code>
    /// public static Result&lt;EmailAddress&gt; TryCreate(string? value, string? fieldName = null)
    /// {
    ///     var field = fieldName ?? "email";
    ///     
    ///     if (string.IsNullOrWhiteSpace(value))
    ///         return Error.Validation("Email address is required", field);
    ///         
    ///     var trimmed = value.Trim();
    ///     
    ///     if (!trimmed.Contains('@'))
    ///         return Error.Validation("Email address must contain @", field);
    ///         
    ///     if (trimmed.Length > 254)
    ///         return Error.Validation("Email address is too long", field);
    ///         
    ///     return new EmailAddress(trimmed.ToLowerInvariant());
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// Implementation with multiple validation rules:
    /// <code>
    /// public static Result&lt;UserId&gt; TryCreate(string? value, string? fieldName = null)
    /// {
    ///     var field = fieldName ?? "userId";
    ///     
    ///     return value
    ///         .ToResult(Error.Validation("User ID is required", field))
    ///         .Ensure(v => Guid.TryParse(v, out _),
    ///                Error.Validation("User ID must be a valid GUID", field))
    ///         .Ensure(v => Guid.Parse(v) != Guid.Empty,
    ///                Error.Validation("User ID cannot be empty", field))
    ///         .Map(v => new UserId(Guid.Parse(v)));
    /// }
    /// </code>
    /// </example>
    static abstract Result<T> TryCreate(string? value, string? fieldName = null);
}
