namespace FunctionalDdd;

/// <summary>
/// Base class for creating strongly-typed string value objects that cannot be null or empty.
/// Provides a foundation for domain primitives like names, descriptions, codes, and other textual concepts.
/// </summary>
/// <remarks>
/// <para>
/// This class extends <see cref="ScalarValueObject{T}"/> to provide a specialized base for string-based value objects
/// with automatic validation that prevents null or empty strings. When used with the <c>partial</c> keyword,
/// the PrimitiveValueObjectGenerator source generator automatically creates:
/// <list type="bullet">
/// <item>Static factory method (<c>TryCreate</c>) with null/empty/whitespace validation</item>
/// <item><c>IParsable&lt;T&gt;</c> implementation (<c>Parse</c>, <c>TryParse</c>)</item>
/// <item>JSON serialization support via <c>ParsableJsonConverter&lt;T&gt;</c></item>
/// <item>Explicit cast operator from string</item>
/// <item>OpenTelemetry activity tracing</item>
/// </list>
/// </para>
/// <para>
/// Common use cases:
/// <list type="bullet">
/// <item>Person names (FirstName, LastName, FullName)</item>
/// <item>Product attributes (ProductName, Description, SKU)</item>
/// <item>Location data (City, State, Country, PostalCode)</item>
/// <item>Business identifiers (CompanyName, TaxId, AccountNumber)</item>
/// <item>Any domain concept represented by required text</item>
/// </list>
/// </para>
/// <para>
/// Benefits over plain strings:
/// <list type="bullet">
/// <item><strong>Type safety</strong>: Cannot mix FirstName with LastName</item>
/// <item><strong>Validation</strong>: Prevents null/empty strings at creation time</item>
/// <item><strong>Domain clarity</strong>: Self-documenting code that expresses intent</item>
/// <item><strong>Consistency</strong>: Centralized trimming and normalization</item>
/// <item><strong>Testability</strong>: Easy to test validation rules in isolation</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Creating a strongly-typed name value object:
/// <code>
/// // Define the value object (partial keyword enables source generation)
/// public partial class FirstName : RequiredString
/// {
/// }
/// 
/// // The source generator automatically creates:
/// // - public static Result&lt;FirstName&gt; TryCreate(string? value, string? fieldName = null)
/// // - public static FirstName Parse(string s, IFormatProvider? provider)
/// // - public static bool TryParse(string? s, IFormatProvider? provider, out FirstName result)
/// // - public static explicit operator FirstName(string value)
/// // - private FirstName(string value) : base(value) { }
/// 
/// // Usage examples:
/// 
/// // Create with validation
/// var result1 = FirstName.TryCreate("John");
/// // Returns: Success(FirstName("John"))
/// 
/// var result2 = FirstName.TryCreate("");
/// // Returns: Failure(ValidationError("First Name cannot be empty."))
/// 
/// var result3 = FirstName.TryCreate(null);
/// // Returns: Failure(ValidationError("First Name cannot be empty."))
/// 
/// var result4 = FirstName.TryCreate("  John  ");
/// // Returns: Success(FirstName("John")) - automatically trimmed
/// 
/// // With custom field name for validation errors
/// var result5 = FirstName.TryCreate(input, "user.firstName");
/// // Error field will be "user.firstName" instead of default "firstName"
/// 
/// // Using in entity creation
/// public class Person : Entity&lt;PersonId&gt;
/// {
///     public FirstName FirstName { get; }
///     public LastName LastName { get; }
///     
///     public static Result&lt;Person&gt; Create(string firstName, string lastName) =>
///         FirstName.TryCreate(firstName)
///             .Combine(LastName.TryCreate(lastName))
///             .Map((first, last) => new Person(PersonId.NewUnique(), first, last));
///     
///     private Person(PersonId id, FirstName firstName, LastName lastName) 
///         : base(id)
///     {
///         FirstName = firstName;
///         LastName = lastName;
///     }
/// }
/// </code>
/// </example>
/// <example>
/// Using in API validation:
/// <code>
/// // Request DTO
/// public record CreateUserRequest(string FirstName, string LastName, string Email);
/// 
/// // API endpoint with automatic validation
/// app.MapPost("/users", (CreateUserRequest request) =>
///     FirstName.TryCreate(request.FirstName, nameof(request.FirstName))
///         .Combine(LastName.TryCreate(request.LastName, nameof(request.LastName)))
///         .Combine(EmailAddress.TryCreate(request.Email, nameof(request.Email)))
///         .Bind((first, last, email) => User.Create(first, last, email))
///         .ToHttpResult());
/// 
/// // POST /users with empty FirstName:
/// // Response: 400 Bad Request
/// // {
/// //   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
/// //   "title": "One or more validation errors occurred.",
/// //   "status": 400,
/// //   "errors": {
/// //     "firstName": ["First Name cannot be empty."]
/// //   }
/// // }
/// </code>
/// </example>
/// <example>
/// Multiple string-based value objects:
/// <code>
/// public partial class FirstName : RequiredString { }
/// public partial class LastName : RequiredString { }
/// public partial class CompanyName : RequiredString { }
/// public partial class ProductName : RequiredString { }
/// public partial class Description : RequiredString { }
/// 
/// public class Product : Entity&lt;ProductId&gt;
/// {
///     public ProductName Name { get; private set; }
///     public Description Description { get; private set; }
///     
///     public Result&lt;Product&gt; UpdateName(ProductName newName) =>
///         newName.ToResult()
///             .Tap(name => Name = name)
///             .Map(_ => this);
///     
///     // Compiler prevents mixing types:
///     // UpdateName(description); // Won't compile!
///     // UpdateName(firstName);   // Won't compile!
/// }
/// </code>
/// </example>
/// <example>
/// Advanced: Adding custom validation to derived types:
/// <code>
/// // While RequiredString handles null/empty, you can add domain-specific rules
/// public partial class ProductSKU : RequiredString
/// {
///     // Additional validation can be done in factory methods
///     public static Result&lt;ProductSKU&gt; TryCreateWithValidation(string? value) =>
///         TryCreate(value) // Use generated validation first
///             .Ensure(sku => sku.Value.Length &lt;= 20,
///                    Error.Validation("SKU must be 20 characters or less", "sku"))
///             .Ensure(sku => sku.Value.All(c => char.IsLetterOrDigit(c) || c == '-'),
///                    Error.Validation("SKU can only contain letters, digits, and hyphens", "sku"));
/// }
/// 
/// // Usage
/// var result = ProductSKU.TryCreateWithValidation("PROD-12345");
/// // Success
/// 
/// var invalid = ProductSKU.TryCreateWithValidation("PROD@12345");
/// // Failure: "SKU can only contain letters, digits, and hyphens"
/// </code>
/// </example>
/// <seealso cref="ScalarValueObject{T}"/>
/// <seealso cref="RequiredGuid"/>
/// <seealso cref="EmailAddress"/>
public abstract class RequiredString : ScalarValueObject<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequiredString"/> class with the specified string value.
    /// </summary>
    /// <param name="value">The string value. Must not be null or empty.</param>
    /// <remarks>
    /// <para>
    /// This constructor is protected and should be called by derived classes.
    /// When using the source generator (with <c>partial</c> keyword), a private constructor
    /// is automatically generated that includes validation and trimming.
    /// </para>
    /// <para>
    /// Direct instantiation should be avoided. Instead, use the generated factory method:
    /// <list type="bullet">
    /// <item><c>TryCreate(string?, string?)</c> - Create from string with validation and trimming</item>
    /// </list>
    /// </para>
    /// <para>
    /// The generated TryCreate method automatically:
    /// <list type="bullet">
    /// <item>Returns validation error for null values</item>
    /// <item>Returns validation error for empty strings</item>
    /// <item>Returns validation error for whitespace-only strings</item>
    /// <item>Trims leading and trailing whitespace from valid strings</item>
    /// </list>
    /// </para>
    /// </remarks>
    protected RequiredString(string value) : base(value)
    {
    }
}
