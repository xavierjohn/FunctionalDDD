namespace SampleWebApplication;

using FunctionalDDD;

public class FirstName : RequiredString<FirstName>
{
    private FirstName(string value) : base(value)
    {
    }
}
