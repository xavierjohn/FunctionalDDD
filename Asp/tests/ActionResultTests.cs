namespace Asp.Tests;

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
        response.Result.Should().BeOfType<OkObjectResult>();
        var okObjResult = (OkObjectResult)response.Result!;
        okObjResult.Value.Should().Be("Test");
        okObjResult.StatusCode.Should().Be(StatusCodes.Status200OK);
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
        response.Result.Should().BeOfType<OkObjectResult>();
        var okObjResult = (OkObjectResult)response.Result!;
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
        response.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = (BadRequestObjectResult)response.Result!;
        badRequest.Value.Should().Be(expected);
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
}
