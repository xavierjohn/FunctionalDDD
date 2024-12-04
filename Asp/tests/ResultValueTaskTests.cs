namespace Asp.Tests;

using FunctionalDdd;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class ResultValueTaskTests
{
    [Fact]
    public async Task Will_return_Ok_Result_Async()
    {
        // Arrange
        var result = ValueTask.FromResult(Result.Success("Test"));

        // Act
        var response = await result.ToHttpResultAsync();

        // Assert
        response.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.Ok<string>>();
        var okResult = response.As<Microsoft.AspNetCore.Http.HttpResults.Ok<string>>();
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be("Test");
    }

    [Fact]
    public async Task Will_return_BadRequest_Result_Async()
    {
        // Arrange
        var error = Error.BadRequest("Test");
        var result = ValueTask.FromResult(Result.Failure<string>(error));
        var expected = new ProblemDetails
        {
            Title = "Bad Request",
            Detail = "Test",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Status = StatusCodes.Status400BadRequest
        };

        // Act
        var response = await result.ToHttpResultAsync();

        // Assert
        response.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();
        var problemResult = response.As<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();
        problemResult.ContentType.Should().Be("application/problem+json");
        problemResult.ProblemDetails.Should().BeEquivalentTo(expected);
    }
}
