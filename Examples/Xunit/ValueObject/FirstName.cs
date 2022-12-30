namespace Example;

using FunctionalDDD.DomainDrivenDesign;

internal class FirstName : RequiredString<FirstName>
{
    private FirstName(string value) : base(value)
    {
    }
}
