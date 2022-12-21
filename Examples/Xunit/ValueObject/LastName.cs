namespace Example;
using FunctionalDDD.CommonValueObjects;

internal class LastName : RequiredString<LastName>
{
    private LastName(string value) : base(value)
    {
    }
}
