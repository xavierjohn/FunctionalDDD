namespace SampleWebApplication;
using FunctionalDDD;

public class UserId : RequiredGuid<UserId>
{
    private UserId(Guid value) : base(value)
    {
    }
}
