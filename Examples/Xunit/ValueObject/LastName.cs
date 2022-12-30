namespace Example;

using FunctionalDDD.DomainDrivenDesign;

internal class LastName : RequiredString<LastName>
{
    private LastName(string value) : base(value)
    {
    }
}
