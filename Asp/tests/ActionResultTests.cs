namespace Asp.Tests;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System.Net.Http.Headers;

public class ActionResultTests
{
    [Fact]
    public void Will_return_Ok_Result()
    {
        // Arrange
        var controller = new Mock<ControllerBase> { CallBase = true }.Object;
        var result = Result.Success("Test");

        // Act
        var response = result.ToActionResult(controller);

        // Assert
        response.Result.As<OkObjectResult>().StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public void Will_return_BadRequest_Result()
    {
        // Arrange
        var controller = new Mock<ControllerBase> { CallBase = true }.Object;
        var error = Error.BadRequest("Test");
        var result = Result.Failure<string>(error);
        var expected = new ProblemDetails
        {
            Detail = "Test",
            Status = StatusCodes.Status400BadRequest
        };

        // Act
        var response = result.ToActionResult(controller);

        // Assert
        response.Result.Should().BeOfType<ObjectResult>();
        var objectResult = response.Result.As<ObjectResult>();
        objectResult.Value.Should().BeEquivalentTo(expected);
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void ToPartialOrOkActionResult_will_return_partial_status_code_when_results_are_partial_callback()
    {
        // Arrange
        var controller = new Mock<ControllerBase> { CallBase = true }.Object;
        var result = Result.Success("Test");

        // Act
        var response = result.ToActionResult(controller,
            static r => new ContentRangeHeaderValue(4, 10, 15) { Unit = "stones" },
            static r => "Replaced");

        // Assert
        var partialResult = response.Result.As<PartialObjectResult>();
        partialResult.Value.Should().Be("Replaced");
        partialResult.StatusCode.Should().Be(StatusCodes.Status206PartialContent);
        partialResult.ContentRangeHeaderValue.From.Should().Be(4);
        partialResult.ContentRangeHeaderValue.To.Should().Be(10);
        partialResult.ContentRangeHeaderValue.Length.Should().Be(15);
        partialResult.ContentRangeHeaderValue.Unit.Should().Be("stones");
    }

    [Fact]
    public void ToPartialOrOkActionResult_will_not_call_callback_functions_if_result_in_failed_state()
    {
        // Arrange
        var controller = new Mock<ControllerBase> { CallBase = true }.Object;
        var result = Result.Failure<string>(Error.NotFound("Can't find it"));

        // Act
        var response = result.ToActionResult<string, string>(controller,
            static r => throw new InvalidOperationException(),
            static r => throw new InvalidOperationException());

        // Assert
        response.Result.As<ObjectResult>().StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public void ToPartialOrOkActionResult_will_return_Okay_status_code_when_results_are_complete()
    {
        // Arrange
        var controller = new Mock<ControllerBase> { CallBase = true }.Object;
        var result = Result.Success("Test");

        // Act
        var response = result.ToActionResult(controller, 0, 9, 10);

        // Assert
        var okObjResult = response.Result.As<OkObjectResult>();
        okObjResult.Value.Should().Be("Test");
        okObjResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

}
