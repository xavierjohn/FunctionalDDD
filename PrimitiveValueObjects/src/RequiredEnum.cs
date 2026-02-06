namespace FunctionalDdd.PrimitiveValueObjects;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Base class for creating strongly-typed enum value objects with source-generated ASP.NET Core support.
/// Extends <see cref="EnumValueObject{TSelf}"/> to provide automatic model binding, JSON validation, and error collection.
/// </summary>
/// <typeparam name="TSelf">The derived enum value object type itself (CRTP pattern).</typeparam>
/// <remarks>
/// <para>
/// This class combines the power of <see cref="EnumValueObject{TSelf}"/> with the PrimitiveValueObjectGenerator
/// source generator to provide:
/// <list type="bullet">
/// <item><c>IScalarValue&lt;TSelf, string&gt;</c> implementation for ASP.NET Core automatic validation</item>
/// <item><c>TryCreate(string)</c> - Factory method for non-nullable strings (required by IScalarValue)</item>
/// <item><c>TryCreate(string?, string?)</c> - Factory method with validation and custom field name</item>
/// <item><c>IParsable&lt;T&gt;</c> implementation (<c>Parse</c>, <c>TryParse</c>)</item>
/// <item>JSON serialization support via <c>EnumValueObjectJsonConverter&lt;T&gt;</c></item>
/// <item>ASP.NET Core model binding from route/query/form/headers</item>
/// <item>OpenTelemetry activity tracing</item>
/// </list>
/// </para>
/// <para>
/// Common use cases:
/// <list type="bullet">
/// <item>Order/payment/shipping statuses</item>
/// <item>User roles and permissions</item>
/// <item>Document states in workflows</item>
/// <item>Any finite set of domain values with behavior</item>
/// </list>
/// </para>
/// <para>
/// Benefits over plain C# enums:
/// <list type="bullet">
/// <item><strong>Type safety</strong>: Invalid values are impossible (no <c>(OrderStatus)999</c>)</item>
/// <item><strong>Behavior</strong>: Each value can have associated methods and properties</item>
/// <item><strong>ASP.NET integration</strong>: Automatic validation with error collection</item>
/// <item><strong>Extensibility</strong>: Add computed properties and domain logic</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Creating a strongly-typed enum value object:
/// <code><![CDATA[
/// // Define the enum value object (partial keyword enables source generation)
/// public partial class OrderState : RequiredEnum<OrderState>
/// {
///     public static readonly OrderState Draft = new();
///     public static readonly OrderState Confirmed = new();
///     public static readonly OrderState Shipped = new();
///     public static readonly OrderState Delivered = new();
///     public static readonly OrderState Cancelled = new();
/// }
/// 
/// // The source generator automatically creates:
/// // - IScalarValue<OrderState, string> interface implementation
/// // - public static Result<OrderState> TryCreate(string value)
/// // - public static Result<OrderState> TryCreate(string? value, string? fieldName = null)
/// // - public static OrderState Parse(string s, IFormatProvider? provider)
/// // - public static bool TryParse(string? s, IFormatProvider? provider, out OrderState result)
/// // - [JsonConverter(typeof(EnumValueObjectJsonConverter<OrderState>))] attribute
/// 
/// // Usage examples:
/// var result = OrderState.TryCreate("Draft");           // Result<OrderState>
/// var state = OrderState.Parse("Confirmed");            // Throws on invalid
/// var all = OrderState.GetAll();                        // All defined values
/// ]]></code>
/// </example>
/// <example>
/// Enum value object with behavior:
/// <code><![CDATA[
/// public partial class PaymentMethod : RequiredEnum<PaymentMethod>
/// {
///     public static readonly PaymentMethod CreditCard = new(fee: 0.029m);
///     public static readonly PaymentMethod BankTransfer = new(fee: 0.005m);
///     public static readonly PaymentMethod Cash = new(fee: 0m);
///     
///     public decimal Fee { get; }
///     
///     private PaymentMethod(decimal fee) => Fee = fee;
///     
///     public decimal CalculateFee(decimal amount) => amount * Fee;
/// }
/// ]]></code>
/// </example>
/// <example>
/// Using in ASP.NET Core DTOs with automatic validation:
/// <code><![CDATA[
/// public record UpdateOrderDto
/// {
///     public OrderState State { get; init; } = null!;
/// }
/// 
/// // In controller - validation happens automatically
/// [HttpPut("{id}")]
/// public IActionResult UpdateOrder(Guid id, UpdateOrderDto dto)
/// {
///     // If we reach here, dto.State is already validated!
///     return Ok(_orderService.UpdateState(id, dto.State));
/// }
/// ]]></code>
/// </example>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix - RequiredEnum is a valid DDD pattern name
public abstract class RequiredEnum<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TSelf>
    : EnumValueObject<TSelf>
    where TSelf : RequiredEnum<TSelf>
#pragma warning restore CA1711
{
    /// <summary>
    /// Initializes a new instance. The Name is auto-derived from the field name.
    /// </summary>
    protected RequiredEnum()
    {
    }
}
