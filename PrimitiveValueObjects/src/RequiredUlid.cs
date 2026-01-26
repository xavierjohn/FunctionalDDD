namespace FunctionalDdd.PrimitiveValueObjects;

/// <summary>
/// Base class for creating strongly-typed ULID value objects that cannot have the default (empty) ULID value.
/// Provides a foundation for entity identifiers and other domain concepts represented by ULIDs.
/// </summary>
/// <remarks>
/// <para>
/// This class extends <see cref="ScalarValueObject{TSelf, T}"/> to provide a specialized base for ULID-based value objects
/// with automatic validation that prevents empty/default ULIDs. When used with the <c>partial</c> keyword,
/// the PrimitiveValueObjectGenerator source generator automatically creates:
/// <list type="bullet">
/// <item><c>IScalarValue&lt;TSelf, Ulid&gt;</c> implementation for ASP.NET Core automatic validation</item>
/// <item><c>NewUnique()</c> - Factory method for generating new unique identifiers</item>
/// <item><c>TryCreate(Ulid)</c> - Factory method for non-nullable ULIDs (required by IScalarValue)</item>
/// <item><c>TryCreate(Ulid?, string?)</c> - Factory method with empty ULID validation and custom field name</item>
/// <item><c>TryCreate(string?, string?)</c> - Factory method for parsing strings with validation</item>
/// <item><c>IParsable&lt;T&gt;</c> implementation (<c>Parse</c>, <c>TryParse</c>)</item>
/// <item>JSON serialization support via <c>ParsableJsonConverter&lt;T&gt;</c></item>
/// <item>Explicit cast operator from Ulid</item>
/// <item>OpenTelemetry activity tracing</item>
/// </list>
/// </para>
/// <para>
/// ULIDs (Universally Unique Lexicographically Sortable Identifiers) provide several advantages over GUIDs:
/// <list type="bullet">
/// <item><strong>Lexicographically sortable</strong>: ULIDs sort naturally by creation time</item>
/// <item><strong>Time-based component</strong>: First 48 bits encode millisecond timestamp</item>
/// <item><strong>Compact representation</strong>: 26-character Crockford Base32 encoding</item>
/// <item><strong>Database friendly</strong>: Better index performance due to sequential nature</item>
/// <item><strong>URL safe</strong>: Case-insensitive, no special characters</item>
/// </list>
/// </para>
/// <para>
/// Common use cases:
/// <list type="bullet">
/// <item>Entity identifiers (CustomerId, OrderId, ProductId)</item>
/// <item>Event sourcing event IDs (sortable by creation time)</item>
/// <item>Distributed system identifiers</item>
/// <item>Log correlation IDs</item>
/// <item>Any domain concept requiring a unique, sortable, non-empty identifier</item>
/// </list>
/// </para>
/// <para>
/// Benefits over plain ULIDs:
/// <list type="bullet">
/// <item><strong>Type safety</strong>: Cannot accidentally use CustomerId where OrderId is expected</item>
/// <item><strong>Validation</strong>: Prevents empty/default ULIDs at creation time</item>
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
/// public partial class OrderId : RequiredUlid&lt;OrderId&gt;
/// {
/// }
/// 
/// // The source generator automatically creates:
/// // - IScalarValue&lt;OrderId, Ulid&gt; interface implementation
/// // - public static OrderId NewUnique() => new(Ulid.NewUlid());
/// // - public static Result&lt;OrderId&gt; TryCreate(Ulid value)
/// // - public static Result&lt;OrderId&gt; TryCreate(Ulid? value, string? fieldName = null)
/// // - public static Result&lt;OrderId&gt; TryCreate(string? value, string? fieldName = null)
/// // - public static OrderId Parse(string s, IFormatProvider? provider)
/// // - public static bool TryParse(string? s, IFormatProvider? provider, out OrderId result)
/// // - public static explicit operator OrderId(Ulid value)
/// // - private OrderId(Ulid value) : base(value) { }
/// 
/// // Usage examples:
/// 
/// // Generate a new unique ID (time-ordered)
/// var orderId = OrderId.NewUnique();
/// 
/// // Create from existing ULID with validation
/// var result1 = OrderId.TryCreate(existingUlid);
/// // Returns: Success(OrderId) if ulid != default
/// // Returns: Failure(ValidationError) if ulid == default
/// 
/// // Create from string with validation
/// var result2 = OrderId.TryCreate("01ARZ3NDEKTSV4RRFFQ69G5FAV");
/// // Returns: Success(OrderId) if valid ULID format
/// // Returns: Failure(ValidationError) if invalid format or empty ULID
/// 
/// // With custom field name for validation errors
/// var result3 = OrderId.TryCreate(input, "order.id");
/// // Error field will be "order.id" instead of default "orderId"
/// 
/// // In entity constructors
/// public class Order : Entity&lt;OrderId&gt;
/// {
///     public CustomerId CustomerId { get; }
///     
///     private Order(OrderId id, CustomerId customerId) : base(id)
///     {
///         CustomerId = customerId;
///     }
///     
///     public static Result&lt;Order&gt; Create(CustomerId customerId) =>
///         customerId.ToResult()
///             .Map(c => new Order(OrderId.NewUnique(), c));
/// }
/// </code>
/// </example>
/// <example>
/// ASP.NET Core automatic validation with route parameters:
/// <code>
/// // 1. Register automatic validation in Program.cs
/// builder.Services
///     .AddControllers()
///     .AddScalarValueObjectValidation(); // Enables automatic validation!
///
/// // 2. Use value objects directly in controller actions
/// [ApiController]
/// [Route("api/orders")]
/// public class OrdersController : ControllerBase
/// {
///     [HttpGet("{id}")]
///     public async Task&lt;ActionResult&lt;Order&gt;&gt; Get(OrderId id) // Automatically validated!
///     {
///         // If we reach here, 'id' is a valid, non-empty OrderId
///         // Model binding validated it automatically
///         var order = await _repository.GetByIdAsync(id);
///         return Ok(order);
///     }
///
///     [HttpDelete("{id}")]
///     public async Task&lt;IActionResult&gt; Delete(OrderId id) // Also works here!
///     {
///         await _repository.DeleteAsync(id);
///         return NoContent();
///     }
/// }
///
/// // Invalid ULID is rejected automatically:
/// // GET /api/orders/00000000000000000000000000
/// // Response: 400 Bad Request
/// // {
/// //   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
/// //   "title": "One or more validation errors occurred.",
/// //   "status": 400,
/// //   "errors": {
/// //     "id": ["Order Id cannot be empty."]
/// //   }
/// // }
/// </code>
/// </example>
/// <example>
/// Multiple strongly-typed IDs in the same domain:
/// <code>
/// public partial class CustomerId : RequiredUlid&lt;CustomerId&gt; { }
/// public partial class OrderId : RequiredUlid&lt;OrderId&gt; { }
/// public partial class ProductId : RequiredUlid&lt;ProductId&gt; { }
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
/// <seealso cref="RequiredGuid{TSelf}"/>
/// <seealso cref="RequiredString{TSelf}"/>
public abstract class RequiredUlid<TSelf> : ScalarValueObject<TSelf, Ulid>
    where TSelf : RequiredUlid<TSelf>, IScalarValue<TSelf, Ulid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequiredUlid{TSelf}"/> class with the specified ULID value.
    /// </summary>
    /// <param name="value">The ULID value. Must not be the default empty ULID.</param>
    /// <remarks>
    /// <para>
    /// This constructor is protected and should be called by derived classes.
    /// When using the source generator (with <c>partial</c> keyword), a private constructor
    /// is automatically generated that includes validation.
    /// </para>
    /// <para>
    /// Direct instantiation should be avoided. Instead, use the generated factory methods:
    /// <list type="bullet">
    /// <item><c>NewUnique()</c> - Generate a new unique ULID</item>
    /// <item><c>TryCreate(Ulid?, string?)</c> - Create from ULID with validation</item>
    /// <item><c>TryCreate(string?, string?)</c> - Create from string with validation</item>
    /// </list>
    /// </para>
    /// </remarks>
    protected RequiredUlid(Ulid value) : base(value)
    {
    }
}