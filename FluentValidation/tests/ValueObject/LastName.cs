namespace FunctionalDDD.FluentValidation.Tests;
using FunctionalDDD.CommonValueObjects;

internal class LastName : RequiredString<LastName>
{
    private LastName(string value) : base(value)
    {
    }
}
