namespace SpecificationExample.ValueObjects;

using FunctionalDdd.PrimitiveValueObjects;

/// <summary>
/// Customer name (non-empty, max 100 characters).
/// </summary>
public partial class CustomerName : RequiredString<CustomerName>
{
}
