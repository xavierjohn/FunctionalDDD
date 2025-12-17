namespace FunctionalDdd;

/// <summary>
/// Represents a domain or business rule violation.
/// Use this for errors that occur when domain logic or business rules prevent an operation from completing.
/// Maps to HTTP 422 Unprocessable Entity.
/// </summary>
/// <remarks>
/// This is distinct from validation errors (field-level data problems) and conflicts (state-based collisions).
/// Domain errors represent semantic violations of business invariants that cannot be expressed as simple field validation.
/// </remarks>
/// <example>
/// <code>
/// // Business rule violations
/// Error.Domain("Cannot withdraw more than account balance")
/// Error.Domain("Minimum order quantity is 10 units")
/// Error.Domain("Cannot cancel order after shipment has begun")
/// Error.Domain("Discount percentage cannot exceed maximum allowed limit")
/// </code>
/// </example>
public sealed class DomainError : Error
{
    public DomainError(string detail, string code, string? instance = null) : base(detail, code, instance)
    {
    }
}
