namespace RailwayOrientedProgramming.Tests;

using FunctionalDDD.RailwayOrientedProgramming;
using Xunit;

public class ErrorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("abc")]
    public void Create_conflict_error(string? target)
    {
        // Arrange
        // Act
        var error = Error.Conflict("message", "code", target);

        // Assert
        error.Message.Should().Be("message");
        error.Code.Should().Be("code");
        error.Should().BeOfType<ConflictError>();
        error.Target.Should().Be(target);

    }

    [Theory]
    [InlineData(null)]
    [InlineData("abc")]
    public void Create_not_found_error(string? target)
    {
        // Arrange
        // Act
        var error = Error.NotFound("message", "code", target);

        // Assert
        error.Message.Should().Be("message");
        error.Code.Should().Be("code");
        error.Should().BeOfType<NotFoundError>();
        error.Target.Should().Be(target);
    }

    [Fact]
    public void Create_not_found_error_default()
    {
        // Arrange
        // Act
        var error = Error.NotFound("message");

        // Assert
        error.Message.Should().Be("message");
        error.Code.Should().Be("not.found.error");
        error.Should().BeOfType<NotFoundError>();
        error.Target.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("abc")]
    public void Create_Forbidden_error(string? target)
    {
        // Arrange
        // Act
        var error = Error.Forbidden("message", "code", target);

        // Assert
        error.Message.Should().Be("message");
        error.Code.Should().Be("code");
        error.Should().BeOfType<ForbiddenError>();
        error.Target.Should().Be(target);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("abc")]
    public void Create_Unauthorized_error(string? target)
    {
        // Arrange
        // Act
        var error = Error.Unauthorized("message", "code", target);

        // Assert
        error.Message.Should().Be("message");
        error.Code.Should().Be("code");
        error.Should().BeOfType<UnauthorizedError>();
        error.Target.Should().Be(target);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("abc")]
    public void Create_Unexpected_error(string? target)
    {
        // Arrange
        // Act
        var error = Error.Unexpected("message", "code", target);

        // Assert
        error.Message.Should().Be("message");
        error.Code.Should().Be("code");
        error.Should().BeOfType<UnexpectedError>();
        error.Target.Should().Be(target);
    }
}
