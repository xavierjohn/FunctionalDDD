namespace EfCoreExample.ValueObjects;

using FunctionalDdd.PrimitiveValueObjects;

/// <summary>
/// Strongly-typed Customer identifier using GUID.
/// </summary>
public partial class CustomerId : RequiredGuid<CustomerId>
{
}