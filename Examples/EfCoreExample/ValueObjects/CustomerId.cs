namespace EfCoreExample.ValueObjects;

using FunctionalDdd.PrimitiveValueObjects;

/// <summary>
/// Strongly-typed Customer identifier using ULID.
/// </summary>
public partial class CustomerId : RequiredUlid<CustomerId>
{
}
