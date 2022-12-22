namespace SampleWebApplication;
using FunctionalDDD;

public class LastName : RequiredString<LastName>
{
    private LastName(string value) : base(value)
    {
    }
}
