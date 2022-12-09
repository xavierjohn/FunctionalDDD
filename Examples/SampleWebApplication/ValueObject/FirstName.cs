namespace SampleWebApplication;
using FunctionalDDD.CommonValueObjects;

public class FirstName : RequiredString<FirstName>
{
    private FirstName(string value) : base(value)
    {
    }
}
