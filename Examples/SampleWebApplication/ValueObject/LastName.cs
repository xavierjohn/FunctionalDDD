namespace SampleWebApplication;
using FunctionalDDD.CommonValueObjects;

public class LastName : RequiredString<LastName>
{
    private LastName(string value) : base(value)
    {
    }
}
