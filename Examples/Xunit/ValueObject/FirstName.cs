using FunctionalDDD.CommonValueObjects;

namespace Example;
internal class FirstName : RequiredString<FirstName>
{
    private FirstName(string value) : base(value)
    {
    }
}
