namespace FunctionalDDD.FluentValidation.Tests;
using FunctionalDDD.CommonValueObjects;

internal class FirstName : RequiredString<FirstName>
{
    private FirstName(string value) : base(value)
    {
    }
}
