namespace FunctionalDDD.FluentValidation.Tests;
using FunctionalDDD.CommonValueObjects;

internal class UserId : RequiredGuid<UserId>
{
    private UserId(Guid value) : base(value)
    {
    }

    public static UserId CreateUnique() => new(Guid.NewGuid());
}
