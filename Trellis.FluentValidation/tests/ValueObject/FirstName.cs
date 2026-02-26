namespace FluentValidationExt.Tests;

using Trellis;
using Trellis.PrimitiveValueObjects;

internal partial class FirstName : RequiredString<FirstName>
{
}