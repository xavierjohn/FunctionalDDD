using System;
using System.ComponentModel;
using FluentAssertions;
using FunctionalDDD.CommonValueObjects;
using FunctionalDDD.Core;
using Newtonsoft.Json;

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

        var actual = x.Bind(x => Result.Success(string.Join(" ", x.Item2, x.Item3, x.Item1, x.Item4)));

        // var actual = x.Bind((email, firstName, lastName, anotherEmail) => Result.Success(firstName + lastName + email + anotherEmail));
        // Error CS0411  The type arguments for method 'ResultExtensions.Bind<T, K>(Result<T>, Func<T, Result<K>>)' cannot be inferred from the usage.Try specifying the type arguments explicitly.


        //var actual = x.Bind<Result<(EmailAddress, FirstName, LastName, EmailAddress)>, string>((email, firstName, lastName, anotherEmail) => Result.Success(firstName + lastName + email + anotherEmail));
        // Error CS1929  'Result<(EmailAddress, FirstName, LastName, EmailAddress)>' does not contain a definition for 'Bind' and the best extension method overload 'ResultExtensions.Bind<Result<(EmailAddress, FirstName, LastName, EmailAddress)>, string>(Result<Result<(EmailAddress, FirstName, LastName, EmailAddress)>>, Func<Result<(EmailAddress, FirstName, LastName, EmailAddress)>, Result<string>>)' requires a receiver of type 'Result<Result<(EmailAddress, FirstName, LastName, EmailAddress)>>'


        actual.Value.Should().Be("Xavier John xavier@somewhere.com xavier@somewhereelse.com");
    }
}
