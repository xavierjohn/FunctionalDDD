namespace FluentValidationExt.Tests;

using FunctionalDdd;
using FunctionalDdd.PrimitiveValueObjects;

internal partial class FirstName : RequiredString<FirstName>
{
}
