namespace FluentValidationExt.Tests;

using Trellis;
using Trellis.PrimitiveValueObjects;

internal partial class UserId : RequiredGuid<UserId>
{
}