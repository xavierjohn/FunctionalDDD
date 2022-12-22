namespace Example;
using FunctionalDDD;
internal class FirstName : RequiredString<FirstName>
{
    private FirstName(string value) : base(value)
    {
    }
}
