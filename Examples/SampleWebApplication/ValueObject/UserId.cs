namespace SampleWebApplication;

using FunctionalDDD.DomainDrivenDesign;

public class UserId : RequiredGuid<UserId>
{
    private UserId(Guid value) : base(value)
    {
    }
}
