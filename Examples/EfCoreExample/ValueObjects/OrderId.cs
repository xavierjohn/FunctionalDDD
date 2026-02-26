namespace EfCoreExample.ValueObjects;

using Trellis.PrimitiveValueObjects;

/// <summary>
/// Strongly-typed Order identifier using GUID V7 for time-ordered, sortable IDs.
/// GUID V7s are perfect for distributed systems and provide natural chronological ordering.
/// </summary>
public partial class OrderId : RequiredGuid<OrderId>
{
}