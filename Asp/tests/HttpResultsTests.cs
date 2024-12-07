namespace Asp.Tests;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using FunctionalDdd;

public class HttpResultsTests
{
    [Fact]
    public void Will_return_Ok_Result()
    {
        // Arrange
        var result = Result.Success("Test");

        // Act
        var response = result.ToHttpResult();

        // Assert
        response.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.Ok<string>>();
        var okResult = response.As<Microsoft.AspNetCore.Http.HttpResults.Ok<string>>();
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be("Test");
    }

    [Fact]
    public void Will_return_BadRequest_Result()
    {
        // Arrange
        var error = Error.BadRequest("Test");
        var result = Result.Failure<string>(error);
        var expected = new ProblemDetails
        {
            Title = "Bad Request",
            Detail = "Test",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Status = StatusCodes.Status400BadRequest
        };

        // Act
        var response = result.ToHttpResult();

        // Assert
        response.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();
        var problemResult = response.As<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();
        problemResult.ContentType.Should().Be("application/problem+json");
        problemResult.ProblemDetails.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Will_return_BadRequst_for_validation_failure()
    {
        // Arrange
        ValidationError.FieldDetails field1 = new("MyField1", ["Detail 1"]);
        ValidationError.FieldDetails field2 = new("MyField2", ["Detail 2", "More Detail 2"]);
        Error errors = Error.Validation([field1, field2], "Some validation falied.", "magicInstance");
        var result = Result.Failure(errors);
        var expected = new HttpValidationProblemDetails
        {
            Title = "One or more validation errors occurred.",
            Detail = "Some validation falied.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Status = StatusCodes.Status400BadRequest,
            Instance = "magicInstance",
            Errors = new Dictionary<string, string[]>
            {
                ["MyField1"] = ["Detail 1"],
                ["MyField2"] = ["Detail 2", "More Detail 2"]
            }
        };

        // Act
        var response = result.ToHttpResult();

        // Assert
        response.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();
        var problemResult = response.As<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();
        problemResult.ContentType.Should().Be("application/problem+json");
        problemResult.ProblemDetails.Should().BeOfType<HttpValidationProblemDetails>();
        HttpValidationProblemDetails actualProblemDetails = problemResult.ProblemDetails.As<HttpValidationProblemDetails>();
        actualProblemDetails.Should().BeEquivalentTo(expected);

    }

    [Fact]
    public void Will_retun_NotFound()
    {
        // Arrange
        var result = Result.Failure(Error.NotFound("User not found", "Chris"));
        var expected = new ProblemDetails
        {
            Title = "Not Found",
            Detail = "User not found",
            Instance = "Chris",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            Status = StatusCodes.Status404NotFound
        };

        // Act
        var response = result.ToHttpResult();

        // Assert
        response.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();
        var problemResult = response.As<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();
        problemResult.ContentType.Should().Be("application/problem+json");
        problemResult.ProblemDetails.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Will_retun_Conflict()
    {
        // Arrange
        var result = Result.Failure(Error.Conflict("Record has changed.", "Jon"));
        var expected = new ProblemDetails
        {
            Title = "Conflict",
            Detail = "Record has changed.",
            Instance = "Jon",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
            Status = StatusCodes.Status409Conflict
        };

        // Act
        var response = result.ToHttpResult();

        // Assert
        response.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();
        var problemResult = response.As<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();
        problemResult.ContentType.Should().Be("application/problem+json");
        problemResult.ProblemDetails.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Will_retun_Unauthorized()
    {
        // Arrange
        var result = Result.Failure(Error.Unauthorized("You do not have access.", "Donald"));
        var expected = new ProblemDetails
        {
            Title = "Unauthorized",
            Detail = "You do not have access.",
            Instance = "Donald",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
            Status = StatusCodes.Status401Unauthorized
        };

        // Act
        var response = result.ToHttpResult();

        // Assert
        response.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();
        var problemResult = response.As<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();
        problemResult.ContentType.Should().Be("application/problem+json");
        problemResult.ProblemDetails.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Will_return_Forbidden()
    {
        // Arrange
        var result = Result.Failure(Error.Forbidden("Access is forbidden.", "Alice"));
        var expected = new ProblemDetails
        {
            Title = "Forbidden",
            Detail = "Access is forbidden.",
            Instance = "Alice",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            Status = StatusCodes.Status403Forbidden
        };

        // Act
        var response = result.ToHttpResult();

        // Assert
        response.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();
        var problemResult = response.As<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();
        problemResult.ContentType.Should().Be("application/problem+json");
        problemResult.ProblemDetails.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Will_return_InternalServerError()
    {
        // Arrange
        var result = Result.Failure(Error.Unexpected("An unexpected error occurred.", "Server"));
        var expected = new ProblemDetails
        {
            Title = "An error occurred while processing your request.",
            Detail = "An unexpected error occurred.",
            Instance = "Server",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            Status = StatusCodes.Status500InternalServerError
        };

        // Act
        var response = result.ToHttpResult();

        // Assert
        response.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();
        var problemResult = response.As<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();
        problemResult.ContentType.Should().Be("application/problem+json");
        problemResult.ProblemDetails.Should().BeEquivalentTo(expected);
    }
}
