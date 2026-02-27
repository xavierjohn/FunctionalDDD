using Trellis;

namespace EfCoreExample.ValueObjects;

/// <summary>
/// Strongly-typed Customer identifier using GUID.
/// </summary>
public partial class CustomerId : RequiredGuid<CustomerId>
{
}