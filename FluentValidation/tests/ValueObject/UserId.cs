namespace FluentValidationExt.Tests;

using FunctionalDdd;
using FunctionalDdd.PrimitiveValueObjects;

internal partial class UserId : RequiredGuid<UserId>
{
}