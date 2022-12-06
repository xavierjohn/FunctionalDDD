using FunctionalDDD.CommonValueObjects;

namespace Example;
internal class LastName : RequiredString<LastName>
{
    private LastName(string value) : base(value)
    {
    }
}
