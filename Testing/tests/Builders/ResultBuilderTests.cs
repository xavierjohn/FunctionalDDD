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
    public void Failure_Should_Create_Failure_Result()
    {
        // Act
        var error = Error.BadRequest("Test error");
        var result = ResultBuilder.Failure<int>(error);

        // Assert
        result.Should()
            .BeFailureOfType<BadRequestError>()
            .Which.Should()
            .HaveDetail("Test error");
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
    public void Unauthorized_With_Default_Message_Should_Create_Unauthorized_Error()
    {
        // Act
        var result = ResultBuilder.Unauthorized<int>();

        // Assert
        result.Should()
            .BeFailureOfType<UnauthorizedError>()
            .Which.Should()
            .HaveDetail("Unauthorized");
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
    public void Forbidden_With_Default_Message_Should_Create_Forbidden_Error()
    {
        // Act
        var result = ResultBuilder.Forbidden<int>();

        // Assert
        result.Should()
            .BeFailureOfType<ForbiddenError>()
            .Which.Should()
            .HaveDetail("Forbidden");
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

    [Fact]
    public void ServiceUnavailable_Should_Create_ServiceUnavailable_Error()
    {
        // Act
        var result = ResultBuilder.ServiceUnavailable<int>("Service down");

        // Assert
        result.Should()
            .BeFailureOfType<ServiceUnavailableError>()
            .Which.Should()
            .HaveDetail("Service down");
    }

    [Fact]
    public void Unexpected_Should_Create_Unexpected_Error()
    {
        // Act
        var result = ResultBuilder.Unexpected<int>("Something went wrong");

        // Assert
        result.Should()
            .BeFailureOfType<UnexpectedError>()
            .Which.Should()
            .HaveDetail("Something went wrong");
    }

    [Fact]
    public void Domain_Should_Create_Domain_Error()
    {
        // Act
        var result = ResultBuilder.Domain<int>("Business rule violation");

        // Assert
        result.Should()
            .BeFailureOfType<DomainError>()
            .Which.Should()
            .HaveDetail("Business rule violation");
    }

    [Fact]
    public void RateLimit_Should_Create_RateLimit_Error()
    {
        // Act
        var result = ResultBuilder.RateLimit<int>("Too many requests");

        // Assert
        result.Should()
            .BeFailureOfType<RateLimitError>()
            .Which.Should()
            .HaveDetail("Too many requests");
    }

    [Fact]
    public void BadRequest_Should_Create_BadRequest_Error()
    {
        // Act
        var result = ResultBuilder.BadRequest<int>("Invalid request");

        // Assert
        result.Should()
            .BeFailureOfType<BadRequestError>()
            .Which.Should()
            .HaveDetail("Invalid request");
    }
}
