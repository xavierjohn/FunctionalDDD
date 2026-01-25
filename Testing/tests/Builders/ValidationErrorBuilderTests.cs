namespace FunctionalDdd.Testing.Tests.Builders;

using FunctionalDdd.Testing.Builders;

public class ValidationErrorBuilderTests
{
    [Fact]
    public void Should_Build_ValidationError_With_Single_Field()
    {
        // Act
        var error = ValidationErrorBuilder.Create()
            .WithFieldError("email", "Email is required")
            .Build();

        // Assert
        error.Should()
            .HaveFieldCount(1)
            .And.HaveFieldErrorWithDetail("email", "Email is required");
    }

    [Fact]
    public void Should_Build_ValidationError_With_Multiple_Fields()
    {
        // Act
        var error = ValidationErrorBuilder.Create()
            .WithFieldError("email", "Email is required")
            .WithFieldError("password", "Password is required")
            .WithFieldError("age", "Must be 18 or older")
            .Build();

        // Assert
        error.Should()
            .HaveFieldCount(3)
            .And.HaveFieldError("email")
            .And.HaveFieldError("password")
            .And.HaveFieldError("age");
    }

    [Fact]
    public void Should_Build_ValidationError_With_Multiple_Details_Per_Field()
    {
        // Act
        var error = ValidationErrorBuilder.Create()
            .WithFieldError("email", "Email is required")
            .WithFieldError("email", "Invalid email format")
            .Build();

        // Assert
        error.Should()
            .HaveFieldCount(1)
            .And.HaveFieldErrorWithDetail("email", "Email is required")
            .And.HaveFieldErrorWithDetail("email", "Invalid email format");
    }

    [Fact]
    public void WithFieldError_Params_Should_Add_Multiple_Details()
    {
        // Act
        var error = ValidationErrorBuilder.Create()
            .WithFieldError("email", "Email is required", "Invalid format", "Must be unique")
            .Build();

        // Assert
        error.Should()
            .HaveFieldCount(1)
            .And.HaveFieldErrorWithDetail("email", "Email is required")
            .And.HaveFieldErrorWithDetail("email", "Invalid format")
            .And.HaveFieldErrorWithDetail("email", "Must be unique");
    }

    [Fact]
    public void BuildFailure_Should_Create_Failed_Result()
    {
        // Act
        var result = ValidationErrorBuilder.Create()
            .WithFieldError("email", "Email is required")
            .BuildFailure<int>();

        // Assert
        result.Should()
            .BeFailureOfType<ValidationError>()
            .Which.Should()
            .HaveFieldError("email");
    }

    [Fact]
    public void Build_Should_Throw_When_No_Errors_Added()
    {
        // Arrange
        var builder = ValidationErrorBuilder.Create();

        // Act
        Action act = () => builder.Build();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*At least one field error*");
    }
}