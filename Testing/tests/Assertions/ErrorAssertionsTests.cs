namespace FunctionalDdd.Testing.Tests.Assertions;

public class ErrorAssertionsTests
{
    [Fact]
    public void HaveCode_Should_Pass_When_Code_Matches()
    {
        // Arrange
        var error = Error.NotFound("Not found");

        // Act & Assert
        error.Should().HaveCode("not.found.error");
    }

    [Fact]
    public void HaveCode_Should_Fail_When_Code_Does_Not_Match()
    {
        // Arrange
        var error = Error.NotFound("Not found");

        // Act
        var act = () => error.Should().HaveCode("wrong.code");

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void HaveDetail_Should_Pass_When_Detail_Matches()
    {
        // Arrange
        var error = Error.BadRequest("Invalid input");

        // Act & Assert
        error.Should().HaveDetail("Invalid input");
    }

    [Fact]
    public void HaveDetail_Should_Fail_When_Detail_Does_Not_Match()
    {
        // Arrange
        var error = Error.BadRequest("Invalid input");

        // Act
        var act = () => error.Should().HaveDetail("Wrong detail");

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void HaveDetailContaining_Should_Pass_When_Contains_Substring()
    {
        // Arrange
        var error = Error.NotFound("User with ID 123 not found");

        // Act & Assert
        error.Should().HaveDetailContaining("123");
    }

    [Fact]
    public void HaveDetailContaining_Should_Fail_When_Does_Not_Contain()
    {
        // Arrange
        var error = Error.NotFound("User not found");

        // Act
        var act = () => error.Should().HaveDetailContaining("456");

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Should_Allow_Chaining_Assertions()
    {
        // Arrange
        var error = Error.Conflict("Resource already exists");

        // Act & Assert
        error.Should()
            .HaveCode("conflict.error")
            .And.HaveDetail("Resource already exists")
            .And.HaveDetailContaining("exists");
    }

    [Fact]
    public void HaveCode_Should_Support_Because_Reason()
    {
        // Arrange
        var error = Error.Unauthorized("Not authenticated");

        // Act & Assert
        error.Should().HaveCode("unauthorized.error", "because authentication is required");
    }

    [Fact]
    public void HaveDetail_Should_Support_Because_Reason()
    {
        // Arrange
        var error = Error.Forbidden("Access denied");

        // Act & Assert
        error.Should().HaveDetail("Access denied", "because user lacks permission");
    }

    [Fact]
    public void HaveDetailContaining_Should_Support_Because_Reason()
    {
        // Arrange
        var error = Error.Domain("Balance insufficient for withdrawal");

        // Act & Assert
        error.Should().HaveDetailContaining("insufficient", "because this is a business rule");
    }

    #region HaveInstance Tests

    [Fact]
    public void HaveInstance_Should_Pass_When_Instance_Matches()
    {
        // Arrange
        var error = Error.NotFound("Not found", "resource-123");

        // Act & Assert
        error.Should().HaveInstance("resource-123");
    }

    [Fact]
    public void HaveInstance_Should_Fail_When_Instance_Does_Not_Match()
    {
        // Arrange
        var error = Error.NotFound("Not found", "resource-123");

        // Act
        var act = () => error.Should().HaveInstance("wrong-instance");

        // Assert
        act.Should().Throw<Exception>();
    }

    #endregion

    #region BeOfType Tests

    [Fact]
    public void BeOfType_Should_Pass_When_Type_Matches()
    {
        // Arrange
        var error = Error.NotFound("Not found");

        // Act & Assert
        error.Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public void BeOfType_Should_Fail_When_Type_Does_Not_Match()
    {
        // Arrange
        var error = Error.NotFound("Not found");

        // Act
        var act = () => error.Should().BeOfType<ValidationError>();

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void BeOfType_Should_Return_Typed_Error_For_Chaining()
    {
        // Arrange
        var error = Error.Validation("Invalid email", "email");

        // Act & Assert
        error.Should()
            .BeOfType<ValidationError>()
            .Which.Should()
            .HaveFieldError("email");
    }

    #endregion

    #region Be Tests

    [Fact]
    public void Be_Should_Pass_When_Errors_Are_Equal()
    {
        // Arrange
        var error1 = Error.NotFound("Not found");
        var error2 = Error.NotFound("Different detail but same code");

        // Act & Assert (Errors are equal by Code)
        error1.Should().Be(error2);
    }

    [Fact]
    public void Be_Should_Fail_When_Errors_Are_Different()
    {
        // Arrange
        var error1 = Error.NotFound("Not found");
        var error2 = Error.BadRequest("Bad request");

        // Act
        var act = () => error1.Should().Be(error2);

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Be_Should_Support_Because_Reason()
    {
        // Arrange
        var error1 = Error.Conflict("Conflict");
        var error2 = Error.Conflict("Same conflict");

        // Act & Assert
        error1.Should().Be(error2, "because they have the same error code");
    }

    #endregion
}