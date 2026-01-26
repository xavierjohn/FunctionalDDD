namespace EfCoreExample.ValueObjects;

using FunctionalDdd.PrimitiveValueObjects;

/// <summary>
/// Strongly-typed Customer name that cannot be null, empty, or whitespace.
/// </summary>
public partial class CustomerName : RequiredString<CustomerName>
{
}