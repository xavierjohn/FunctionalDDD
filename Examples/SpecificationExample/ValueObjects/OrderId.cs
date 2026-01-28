namespace SpecificationExample.ValueObjects;

using FunctionalDdd.PrimitiveValueObjects;

/// <summary>
/// Strongly-typed Order identifier using ULID (time-ordered, sortable).
/// </summary>
public partial class OrderId : RequiredUlid<OrderId>
{
}
