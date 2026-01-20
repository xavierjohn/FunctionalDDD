namespace FunctionalDdd;

/// <summary>
/// Base class for creating strongly-typed GUID value objects that cannot have the default (empty) GUID value.
/// Provides a foundation for entity identifiers and other domain concepts represented by GUIDs.
/// </summary>
/// <remarks>
/// <para>
/// This class extends <see cref="ScalarValueObject{TSelf, T}"/> to provide a specialized base for GUID-based value objects
/// with automatic validation that prevents empty/default GUIDs. When used with the <c>partial</c> keyword,
/// the PrimitiveValueObjectGenerator source generator automatically creates:
/// <list type="bullet">
/// <item>Static factory methods (<c>NewUnique</c>, <c>TryCreate</c>)</item>
/// <item><c>IParsable&lt;T&gt;</c> implementation (<c>Parse</c>, <c>TryParse</c>)</item>
/// <item>Validation logic that ensures non-empty GUIDs</item>
/// <item>JSON serialization support via <c>ParsableJsonConverter&lt;T&gt;</c></item>
/// <item>OpenTelemetry activity tracing</item>
/// </list>
/// </para>
/// <para>
/// Common use cases:
/// <list type="bullet">
/// <item>Entity identifiers (CustomerId, OrderId, ProductId)</item>
/// <item>Correlation IDs for distributed tracing</item>
/// <item>Session or transaction identifiers</item>
/// <item>Any domain concept requiring a globally unique, non-empty identifier</item>
/// </list>
/// </para>
/// <para>
/// Benefits over plain GUIDs:
/// <list type="bullet">
/// <item><strong>Type safety</strong>: Cannot accidentally use CustomerId where OrderId is expected</item>
/// <item><strong>Validation</strong>: Prevents empty/default GUIDs at creation time</item>
/// <item><strong>Domain clarity</strong>: Makes code more self-documenting and expressive</item>
/// <item><strong>Serialization</strong>: Consistent JSON and database representation</item>
/// <item><strong>Factory methods</strong>: Clean API for creating new unique identifiers</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Creating a strongly-typed entity identifier:
/// <code>
/// // Define the value object (partial keyword enables source generation)
/// public partial class CustomerId : RequiredGuid&lt;CustomerId&gt;
/// {
/// }
/// 
/// // The source generator automatically creates:
/// // - public static CustomerId NewUnique() => new(Guid.NewGuid());
/// // - public static Result&lt;CustomerId&gt; TryCreate(Guid? value, string? fieldName = null)
/// // - public static Result&lt;CustomerId&gt; TryCreate(string? value, string? fieldName = null)
/// // - public static CustomerId Parse(string s, IFormatProvider? provider)
/// // - public static bool TryParse(string? s, IFormatProvider? provider, out CustomerId result)
/// // - private CustomerId(Guid value) : base(value) { }
/// 
/// // Usage examples:
/// 
/// // Generate a new unique ID
/// var customerId = CustomerId.NewUnique();
/// 
/// // Create from existing GUID with validation
/// var result1 = CustomerId.TryCreate(existingGuid);
/// // Returns: Success(CustomerId) if guid != Guid.Empty
/// // Returns: Failure(ValidationError) if guid == Guid.Empty
/// 
/// // Create from string with validation
/// var result2 = CustomerId.TryCreate("550e8400-e29b-41d4-a716-446655440000");
/// // Returns: Success(CustomerId) if valid GUID format
/// // Returns: Failure(ValidationError) if invalid format or empty GUID
/// 
/// // With custom field name for validation errors
/// var result3 = CustomerId.TryCreate(input, "customer.id");
/// // Error field will be "customer.id" instead of default "customerId"
/// 
/// // In entity constructors
/// public class Customer : Entity&lt;CustomerId&gt;
/// {
///     public EmailAddress Email { get; }
///     
///     private Customer(CustomerId id, EmailAddress email) : base(id)
///     {
///         Email = email;
///     }
///     
///     public static Result&lt;Customer&gt; Create(EmailAddress email) =>
///         email.ToResult()
///             .Map(e => new Customer(CustomerId.NewUnique(), e));
/// }
/// </code>
/// </example>
/// <example>
/// Using in API endpoints with automatic JSON serialization:
/// <code>
/// // Request DTO
/// public record GetCustomerRequest(CustomerId CustomerId);
/// 
/// // API endpoint
/// app.MapGet("/customers/{id}", (string id) =>
///     CustomerId.TryCreate(id)
///         .Bind(_customerRepository.GetAsync)
///         .Map(customer => new CustomerDto(customer))
///         .ToHttpResult());
/// 
/// // JSON request/response examples:
/// // Request: GET /customers/550e8400-e29b-41d4-a716-446655440000
/// // Response: { "customerId": "550e8400-e29b-41d4-a716-446655440000", "name": "John" }
/// // 
/// // Invalid GUID: GET /customers/00000000-0000-0000-0000-000000000000
/// // Response: 400 Bad Request with ValidationError
/// </code>
/// </example>
/// <example>
/// Multiple strongly-typed IDs in the same domain:
/// <code>
/// public partial class CustomerId : RequiredGuid&lt;CustomerId&gt; { }
/// public partial class OrderId : RequiredGuid&lt;OrderId&gt; { }
/// public partial class ProductId : RequiredGuid&lt;ProductId&gt; { }
/// 
/// public class Order : Entity&lt;OrderId&gt;
/// {
///     public CustomerId CustomerId { get; }
///     private readonly List&lt;OrderLine&gt; _lines = [];
///     
///     public Result&lt;Order&gt; AddLine(ProductId productId, int quantity) =>
///         this.ToResult()
///             .Ensure(_ => quantity > 0, Error.Validation("Quantity must be positive"))
///             .Tap(_ => _lines.Add(new OrderLine(productId, quantity)));
///     
///     // Compiler prevents mixing IDs:
///     // AddLine(customerId, 5); // Won't compile - type safety!
/// }
/// </code>
/// </example>
/// <seealso cref="ScalarValueObject{TSelf, T}"/>
/// <seealso cref="RequiredString{TSelf}"/>
public abstract class RequiredGuid<TSelf> : ScalarValueObject<TSelf, Guid>
    where TSelf : RequiredGuid<TSelf>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequiredGuid{TSelf}"/> class with the specified GUID value.
    /// </summary>
    /// <param name="value">The GUID value. Must not be <see cref="Guid.Empty"/>.</param>
    /// <remarks>
    /// <para>
    /// This constructor is protected and should be called by derived classes.
    /// When using the source generator (with <c>partial</c> keyword), a private constructor
    /// is automatically generated that includes validation.
    /// </para>
    /// <para>
    /// Direct instantiation should be avoided. Instead, use the generated factory methods:
    /// <list type="bullet">
    /// <item><c>NewUnique()</c> - Generate a new unique GUID</item>
    /// <item><c>TryCreate(Guid?, string?)</c> - Create from GUID with validation</item>
    /// <item><c>TryCreate(string?, string?)</c> - Create from string with validation</item>
    /// </list>
    /// </para>
    /// </remarks>
    protected RequiredGuid(Guid value) : base(value)
    {
    }
}
