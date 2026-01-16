namespace Asp.Tests.Validation;

using FunctionalDdd;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;
using IHttpResult = Microsoft.AspNetCore.Http.IResult;

/// <summary>
/// Tests for ValueObjectValidationEndpointFilter behavior.
/// </summary>
public class ValueObjectValidationEndpointFilterTests
{
    [Fact]
    public async Task InvokeAsync_NoErrors_CallsNext()
    {
        // Arrange
        var filter = new ValueObjectValidationEndpointFilter();
        var nextCalled = false;
        using var scope = ValidationErrorsContext.BeginScope();

        var context = CreateMockContext();
        EndpointFilterDelegate next = _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>("success");
        };

        // Act
        var result = await filter.InvokeAsync(context, next);

        // Assert
        nextCalled.Should().BeTrue();
        result.Should().Be("success");
    }

    [Fact]
    public async Task InvokeAsync_WithErrors_ReturnsValidationProblem()
    {
        // Arrange
        var filter = new ValueObjectValidationEndpointFilter();
        var nextCalled = false;
        using var scope = ValidationErrorsContext.BeginScope();
        ValidationErrorsContext.AddError(ValidationError.For("email", "Email is invalid"));

        var context = CreateMockContext();
        EndpointFilterDelegate next = _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>("success");
        };

        // Act
        var result = await filter.InvokeAsync(context, next);

        // Assert
        nextCalled.Should().BeFalse();
        result.Should().BeAssignableTo<IHttpResult>();
        
        // Results.ValidationProblem returns a ProblemHttpResult
        var problemResult = result as ProblemHttpResult;
        problemResult.Should().NotBeNull();
        problemResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task InvokeAsync_WithMultipleErrors_AllErrorsInResponse()
    {
        // Arrange
        var filter = new ValueObjectValidationEndpointFilter();
        using var scope = ValidationErrorsContext.BeginScope();
        ValidationErrorsContext.AddError(ValidationError.For("firstName", "First name is required"));
        ValidationErrorsContext.AddError(ValidationError.For("lastName", "Last name is required"));

        var context = CreateMockContext();
        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>("success");

        // Act
        var result = await filter.InvokeAsync(context, next);

        // Assert
        var problemResult = result as ProblemHttpResult;
        problemResult.Should().NotBeNull();
        problemResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        
        // The ProblemDetails should contain validation errors
        var problemDetails = problemResult.ProblemDetails as HttpValidationProblemDetails;
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().HaveCount(2);
        problemDetails.Errors.Should().ContainKey("firstName");
        problemDetails.Errors.Should().ContainKey("lastName");
    }

    private static DefaultEndpointFilterInvocationContext CreateMockContext()
    {
        var httpContext = new DefaultHttpContext();
        return new DefaultEndpointFilterInvocationContext(httpContext);
    }

    // Simple implementation for testing
    private sealed class DefaultEndpointFilterInvocationContext : EndpointFilterInvocationContext
    {
        public DefaultEndpointFilterInvocationContext(HttpContext httpContext) =>
            HttpContext = httpContext;

        public override HttpContext HttpContext { get; }

        public override IList<object?> Arguments => [];

        public override T GetArgument<T>(int index) => throw new NotImplementedException();
    }
}
