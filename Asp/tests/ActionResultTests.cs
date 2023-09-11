namespace Asp.Tests;

using FunctionalDDD.RailwayOrientedProgramming;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class ActionResultTests
{
    [Fact]
    public void Will_return_Ok_Result()
    {
        // Arrange
        var controller = new Mock<ControllerBase> { CallBase = true }.Object;
        var result = Result.Success("Test");

        // Act
        var response = result.ToOkActionResult(controller);

        // Assert
        response.Result.As<OkObjectResult>().StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Will_return_Ok_Result_Async()
    {
        // Arrange
        var controller = new Mock<ControllerBase> { CallBase = true }.Object;
        var result = Task.FromResult(Result.Success("Test"));

        // Act
        var response = await result.ToOkActionResultAsync(controller);

        // Assert
        var okObjResult = response.Result.As<OkObjectResult>();
        okObjResult.Value.Should().Be("Test");
        okObjResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public void Will_return_BadRequest_Result()
    {
        // Arrange
        var controller = new Mock<ControllerBase> { CallBase = true }.Object;
        var expected = Error.BadRequest("Test");
        var result = Result.Failure<string>(expected);

        // Act
        var response = result.ToOkActionResult(controller);

        // Assert
        var badRequest = response.Result.As<BadRequestObjectResult>();
        badRequest.Value.Should().Be(expected);
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Will_return_BadRequest_Result_Async()
    {
        // Arrange
        var controller = new Mock<ControllerBase> { CallBase = true }.Object;
        var expected = Error.BadRequest("Test");
        var result = Task.FromResult(Result.Failure<string>(expected));

        // Act
        var response = await result.ToOkActionResultAsync(controller);

        // Assert
        var badRequest = response.Result.As<BadRequestObjectResult>();
        badRequest.Value.Should().Be(expected);
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task ToPartialOrOkActionResultAsync_will_return_partial_status_code_when_results_are_partial()
    {
        // Arrange
        var controller = new Mock<ControllerBase> { CallBase = true }.Object;
        var result = Task.FromResult(Result.Success("Test"));

        // Act
        var response = await result.ToPartialOrOkActionResultAsync(controller, 4, 10, 15);

        // Assert
        var partialResult = response.Result.As<PartialObjectResult>();
        partialResult.Value.Should().Be("Test");
        partialResult.StatusCode.Should().Be(StatusCodes.Status206PartialContent);
        partialResult.ContentRangeHeaderValue.From.Should().Be(4);
        partialResult.ContentRangeHeaderValue.To.Should().Be(10);
        partialResult.ContentRangeHeaderValue.Length.Should().Be(15);
        partialResult.ContentRangeHeaderValue.Unit.Value.Should().Be("items");
    }

    [Fact]
    public async Task ToPartialOrOkActionResultAsync_will_return_Okay_status_code_when_results_are_complete()
    {
        // Arrange
        var controller = new Mock<ControllerBase> { CallBase = true }.Object;
        var result = Task.FromResult(Result.Success("Test"));

        // Act
        var response = await result.ToPartialOrOkActionResultAsync(controller, 0, 9, 10);

        // Assert
        var okObjResult = response.Result.As<OkObjectResult>();
        okObjResult.Value.Should().Be("Test");
        okObjResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
}
