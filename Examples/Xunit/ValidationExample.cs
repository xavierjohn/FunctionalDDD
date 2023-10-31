﻿namespace Example;

using FunctionalDDD.Domain;
using FunctionalDDD.Results;
using FunctionalDDD.Results.Errors;

public class ValidationExample
{
    [Fact]
    public void Validation_successful_Test()
    {

        var actual = EmailAddress.TryCreate("xavier@somewhere.com")
            .Combine(FirstName.TryCreate("Xavier"))
            .Combine(LastName.TryCreate("John"))
            .Combine(EmailAddress.TryCreate("xavier@somewhereelse.com"))
            .Bind((email, firstName, lastName, anotherEmail) => Result.Success(string.Join(" ", firstName, lastName, email, anotherEmail)));

        actual.Value.Should().Be("Xavier John xavier@somewhere.com xavier@somewhereelse.com");
    }

    [Fact]
    public void Validation_failed_Test()
    {

        var actual = EmailAddress.TryCreate("xavier@somewhere.com")
            .Combine(FirstName.TryCreate("Xavier"))
            .Combine(LastName.TryCreate(string.Empty))
            .Combine(EmailAddress.TryCreate("xavier @ somewhereelse.com"))
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
        string? firstName = null;
        string email = "xavier@somewhere.com";
        string? lastName = "Deva";

        var actual = EmailAddress.TryCreate(email)
            .Combine(Maybe.Optional(firstName, FirstName.TryCreate))
            .Combine(Maybe.Optional(lastName, LastName.TryCreate))
            .Bind(Add);

        actual.Value.Should().Be("xavier@somewhere.com  Deva");

        static Result<string> Add(EmailAddress emailAddress, Maybe<FirstName> firstname, Maybe<LastName> lastname)
        {
            return emailAddress + " " + firstname + " " + lastname;
        }
    }

    [Fact]
    public void Convert_optional_primitive_type_to_concrete_objects_failure()
    {
        string? firstName = string.Empty;
        string email = "xavier@somewhere.com";
        string? lastName = "Deva";

        var actual = EmailAddress.TryCreate(email)
            .Combine(Maybe.Optional(firstName, FirstName.TryCreate))
            .Combine(Maybe.Optional(lastName, LastName.TryCreate))
            .Bind(Add);

        actual.IsFailure.Should().BeTrue();
        actual.Error.Should().BeOfType<ValidationError>();
        var validationError = (ValidationError)actual.Error;
        validationError.Message.Should().Be("First Name cannot be empty.");

        static Result<string> Add(EmailAddress emailAddress, Maybe<FirstName> firstname, Maybe<LastName> lastname)
        {
            return emailAddress + " " + firstname + " " + lastname;
        }
    }
}
