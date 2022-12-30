namespace SampleWebApplication;

using FunctionalDDD.DomainDrivenDesign;

public class LastName : RequiredString<LastName>
{
    private LastName(string value) : base(value)
    {
    }
}
