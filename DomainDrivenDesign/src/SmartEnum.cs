namespace FunctionalDdd;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

/// <summary>
/// Base class for creating strongly-typed, behavior-rich enumeration value objects.
/// Smart enums are a DDD pattern that replaces C# enums with full-featured classes that can have behavior.
/// </summary>
/// <typeparam name="TSelf">The derived smart enum type itself (CRTP pattern).</typeparam>
/// <remarks>
/// <para>
/// Smart enums address limitations of C# enums:
/// <list type="bullet">
/// <item><strong>Behavior</strong>: Each value can have associated behavior and properties</item>
/// <item><strong>Type safety</strong>: Invalid values are impossible (no <c>(OrderStatus)999</c>)</item>
/// <item><strong>Extensibility</strong>: Add methods, computed properties, and domain logic</item>
/// <item><strong>State machines</strong>: Model valid transitions between states</item>
/// <item><strong>Display names</strong>: Rich formatting without attributes</item>
/// </list>
/// </para>
/// <para>
/// Each smart enum member is defined as a static readonly field:
/// <list type="bullet">
/// <item>Members are discovered via reflection and cached for performance</item>
/// <item>The <see cref="Value"/> property provides a stable integer for persistence</item>
/// <item>The <see cref="Name"/> property provides a human-readable identifier</item>
/// </list>
/// </para>
/// <para>
/// Factory methods provide Railway-Oriented Programming support:
/// <list type="bullet">
/// <item><see cref="TryFromValue(int, string?)"/> - Creates from integer value with validation</item>
/// <item><see cref="TryFromName(string?, string?)"/> - Creates from string name with validation</item>
/// <item><see cref="FromValue(int)"/> - Creates from integer, throws on failure</item>
/// <item><see cref="FromName(string)"/> - Creates from string, throws on failure</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Basic smart enum definition:
/// <code><![CDATA[
/// public class OrderStatus : SmartEnum<OrderStatus>
/// {
///     public static readonly OrderStatus Pending = new(1, "Pending");
///     public static readonly OrderStatus Processing = new(2, "Processing");
///     public static readonly OrderStatus Shipped = new(3, "Shipped");
///     public static readonly OrderStatus Delivered = new(4, "Delivered");
///     public static readonly OrderStatus Cancelled = new(5, "Cancelled");
///     
///     private OrderStatus(int value, string name) : base(value, name) { }
/// }
/// 
/// // Usage
/// var status = OrderStatus.Pending;
/// var allStatuses = OrderStatus.GetAll();
/// var fromValue = OrderStatus.TryFromValue(1);  // Result<OrderStatus>
/// var fromName = OrderStatus.TryFromName("Pending");  // Result<OrderStatus>
/// ]]></code>
/// </example>
/// <example>
/// Smart enum with behavior:
/// <code><![CDATA[
/// public class OrderStatus : SmartEnum<OrderStatus>
/// {
///     public static readonly OrderStatus Pending = new(1, "Pending", canCancel: true);
///     public static readonly OrderStatus Processing = new(2, "Processing", canCancel: true);
///     public static readonly OrderStatus Shipped = new(3, "Shipped", canCancel: false);
///     public static readonly OrderStatus Delivered = new(4, "Delivered", canCancel: false);
///     public static readonly OrderStatus Cancelled = new(5, "Cancelled", canCancel: false);
///     
///     public bool CanCancel { get; }
///     
///     private OrderStatus(int value, string name, bool canCancel) : base(value, name)
///     {
///         CanCancel = canCancel;
///     }
///     
///     public bool CanTransitionTo(OrderStatus newStatus) => (this, newStatus) switch
///     {
///         (_, _) when this == newStatus => false,
///         ({ } s, _) when s == Cancelled => false,
///         ({ } s, _) when s == Delivered => false,
///         (_, { } n) when n == Pending => false,
///         _ => true
///     };
/// }
/// 
/// // Usage
/// if (order.Status.CanCancel)
///     order.Cancel();
/// 
/// if (order.Status.CanTransitionTo(OrderStatus.Shipped))
///     order.Ship();
/// ]]></code>
/// </example>
/// <example>
/// Smart enum with polymorphic behavior:
/// <code><![CDATA[
/// public abstract class PaymentMethod : SmartEnum<PaymentMethod>
/// {
///     public static readonly PaymentMethod CreditCard = new CreditCardPayment();
///     public static readonly PaymentMethod BankTransfer = new BankTransferPayment();
///     public static readonly PaymentMethod Crypto = new CryptoPayment();
///     
///     private PaymentMethod(int value, string name) : base(value, name) { }
///     
///     public abstract decimal CalculateFee(decimal amount);
///     public abstract TimeSpan EstimatedProcessingTime { get; }
///     
///     private sealed class CreditCardPayment : PaymentMethod
///     {
///         public CreditCardPayment() : base(1, "CreditCard") { }
///         public override decimal CalculateFee(decimal amount) => amount * 0.029m + 0.30m;
///         public override TimeSpan EstimatedProcessingTime => TimeSpan.FromSeconds(5);
///     }
///     
///     private sealed class BankTransferPayment : PaymentMethod
///     {
///         public BankTransferPayment() : base(2, "BankTransfer") { }
///         public override decimal CalculateFee(decimal amount) => 0.50m;
///         public override TimeSpan EstimatedProcessingTime => TimeSpan.FromDays(3);
///     }
///     
///     private sealed class CryptoPayment : PaymentMethod
///     {
///         public CryptoPayment() : base(3, "Crypto") { }
///         public override decimal CalculateFee(decimal amount) => amount * 0.01m;
///         public override TimeSpan EstimatedProcessingTime => TimeSpan.FromMinutes(30);
///     }
/// }
/// ]]></code>
/// </example>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix - SmartEnum is a well-known pattern name
#pragma warning disable CA1000 // Do not declare static members on generic types - required for fluent factory pattern
public abstract class SmartEnum<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TSelf> : IEquatable<SmartEnum<TSelf>>, IComparable<SmartEnum<TSelf>>
    where TSelf : SmartEnum<TSelf>
{
    // Cache for discovered enum members, keyed by type
    private static readonly ConcurrentDictionary<Type, Dictionary<int, TSelf>> s_valueCache = new();
    private static readonly ConcurrentDictionary<Type, Dictionary<string, TSelf>> s_nameCache = new();

    /// <summary>
    /// Gets the integer value of this smart enum member.
    /// Use this for persistence and serialization.
    /// </summary>
    /// <value>A unique integer identifying this enum member.</value>
    public int Value { get; }

    /// <summary>
    /// Gets the name of this smart enum member.
    /// Use this for display and string serialization.
    /// </summary>
    /// <value>A unique string identifying this enum member.</value>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the smart enum with the specified value and name.
    /// </summary>
    /// <param name="value">The integer value for this enum member.</param>
    /// <param name="name">The string name for this enum member.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty or whitespace.</exception>
    protected SmartEnum(int value, string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty or whitespace.", nameof(name));

        Value = value;
        Name = name;
    }

    /// <summary>
    /// Gets all defined members of this smart enum type.
    /// </summary>
    /// <returns>A read-only collection of all enum members.</returns>
    /// <remarks>
    /// Members are discovered via reflection on first access and cached for subsequent calls.
    /// Only public static readonly fields of type <typeparamref name="TSelf"/> are included.
    /// </remarks>
    /// <example>
    /// <code>
    /// foreach (var status in OrderStatus.GetAll())
    ///     Console.WriteLine($"{status.Value}: {status.Name}");
    /// </code>
    /// </example>
    public static IReadOnlyCollection<TSelf> GetAll() => GetValueCache().Values;

    /// <summary>
    /// Attempts to find a smart enum member by its integer value.
    /// </summary>
    /// <param name="value">The integer value to search for.</param>
    /// <param name="fieldName">Optional field name for validation error messages.</param>
    /// <returns>
    /// A <see cref="Result{TSelf}"/> containing the matching member on success,
    /// or a validation error if no member with the specified value exists.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = OrderStatus.TryFromValue(1);
    /// if (result.IsSuccess)
    ///     Console.WriteLine(result.Value.Name);
    /// </code>
    /// </example>
    public static Result<TSelf> TryFromValue(int value, string? fieldName = null)
    {
        var cache = GetValueCache();
        if (cache.TryGetValue(value, out var member))
            return member;

        var field = NormalizeFieldName(fieldName, typeof(TSelf).Name);
        var validValues = string.Join(", ", cache.Keys.OrderBy(v => v));
        return Error.Validation($"'{value}' is not a valid {typeof(TSelf).Name}. Valid values: {validValues}", field);
    }

    /// <summary>
    /// Attempts to find a smart enum member by its name (case-insensitive).
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <param name="fieldName">Optional field name for validation error messages.</param>
    /// <returns>
    /// A <see cref="Result{TSelf}"/> containing the matching member on success,
    /// or a validation error if no member with the specified name exists.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = OrderStatus.TryFromName("Pending");
    /// if (result.IsSuccess)
    ///     Console.WriteLine(result.Value.Value);
    /// </code>
    /// </example>
    public static Result<TSelf> TryFromName(string? name, string? fieldName = null)
    {
        var field = NormalizeFieldName(fieldName, typeof(TSelf).Name);

        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation($"{typeof(TSelf).Name} cannot be empty.", field);

        var cache = GetNameCache();
        if (cache.TryGetValue(name, out var member))
            return member;

        var validNames = string.Join(", ", cache.Keys.OrderBy(n => n));
        return Error.Validation($"'{name}' is not a valid {typeof(TSelf).Name}. Valid values: {validNames}", field);
    }

    /// <summary>
    /// Gets a smart enum member by its integer value. Throws if not found.
    /// Use this when the value is known to be valid.
    /// </summary>
    /// <param name="value">The integer value to search for.</param>
    /// <returns>The matching smart enum member.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no member with the specified value exists.</exception>
    /// <example>
    /// <code>
    /// var status = OrderStatus.FromValue(1);  // Returns Pending
    /// var invalid = OrderStatus.FromValue(999);  // Throws InvalidOperationException
    /// </code>
    /// </example>
    public static TSelf FromValue(int value)
    {
        var result = TryFromValue(value);
        if (result.IsFailure)
            throw new InvalidOperationException($"Failed to create {typeof(TSelf).Name}: {result.Error.Detail}");
        return result.Value;
    }

    /// <summary>
    /// Gets a smart enum member by its name (case-insensitive). Throws if not found.
    /// Use this when the name is known to be valid.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <returns>The matching smart enum member.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no member with the specified name exists.</exception>
    /// <example>
    /// <code>
    /// var status = OrderStatus.FromName("Pending");  // Returns Pending
    /// var invalid = OrderStatus.FromName("Unknown");  // Throws InvalidOperationException
    /// </code>
    /// </example>
    public static TSelf FromName(string name)
    {
        var result = TryFromName(name);
        if (result.IsFailure)
            throw new InvalidOperationException($"Failed to create {typeof(TSelf).Name}: {result.Error.Detail}");
        return result.Value;
    }

    /// <summary>
    /// Attempts to find a smart enum member by its integer value.
    /// </summary>
    /// <param name="value">The integer value to search for.</param>
    /// <param name="result">When this method returns, contains the matching member if found; otherwise, null.</param>
    /// <returns><c>true</c> if a matching member was found; otherwise, <c>false</c>.</returns>
    public static bool TryFromValue(int value, [NotNullWhen(true)] out TSelf? result)
    {
        var cache = GetValueCache();
        return cache.TryGetValue(value, out result);
    }

    /// <summary>
    /// Attempts to find a smart enum member by its name (case-insensitive).
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <param name="result">When this method returns, contains the matching member if found; otherwise, null.</param>
    /// <returns><c>true</c> if a matching member was found; otherwise, <c>false</c>.</returns>
    public static bool TryFromName(string? name, [NotNullWhen(true)] out TSelf? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var cache = GetNameCache();
        return cache.TryGetValue(name, out result);
    }

    /// <inheritdoc />
    public override string ToString() => Name;

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is SmartEnum<TSelf> other && Equals(other);

    /// <inheritdoc />
    public bool Equals(SmartEnum<TSelf>? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return Value == other.Value;
    }

    /// <inheritdoc />
    public int CompareTo(SmartEnum<TSelf>? other)
    {
        if (other is null)
            return 1;
        return Value.CompareTo(other.Value);
    }

    /// <summary>
    /// Determines whether two smart enum instances are equal.
    /// </summary>
    public static bool operator ==(SmartEnum<TSelf>? left, SmartEnum<TSelf>? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>
    /// Determines whether two smart enum instances are not equal.
    /// </summary>
    public static bool operator !=(SmartEnum<TSelf>? left, SmartEnum<TSelf>? right) => !(left == right);

    /// <summary>
    /// Determines whether the left smart enum is less than the right.
    /// </summary>
    public static bool operator <(SmartEnum<TSelf>? left, SmartEnum<TSelf>? right) =>
        left is null ? right is not null : left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether the left smart enum is less than or equal to the right.
    /// </summary>
    public static bool operator <=(SmartEnum<TSelf>? left, SmartEnum<TSelf>? right) =>
        left is null || left.CompareTo(right) <= 0;

    /// <summary>
    /// Determines whether the left smart enum is greater than the right.
    /// </summary>
    public static bool operator >(SmartEnum<TSelf>? left, SmartEnum<TSelf>? right) =>
        left is not null && left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether the left smart enum is greater than or equal to the right.
    /// </summary>
    public static bool operator >=(SmartEnum<TSelf>? left, SmartEnum<TSelf>? right) =>
        left is null ? right is null : left.CompareTo(right) >= 0;

    /// <summary>
    /// Implicitly converts a smart enum to its integer value.
    /// </summary>
    public static implicit operator int(SmartEnum<TSelf> smartEnum) => smartEnum.Value;

    /// <summary>
    /// Implicitly converts a smart enum to its string name.
    /// </summary>
    public static implicit operator string(SmartEnum<TSelf> smartEnum) => smartEnum.Name;

    // Gets or creates the value-to-member cache
    private static Dictionary<int, TSelf> GetValueCache() =>
        s_valueCache.GetOrAdd(typeof(TSelf), _ => DiscoverMembers().ToDictionary(m => m.Value));

    // Gets or creates the name-to-member cache (case-insensitive)
    private static Dictionary<string, TSelf> GetNameCache() =>
        s_nameCache.GetOrAdd(typeof(TSelf), _ => DiscoverMembers().ToDictionary(m => m.Name, StringComparer.OrdinalIgnoreCase));

    // Discovers all static readonly fields of type TSelf via reflection
    private static IEnumerable<TSelf> DiscoverMembers()
    {
        var fields = typeof(TSelf).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        foreach (var field in fields)
        {
            if (field.FieldType == typeof(TSelf) && field.IsInitOnly)
            {
                var value = field.GetValue(null);
                if (value is TSelf member)
                    yield return member;
            }
        }
    }

    // Normalizes the field name for error messages
    private static string NormalizeFieldName(string? fieldName, string typeName)
    {
        if (!string.IsNullOrEmpty(fieldName))
            return fieldName.Length == 1
                ? fieldName.ToLowerInvariant()
                : char.ToLowerInvariant(fieldName[0]) + fieldName[1..];

        // Convert PascalCase type name to camelCase
        return char.ToLowerInvariant(typeName[0]) + typeName[1..];
    }
}
#pragma warning restore CA1000
#pragma warning restore CA1711
