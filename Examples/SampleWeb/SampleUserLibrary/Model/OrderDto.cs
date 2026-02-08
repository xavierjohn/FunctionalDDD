namespace SampleUserLibrary;

using FunctionalDdd;
using FunctionalDdd.PrimitiveValueObjects;

/// <summary>
/// DTO for updating an order, demonstrating RequiredEnum auto-validation
/// and Maybe&lt;T&gt; for optional value objects.
/// </summary>
public record UpdateOrderDto
{
    /// <summary>
    /// The new order state. Automatically validated via IScalarValue.
    /// </summary>
    public required OrderState State { get; init; }

    /// <summary>
    /// Optional agent assigned to this order (uses Maybe&lt;T&gt; — domain-level optionality).
    /// When omitted or null in JSON, deserializes as Maybe.None (no error).
    /// When present, automatically validated as a non-empty string.
    /// </summary>
    public Maybe<FirstName> AssignedTo { get; init; }

    /// <summary>
    /// Optional notes for the update.
    /// </summary>
    public string? Notes { get; init; }
}

/// <summary>
/// DTO for creating an order with multiple value objects including RequiredEnum.
/// </summary>
public record CreateOrderDto
{
    /// <summary>
    /// Customer's first name.
    /// </summary>
    public required FirstName CustomerFirstName { get; init; }

    /// <summary>
    /// Customer's last name.
    /// </summary>
    public required LastName CustomerLastName { get; init; }

    /// <summary>
    /// Customer's email.
    /// </summary>
    public required EmailAddress CustomerEmail { get; init; }

    /// <summary>
    /// Initial order state. Automatically validated.
    /// </summary>
    public required OrderState InitialState { get; init; }
}
