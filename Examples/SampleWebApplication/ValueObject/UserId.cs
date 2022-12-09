namespace SampleWebApplication;
using FunctionalDDD.CommonValueObjects;

public class UserId : RequiredGuid<UserId>
{
    private UserId(Guid value) : base(value)
    {
    }

    public static UserId CreateUnique() => new(Guid.NewGuid());
}
