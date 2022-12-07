using FunctionalDDD.CommonValueObjects;
using FunctionalDDD.Core;

namespace Example;

public class ValidationExample
{
    [Fact]
    public void Test1()
    {

        var x = EmailAddress.Create("xavier@somewhere.com")
            .Combine(FirstName.Create("Xavier"))
            .Combine(LastName.Create("John"))
            .Combine(EmailAddress.Create("xavier@somewhereelse.com"));

        string combinedString;
        x.Bind((email, lastName, firstName, anotherEmail) => combinedString = firstName + lastName + email + anotherEmail);

        //Error	CS1929	'Result<(EmailAddress, FirstName, LastName, EmailAddress)>' does not contain a definition for 'Bind'
        //and the best extension method overload 'ResultExtensions.Bind(UnitResult, Func<UnitResult>)' requires a receiver of type 'UnitResult'
    }
}
