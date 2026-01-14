namespace Asp.Tests.Validation;

using FunctionalDdd;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Xunit;

/// <summary>
/// Tests for ValueObjectValidationFilter behavior.
/// </summary>
public class ValueObjectValidationFilterTests
{
    [Fact]
    public void OnActionExecuting_NoErrors_DoesNotSetResult()
    {
        // Arrange
        var filter = new ValueObjectValidationFilter();
        var context = CreateActionExecutingContext();
        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeNull();
    }

    [Fact]
    public void OnActionExecuting_WithErrors_SetsBadRequestResult()
    {
        // Arrange
        var filter = new ValueObjectValidationFilter();
        var context = CreateActionExecutingContext();
        using var scope = ValidationErrorsContext.BeginScope();
        ValidationErrorsContext.AddError(ValidationError.For("email", "Email is invalid"));

        // Act
        filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = (BadRequestObjectResult)context.Result!;
        badRequest.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Fact]
    public void OnActionExecuting_WithMultipleErrors_AllErrorsInResponse()
    {
        // Arrange
        var filter = new ValueObjectValidationFilter();
        var context = CreateActionExecutingContext();
        using var scope = ValidationErrorsContext.BeginScope();
        ValidationErrorsContext.AddError(ValidationError.For("firstName", "First name is required"));
        ValidationErrorsContext.AddError(ValidationError.For("lastName", "Last name is required"));
        ValidationErrorsContext.AddError(ValidationError.For("email", "Email is invalid"));

        // Act
        filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = (BadRequestObjectResult)context.Result!;
        var problemDetails = (ValidationProblemDetails)badRequest.Value!;
        problemDetails.Errors.Should().HaveCount(3);
        problemDetails.Errors.Should().ContainKey("firstName");
        problemDetails.Errors.Should().ContainKey("lastName");
        problemDetails.Errors.Should().ContainKey("email");
    }

    [Fact]
    public void OnActionExecuting_WithMultipleErrorsOnSameField_AllDetailsIncluded()
    {
        // Arrange
        var filter = new ValueObjectValidationFilter();
        var context = CreateActionExecutingContext();
        using var scope = ValidationErrorsContext.BeginScope();
        ValidationErrorsContext.AddError("password", "Password is required");
        ValidationErrorsContext.AddError("password", "Password must be at least 8 characters");

        // Act
        filter.OnActionExecuting(context);

        // Assert
        var badRequest = (BadRequestObjectResult)context.Result!;
        var problemDetails = (ValidationProblemDetails)badRequest.Value!;
        problemDetails.Errors["password"].Should().HaveCount(2);
    }

    [Fact]
    public void Filter_HasCorrectOrder() =>
        new ValueObjectValidationFilter().Order.Should().Be(-2000);

    private static ActionExecutingContext CreateActionExecutingContext()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object?>(),
            controller: null!);
    }
}
