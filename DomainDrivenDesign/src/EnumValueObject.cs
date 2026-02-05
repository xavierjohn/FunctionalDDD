namespace FunctionalDdd;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

/// <summary>
/// Base class for creating strongly-typed, behavior-rich enumeration value objects.
/// Enum value objects are a DDD pattern that replaces C# enums with full-featured classes.
/// </summary>
/// <typeparam name="TSelf">The derived enum value object type itself (CRTP pattern).</typeparam>
/// <remarks>
/// <para>
/// Enum value objects address limitations of C# enums:
/// <list type="bullet">
/// <item><strong>Behavior</strong>: Each value can have associated behavior and properties</item>
/// <item><strong>Type safety</strong>: Invalid values are impossible (no <c>(OrderStatus)999</c>)</item>
/// <item><strong>Extensibility</strong>: Add methods, computed properties, and domain logic</item>
/// <item><strong>State machines</strong>: Model valid transitions between states</item>
/// </list>
/// </para>
/// <para>
/// Each enum value object member is defined as a static readonly field:
/// <list type="bullet">
/// <item>Members are discovered via reflection and cached for performance</item>
/// <item>The <see cref="Name"/> property is auto-derived from the field name (infrastructure concern)</item>
/// <item>The <see cref="Value"/> property is auto-generated for persistence (infrastructure concern)</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Basic enum value object (pure domain - no strings needed):
/// <code><![CDATA[
/// public class OrderState : EnumValueObject<OrderState>
/// {
///     public static readonly OrderState Draft = new();
///     public static readonly OrderState Confirmed = new();
///     public static readonly OrderState Shipped = new();
///     
///     private OrderState() { }
/// }
/// 
/// // Usage - Name is auto-derived from field name
/// var state = OrderState.Draft;           // Name = "Draft"
/// var all = OrderState.GetAll();
/// var result = OrderState.TryFromName("Draft");  // Result<OrderState>
/// ]]></code>
/// </example>
/// <example>
/// Enum value object with behavior:
/// <code><![CDATA[
/// public class PaymentMethod : EnumValueObject<PaymentMethod>
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
#pragma warning disable CA1000 // Do not declare static members on generic types - required for factory pattern
public abstract class EnumValueObject<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TSelf>
    : IEquatable<EnumValueObject<TSelf>>, IComparable<EnumValueObject<TSelf>>
    where TSelf : EnumValueObject<TSelf>
{
    private static readonly ConcurrentDictionary<Type, (List<TSelf> Members, Dictionary<string, TSelf> ByName)> s_cache = new();

    /// <summary>
    /// Gets the name of this enum value object member.
    /// Auto-derived from the field name during discovery.
    /// This is an infrastructure concern for serialization and display.
    /// </summary>
    /// <remarks>
    /// Name is lazily initialized on first access to avoid chicken-and-egg issues
    /// with static field initialization order.
    /// </remarks>
    public string Name => _name ?? InitializeName();

    private string? _name;

    private string InitializeName()
    {
        _ = GetCache(); // Populates _name
        return _name!;
    }

    /// <summary>
    /// Gets the auto-generated integer value for persistence.
    /// This is an infrastructure concern - values are assigned based on declaration order (0, 1, 2, ...).
    /// </summary>
    public int Value { get; private set; }

    /// <summary>
    /// Initializes a new instance. The Name is auto-derived from the field name.
    /// </summary>
    protected EnumValueObject()
    {
        // Name and Value are set during discovery via reflection
    }

    /// <summary>
    /// Gets all defined members of this enum value object type.
    /// </summary>
    public static IReadOnlyCollection<TSelf> GetAll() => GetCache().Members;

    /// <summary>
    /// Attempts to find a member by its name (case-insensitive).
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <param name="fieldName">Optional field name for validation error messages.</param>
    /// <returns>A <see cref="Result{TSelf}"/> containing the matching member or a validation error.</returns>
    public static Result<TSelf> TryFromName(string? name, string? fieldName = null)
    {
        var field = NormalizeFieldName(fieldName, typeof(TSelf).Name);

        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation($"{typeof(TSelf).Name} cannot be empty.", field);

        var cache = GetCache();
        if (cache.ByName.TryGetValue(name, out var member))
            return member;

        var validNames = string.Join(", ", cache.ByName.Keys.OrderBy(n => n));
        return Error.Validation($"'{name}' is not a valid {typeof(TSelf).Name}. Valid values: {validNames}", field);
    }

    /// <summary>
    /// Gets a member by its name (case-insensitive). Throws if not found.
    /// </summary>
    public static TSelf FromName(string name)
    {
        var result = TryFromName(name);
        if (result.IsFailure)
            throw new InvalidOperationException($"Failed to create {typeof(TSelf).Name}: {result.Error.Detail}");

        return result.Value;
    }

    /// <summary>
    /// Attempts to find a member by its name (case-insensitive).
    /// </summary>
    public static bool TryFromName(string? name, [NotNullWhen(true)] out TSelf? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return GetCache().ByName.TryGetValue(name, out result);
    }

    /// <summary>
    /// Checks if this instance is one of the specified values.
    /// </summary>
    public bool Is(params TSelf[] values) => values.Contains((TSelf)this);

    /// <summary>
    /// Checks if this instance is not one of the specified values.
    /// </summary>
    public bool IsNot(params TSelf[] values) => !Is(values);

    /// <inheritdoc />
    public override string ToString() => Name;

    /// <inheritdoc />
    public override int GetHashCode() => Name.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is EnumValueObject<TSelf> other && Equals(other);

    /// <inheritdoc />
    public bool Equals(EnumValueObject<TSelf>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public int CompareTo(EnumValueObject<TSelf>? other) =>
        other is null ? 1 : Value.CompareTo(other.Value);

    /// <summary>Determines whether two instances are equal.</summary>
    public static bool operator ==(EnumValueObject<TSelf>? left, EnumValueObject<TSelf>? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Determines whether two instances are not equal.</summary>
    public static bool operator !=(EnumValueObject<TSelf>? left, EnumValueObject<TSelf>? right) => !(left == right);

    /// <summary>Determines whether left is less than right.</summary>
    public static bool operator <(EnumValueObject<TSelf>? left, EnumValueObject<TSelf>? right) =>
        left is null ? right is not null : left.CompareTo(right) < 0;

    /// <summary>Determines whether left is less than or equal to right.</summary>
    public static bool operator <=(EnumValueObject<TSelf>? left, EnumValueObject<TSelf>? right) =>
        left is null || left.CompareTo(right) <= 0;

    /// <summary>Determines whether left is greater than right.</summary>
    public static bool operator >(EnumValueObject<TSelf>? left, EnumValueObject<TSelf>? right) =>
        left is not null && left.CompareTo(right) > 0;

    /// <summary>Determines whether left is greater than or equal to right.</summary>
    public static bool operator >=(EnumValueObject<TSelf>? left, EnumValueObject<TSelf>? right) =>
        left is null ? right is null : left.CompareTo(right) >= 0;

    /// <summary>Implicitly converts to string (the Name).</summary>
    public static implicit operator string(EnumValueObject<TSelf> enumValueObject) => enumValueObject.Name;

    private static (List<TSelf> Members, Dictionary<string, TSelf> ByName) GetCache() =>
        s_cache.GetOrAdd(typeof(TSelf), _ =>
        {
            var members = DiscoverMembers().ToList();
            var byName = members.ToDictionary(m => m.Name, StringComparer.OrdinalIgnoreCase);
            return (members, byName);
        });

    private static IEnumerable<TSelf> DiscoverMembers()
    {
        var fields = typeof(TSelf).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        var index = 0;

        foreach (var field in fields)
        {
            if (field.FieldType == typeof(TSelf) && field.IsInitOnly && field.GetValue(null) is TSelf member)
            {
                // Auto-derive Name from field name and assign Value
                member._name = field.Name;
                member.Value = index++;
                yield return member;
            }
        }
    }

    private static string NormalizeFieldName(string? fieldName, string typeName) =>
        !string.IsNullOrEmpty(fieldName)
            ? (fieldName.Length == 1 ? fieldName.ToLowerInvariant() : char.ToLowerInvariant(fieldName[0]) + fieldName[1..])
            : char.ToLowerInvariant(typeName[0]) + typeName[1..];
}
#pragma warning restore CA1000
