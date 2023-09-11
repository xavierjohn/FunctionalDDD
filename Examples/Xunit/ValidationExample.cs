namespace Example;

using FunctionalDDD.CommonValueObjects;
using FunctionalDDD.RailwayOrientedProgramming;

public class ValidationExample
{
    [Fact]
    public void Validation_successful_Test()
    {

        var actual = EmailAddress.New("xavier@somewhere.com")
            .Combine(FirstName.New("Xavier"))
            .Combine(LastName.New("John"))
            .Combine(EmailAddress.New("xavier@somewhereelse.com"))
            .Bind((email, firstName, lastName, anotherEmail) => Result.Success(string.Join(" ", firstName, lastName, email, anotherEmail)));

        actual.Value.Should().Be("Xavier John xavier@somewhere.com xavier@somewhereelse.com");
    }

    [Fact]
    public void Validation_failed_Test()
    {

        var actual = EmailAddress.New("xavier@somewhere.com")
            .Combine(FirstName.New("Xavier"))
            .Combine(LastName.New(string.Empty))
            .Combine(EmailAddress.New("xavier @ somewhereelse.com"))
            .Bind((email, firstName, lastName, anotherEmail) =>
            {
                true.Should().BeFalse("this code should not get executed");
                return Result.Success(string.Join(" ", firstName, lastName, email, anotherEmail));
            });

        actual.IsFailure.Should().BeTrue();
        var validationErrors = (ValidationError)actual.Error;
        validationErrors.Errors.Should().HaveCount(2);
        validationErrors.Errors.Should().BeEquivalentTo(new[]
        {
            Error.ValidationError("Last Name cannot be empty.", "lastName"),
            Error.ValidationError("Email address is not valid.", "email")
        });
    }

}
