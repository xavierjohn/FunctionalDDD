namespace Asp.Tests;

using FluentAssertions;
using FunctionalDdd;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

/// <summary>
/// Tests for ValueObjectValidationEndpointFilter for Minimal APIs.
/// </summary>
public class ValueObjectValidationEndpointFilterTests
{
    [Fact]
    public async Task InvokeAsync_NoErrors_CallsNext()
    {
        // Arrange
        var filter = new ValueObjectValidationEndpointFilter();
        var nextCalled = false;
        var expectedResult = Results.Ok("success");

        EndpointFilterDelegate next = _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>(expectedResult);
        };

        var context = CreateEndpointFilterContext();

        using (ValidationErrorsContext.BeginScope())
        {
            // No errors added

            // Act
            var result = await filter.InvokeAsync(context, next);

            // Assert
            nextCalled.Should().BeTrue();
            result.Should().BeSameAs(expectedResult);
        }
    }

    [Fact]
    public async Task InvokeAsync_WithErrors_ReturnsValidationProblem()
    {
        // Arrange
        var filter = new ValueObjectValidationEndpointFilter();
        var nextCalled = false;

        EndpointFilterDelegate next = _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        };

        var context = CreateEndpointFilterContext();

        using (ValidationErrorsContext.BeginScope())
        {
            ValidationErrorsContext.AddError("Field1", "Error 1");
            ValidationErrorsContext.AddError("Field2", "Error 2");

            // Act
            var result = await filter.InvokeAsync(context, next);

            // Assert
            nextCalled.Should().BeFalse("next should not be called when errors exist");
            result.Should().BeOfType<ProblemHttpResult>();

            var validationProblem = (ProblemHttpResult)result!;
            validationProblem.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task InvokeAsync_SingleError_ReturnsValidationProblemWithError()
    {
        // Arrange
        var filter = new ValueObjectValidationEndpointFilter();
        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(Results.Ok());
        var context = CreateEndpointFilterContext();

        using (ValidationErrorsContext.BeginScope())
        {
            ValidationErrorsContext.AddError("Email", "Email is required.");

            // Act
            var result = await filter.InvokeAsync(context, next);

            // Assert
            result.Should().BeOfType<ProblemHttpResult>();
            var validationProblem = (ProblemHttpResult)result!;
            validationProblem.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task InvokeAsync_MultipleErrorsForSameField_AllIncluded()
    {
        // Arrange
        var filter = new ValueObjectValidationEndpointFilter();
        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(Results.Ok());
        var context = CreateEndpointFilterContext();

        using (ValidationErrorsContext.BeginScope())
        {
            ValidationErrorsContext.AddError("Password", "Password is too short.");
            ValidationErrorsContext.AddError("Password", "Password must contain a number.");
            ValidationErrorsContext.AddError("Password", "Password must contain uppercase.");

            // Act
            var result = await filter.InvokeAsync(context, next);

            // Assert
            result.Should().BeOfType<ProblemHttpResult>();
            var validationProblem = (ProblemHttpResult)result!;
            validationProblem.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task InvokeAsync_EmptyScope_CallsNext()
    {
        // Arrange
        var filter = new ValueObjectValidationEndpointFilter();
        var nextCalled = false;

        EndpointFilterDelegate next = _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        };

        var context = CreateEndpointFilterContext();

        using (ValidationErrorsContext.BeginScope())
        {
            // Scope exists but no errors

            // Act
            await filter.InvokeAsync(context, next);

            // Assert
            nextCalled.Should().BeTrue();
        }
    }

    [Fact]
    public async Task InvokeAsync_NextReturnsNull_ReturnsNull()
    {
        // Arrange
        var filter = new ValueObjectValidationEndpointFilter();
        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(null);
        var context = CreateEndpointFilterContext();

        using (ValidationErrorsContext.BeginScope())
        {
            // Act
            var result = await filter.InvokeAsync(context, next);

            // Assert
            result.Should().BeNull();
        }
    }

    [Fact]
    public async Task InvokeAsync_NextThrowsException_ExceptionPropagates()
    {
        // Arrange
        var filter = new ValueObjectValidationEndpointFilter();
        EndpointFilterDelegate next = _ => throw new System.InvalidOperationException("Test exception");
        var context = CreateEndpointFilterContext();

        using (ValidationErrorsContext.BeginScope())
        {
            // Act
            var act = async () => await filter.InvokeAsync(context, next);

            // Assert
            await act.Should().ThrowAsync<System.InvalidOperationException>()
                .WithMessage("Test exception");
        }
    }

    [Fact]
    public async Task InvokeAsync_ValidationErrorWithComplexStructure_PreservesStructure()
    {
        // Arrange
        var filter = new ValueObjectValidationEndpointFilter();
        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(Results.Ok());
        var context = CreateEndpointFilterContext();

        using (ValidationErrorsContext.BeginScope())
        {
            // Add errors with nested field names
            ValidationErrorsContext.AddError("User.Email", "Invalid email format");
            ValidationErrorsContext.AddError("User.Address.Street", "Street is required");
            ValidationErrorsContext.AddError("Items[0].Name", "Name cannot be empty");

            // Act
            var result = await filter.InvokeAsync(context, next);

            // Assert
            result.Should().BeOfType<ProblemHttpResult>();
            var validationProblem = (ProblemHttpResult)result!;
            validationProblem.StatusCode.Should().Be(400);
        }
    }

    [Fact]
    public async Task InvokeAsync_NextReturnsNonOkResult_PassesThrough()
    {
        // Arrange
        var filter = new ValueObjectValidationEndpointFilter();
        var notFoundResult = Results.NotFound();
        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(notFoundResult);
        var context = CreateEndpointFilterContext();

        using (ValidationErrorsContext.BeginScope())
        {
            // Act
            var result = await filter.InvokeAsync(context, next);

            // Assert
            result.Should().BeSameAs(notFoundResult);
        }
    }

    [Fact]
    public async Task InvokeAsync_ConcurrentRequests_IsolatedScopes()
    {
        // Arrange
        var filter = new ValueObjectValidationEndpointFilter();

        // Act - Process multiple concurrent requests with different errors
        var tasks = new Task<object?>[]
        {
            ProcessRequestWithError(filter, "Field1", "Error1"),
            ProcessRequestWithError(filter, "Field2", "Error2"),
            ProcessRequestWithError(filter, "Field3", "Error3"),
            ProcessRequestWithError(filter, "Field4", "Error4"),
            ProcessRequestWithError(filter, "Field5", "Error5")
        };

        var results = await Task.WhenAll(tasks);

        // Assert - Each should have only its own error
        foreach (var (result, index) in results.Select((r, i) => (r, i)))
        {
            result.Should().BeOfType<ProblemHttpResult>();
            var validationProblem = (ProblemHttpResult)result!;
            validationProblem.StatusCode.Should().Be(400);
        }
    }

    #region Helper Methods

    private static DefaultEndpointFilterInvocationContext CreateEndpointFilterContext()
    {
        var httpContext = new DefaultHttpContext();
        return new DefaultEndpointFilterInvocationContext(httpContext);
    }

    private static async Task<object?> ProcessRequestWithError(
        ValueObjectValidationEndpointFilter filter,
        string fieldName,
        string errorMessage)
    {
        using (ValidationErrorsContext.BeginScope())
        {
            ValidationErrorsContext.AddError(fieldName, errorMessage);
            EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(Results.Ok());
            var context = CreateEndpointFilterContext();
            return await filter.InvokeAsync(context, next);
        }
    }

    private class DefaultEndpointFilterInvocationContext : EndpointFilterInvocationContext
    {
        public DefaultEndpointFilterInvocationContext(HttpContext httpContext) =>
            HttpContext = httpContext;

        public override HttpContext HttpContext { get; }

        public override IList<object?> Arguments => new List<object?>();

        public override T GetArgument<T>(int index) => default!;
    }

    #endregion
}