namespace SampleWebApplication;

using FunctionalDDD.DomainDrivenDesign;

public class FirstName : RequiredString<FirstName>
{
    private FirstName(string value) : base(value)
    {
    }
}
