namespace EfCoreExample.EnumValueObjects;

using System.Text.Json.Serialization;
using FunctionalDdd;

/// <summary>
/// Enum value object demonstrating rich domain behavior for order state.
/// Unlike a regular C# enum, this encapsulates business rules about state transitions.
/// </summary>
/// <remarks>
/// <para>
/// Benefits over regular enum:
/// <list type="bullet">
/// <item>Encapsulates transition rules (CanTransitionTo)</item>
/// <item>Contains behavior (CanCancel, CanModify, IsTerminal)</item>
/// <item>Type-safe - cannot have invalid values like (OrderStatus)999</item>
/// <item>JSON serialization with validation</item>
/// </list>
/// </para>
/// </remarks>
[JsonConverter(typeof(EnumValueObjectJsonConverter<OrderState>))]
public class OrderState : EnumValueObject<OrderState>
{
    // Define all possible states
    public static readonly OrderState Draft = new(1, "Draft", canModify: true, canCancel: true, isTerminal: false);
    public static readonly OrderState Confirmed = new(2, "Confirmed", canModify: false, canCancel: true, isTerminal: false);
    public static readonly OrderState Shipped = new(3, "Shipped", canModify: false, canCancel: false, isTerminal: false);
    public static readonly OrderState Delivered = new(4, "Delivered", canModify: false, canCancel: false, isTerminal: true);
    public static readonly OrderState Cancelled = new(5, "Cancelled", canModify: false, canCancel: false, isTerminal: true);

    /// <summary>
    /// Indicates whether the order can be modified (add/remove items).
    /// </summary>
    public bool CanModify { get; }

    /// <summary>
    /// Indicates whether the order can be cancelled.
    /// </summary>
    public bool CanCancel { get; }

    /// <summary>
    /// Indicates whether this is a terminal state (no further transitions allowed).
    /// </summary>
    public bool IsTerminal { get; }

    private OrderState(int value, string name, bool canModify, bool canCancel, bool isTerminal)
        : base(value, name)
    {
        CanModify = canModify;
        CanCancel = canCancel;
        IsTerminal = isTerminal;
    }

    /// <summary>
    /// Gets the allowed transitions from this state.
    /// </summary>
    public IReadOnlyList<OrderState> AllowedTransitions => this switch
    {
        _ when this == Draft => [Confirmed, Cancelled],
        _ when this == Confirmed => [Shipped, Cancelled],
        _ when this == Shipped => [Delivered],
        _ => [] // Terminal states have no transitions
    };

    /// <summary>
    /// Checks if the order can transition to the specified state.
    /// </summary>
    public bool CanTransitionTo(OrderState newState) =>
        AllowedTransitions.Contains(newState);

    /// <summary>
    /// Attempts to transition to the new state, returning a Result.
    /// </summary>
    public Result<OrderState> TryTransitionTo(OrderState newState)
    {
        if (CanTransitionTo(newState))
            return newState;

        return Error.Validation(
            $"Cannot transition from '{Name}' to '{newState.Name}'. Allowed transitions: {string.Join(", ", AllowedTransitions.Select(s => s.Name))}",
            "state");
    }
}
