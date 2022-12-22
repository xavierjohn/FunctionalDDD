namespace Example;
using FunctionalDDD;

internal class LastName : RequiredString<LastName>
{
    private LastName(string value) : base(value)
    {
    }
}
