namespace Example;

using System.Runtime.InteropServices;
using FunctionalDDD.Domain;
using FunctionalDDD.Results;
using FunctionalDDD.Results.Errors;

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

    [Fact]
    public void Convert_optional_primitive_type_to_concrete_objects()
    {
        string email = "xavier@somewhere.com";
        string? firstName = null;
        string? lastName = "Deva";

        var actual = EmailAddress.New(email)
            .Combine(Maybe.Optional(firstName, FirstName.New))
            .Combine(Maybe.Optional(lastName, LastName.New))
            .Bind(Add);

        actual.Value.Should().Be("xavier@somewhere.com  Deva");

        static Result<string> Add(EmailAddress emailAddress, Maybe<FirstName> firstname, Maybe<LastName> lastname)
        {
            return emailAddress + " " + firstname + " " + lastname;
        }
    }
}
