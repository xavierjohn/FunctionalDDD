namespace EfCoreExample.ValueObjects;

using FunctionalDdd.PrimitiveValueObjects;

/// <summary>
/// Strongly-typed Order identifier using ULID for time-ordered, sortable IDs.
/// ULIDs are perfect for distributed systems and provide natural chronological ordering.
/// </summary>
public partial class OrderId : RequiredUlid<OrderId>
{
}