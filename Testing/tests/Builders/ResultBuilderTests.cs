namespace FunctionalDdd.Testing.Tests.Builders;

using FunctionalDdd.Testing.Builders;

public class ResultBuilderTests
{
    [Fact]
    public void Success_Should_Create_Success_Result()
    {
        // Act
        var result = ResultBuilder.Success(42);

        // Assert
        result.Should().BeSuccess()
            .Which.Should().Be(42);
    }

    [Fact]
    public void NotFound_Should_Create_NotFound_Error()
    {
        // Act
        var result = ResultBuilder.NotFound<int>("Not found");

        // Assert
        result.Should()
            .BeFailureOfType<NotFoundError>()
            .Which.Should()
            .HaveDetail("Not found");
    }

    [Fact]
    public void NotFound_With_Entity_Should_Include_Entity_In_Detail()
    {
        // Act
        var result = ResultBuilder.NotFound<int>("User", "123");

        // Assert
        result.Should()
            .BeFailureOfType<NotFoundError>()
            .Which.Should()
            .HaveDetailContaining("User")
            .And.HaveDetailContaining("123");
    }

    [Fact]
    public void Validation_Should_Create_Validation_Error()
    {
        // Act
        var result = ResultBuilder.Validation<int>("Invalid email", "email");

        // Assert
        result.Should()
            .BeFailureOfType<ValidationError>()
            .Which.Should()
            .HaveFieldError("email");
    }

    [Fact]
    public void Unauthorized_Should_Create_Unauthorized_Error()
    {
        // Act
        var result = ResultBuilder.Unauthorized<int>("Login required");

        // Assert
        result.Should()
            .BeFailureOfType<UnauthorizedError>()
            .Which.Should()
            .HaveDetail("Login required");
    }

    [Fact]
    public void Forbidden_Should_Create_Forbidden_Error()
    {
        // Act
        var result = ResultBuilder.Forbidden<int>("Access denied");

        // Assert
        result.Should()
            .BeFailureOfType<ForbiddenError>()
            .Which.Should()
            .HaveDetail("Access denied");
    }

    [Fact]
    public void Conflict_Should_Create_Conflict_Error()
    {
        // Act
        var result = ResultBuilder.Conflict<int>("Already exists");

        // Assert
        result.Should()
            .BeFailureOfType<ConflictError>()
            .Which.Should()
            .HaveDetail("Already exists");
    }
}
