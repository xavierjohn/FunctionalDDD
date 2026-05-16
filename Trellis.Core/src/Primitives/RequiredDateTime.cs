namespace Trellis;

/// <summary>
/// Base class for creating strongly-typed DateTime value objects that prevent primitive
/// obsession for dates. Rejects only <c>null</c> by default; <see cref="DateTime.MinValue"/>
/// rejection is opt-in via the <see cref="NotDefaultAttribute"/> attribute. Recommended for
/// any DateTime type used as an EF-mapped property to preserve the database invariant
/// guarantee enforced by <c>TrellisScalarConverter</c> on rehydration.
/// </summary>
/// <remarks>
/// <para>
/// This class extends <see cref="ScalarValueObject{TSelf, T}"/> to provide a specialized base
/// for DateTime-based value objects. When used with the <c>partial</c> keyword,
/// the PrimitiveValueObjectGenerator source generator automatically creates:
/// <list type="bullet">
/// <item><c>IScalarValue&lt;TSelf, DateTime&gt;</c> implementation for ASP.NET Core automatic validation</item>
/// <item><c>TryCreate(DateTime)</c> - Factory method for DateTimes (required by IScalarValue)</item>
/// <item><c>TryCreate(DateTime?, string?)</c> - Factory method with null validation and custom field name</item>
/// <item><c>TryCreate(string?, string?)</c> - Factory method for parsing strings with validation</item>
/// <item><c>IParsable&lt;T&gt;</c> implementation (<c>Parse</c>, <c>TryParse</c>)</item>
/// <item>JSON serialization support via <c>ParsableJsonConverter&lt;T&gt;</c></item>
/// <item>Explicit cast operator from DateTime</item>
/// <item>OpenTelemetry activity tracing</item>
/// </list>
/// </para>
/// <para>
/// Validation rules emitted by the source generator:
/// <list type="bullet">
/// <item><c>null</c> is always rejected with the per-type "cannot be empty." message.</item>
/// <item>Apply <see cref="NotDefaultAttribute"/> to additionally reject <see cref="DateTime.MinValue"/> with the per-type "cannot be DateTime.MinValue." message. Recommended for any DateTime used as an EF-mapped property to preserve the database invariant guarantee enforced by <c>TrellisScalarConverter</c> on rehydration.</item>
/// </list>
/// </para>
/// <para>
/// Common use cases:
/// <list type="bullet">
/// <item>Order dates (OrderDate, ShipDate)</item>
/// <item>Personal dates (BirthDate, HireDate)</item>
/// <item>Scheduling (DueDate, StartDate, EndDate)</item>
/// <item>Any domain concept requiring a non-default DateTime</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Lenient default — only <c>null</c> is rejected:
/// <code>
/// public partial class EventTimestamp : RequiredDateTime&lt;EventTimestamp&gt; { }
///
/// var ok = EventTimestamp.TryCreate(DateTime.UtcNow);              // Success
/// var min = EventTimestamp.TryCreate(DateTime.MinValue);           // Success (lenient)
/// var nul = EventTimestamp.TryCreate((DateTime?)null);             // Failure
/// </code>
/// </example>
/// <example>
/// Strict opt-in — <see cref="DateTime.MinValue"/> rejected:
/// <code>
/// [NotDefault]
/// public partial class OrderDate : RequiredDateTime&lt;OrderDate&gt; { }
///
/// var ok = OrderDate.TryCreate(DateTime.UtcNow);                   // Success
/// var min = OrderDate.TryCreate(DateTime.MinValue);
/// // Failure: "Order Date cannot be DateTime.MinValue."
/// </code>
/// </example>
/// <seealso cref="ScalarValueObject{TSelf, T}"/>
/// <seealso cref="RequiredInt{TSelf}"/>
public abstract class RequiredDateTime<TSelf> : ScalarValueObject<TSelf, DateTime>
    where TSelf : RequiredDateTime<TSelf>, IScalarValue<TSelf, DateTime>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequiredDateTime{TSelf}"/> class with the specified DateTime value.
    /// </summary>
    /// <param name="value">The DateTime value.</param>
    protected RequiredDateTime(DateTime value) : base(value)
    {
    }

    /// <summary>
    /// Returns the DateTime in ISO 8601 round-trip format ("O") using invariant culture.
    /// This ensures JSON serialization via ParsableJsonConverter is deterministic,
    /// culture-independent, and preserves DateTimeKind (UTC/Local/Unspecified).
    /// </summary>
    public override string ToString() => Value.ToString("O", System.Globalization.CultureInfo.InvariantCulture);
}