namespace EfCoreExample.EnumValueObjects;

using System.Text.Json.Serialization;
using FunctionalDdd;

/// <summary>
/// Enum value object demonstrating rich domain behavior for order state.
/// Unlike a regular C# enum, this encapsulates business rules about state transitions.
/// Name is auto-derived from the field name (pure DDD - no strings in domain).
/// </summary>
[JsonConverter(typeof(EnumValueObjectJsonConverter<OrderState>))]
public class OrderState : EnumValueObject<OrderState>
{
    // Pure domain - Name auto-derived from field name
    public static readonly OrderState Draft = new(canModify: true, canCancel: true, isTerminal: false);
    public static readonly OrderState Confirmed = new(canModify: false, canCancel: true, isTerminal: false);
    public static readonly OrderState Shipped = new(canModify: false, canCancel: false, isTerminal: false);
    public static readonly OrderState Delivered = new(canModify: false, canCancel: false, isTerminal: true);
    public static readonly OrderState Cancelled = new(canModify: false, canCancel: false, isTerminal: true);

    public bool CanModify { get; }
    public bool CanCancel { get; }
    public bool IsTerminal { get; }

    private OrderState(bool canModify, bool canCancel, bool isTerminal)
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
        _ => []
    };

    public bool CanTransitionTo(OrderState newState) => AllowedTransitions.Contains(newState);

    public Result<OrderState> TryTransitionTo(OrderState newState)
    {
        if (CanTransitionTo(newState))
            return newState;

        return Error.Validation(
            $"Cannot transition from '{Name}' to '{newState.Name}'. Allowed transitions: {string.Join(", ", AllowedTransitions.Select(s => s.Name))}",
            "state");
    }
}