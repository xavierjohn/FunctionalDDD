namespace Example;

using FunctionalDDD;

public class ValidationExample
{
    [Fact]
    public void Validation_successful_Test()
    {

        var actual = EmailAddress.Create("xavier@somewhere.com")
            .Combine(FirstName.Create("Xavier"))
            .Combine(LastName.Create("John"))
            .Combine(EmailAddress.Create("xavier@somewhereelse.com"))
            .Bind((email, firstName, lastName, anotherEmail) => Result.Success(string.Join(" ", firstName, lastName, email, anotherEmail)));

        actual.Value.Should().Be("Xavier John xavier@somewhere.com xavier@somewhereelse.com");
    }

    [Fact]
    public void Validation_failed_Test()
    {

        var actual = EmailAddress.Create("xavier@somewhere.com")
            .Combine(FirstName.Create("Xavier"))
            .Combine(LastName.Create(string.Empty))
            .Combine(EmailAddress.Create("xavier @ somewhereelse.com"))
            .Bind((email, firstName, lastName, anotherEmail) =>
            {
                true.Should().BeFalse("this code should not get executed");
                return Result.Success(string.Join(" ", firstName, lastName, email, anotherEmail));
            });

        actual.IsFailure.Should().BeTrue();
        actual.Errors.Should().HaveCount(2);
        actual.Errors.Should().BeEquivalentTo(new ErrorList(
            Error.Validation("Last Name cannot be empty", "lastName"),
            Error.Validation("Email address is not valid", "email")));
    }

}
