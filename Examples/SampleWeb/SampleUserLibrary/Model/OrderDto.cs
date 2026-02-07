namespace SampleUserLibrary;

using FunctionalDdd.PrimitiveValueObjects;

/// <summary>
/// DTO for updating an order, demonstrating RequiredEnum auto-validation.
/// </summary>
public record UpdateOrderDto
{
    /// <summary>
    /// The new order state. Automatically validated via IScalarValue.
    /// </summary>
    public required OrderState State { get; init; }

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
