namespace Asp.Tests;

using FluentAssertions;
using FunctionalDdd;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using Xunit;

/// <summary>
/// Tests for the ValueObjectValidationFilter MVC action filter.
/// </summary>
public class ValueObjectValidationFilterTests
{
    private static readonly string[] ExpectedEmailErrors =
    [
        "Email is required.",
        "Email must contain @.",
        "Email domain is invalid."
    ];

    [Fact]
    public void Filter_HasCorrectOrder()
    {
        // Arrange
        var filter = new ValueObjectValidationFilter();

        // Assert
        filter.Order.Should().Be(-2000); // Should run early
    }

    [Fact]
    public void OnActionExecuting_NoValidationErrors_DoesNotShortCircuit()
    {
        // Arrange
        var filter = new ValueObjectValidationFilter();
        var context = CreateActionExecutingContext();

        using (ValidationErrorsContext.BeginScope())
        {
            // No errors added

            // Act
            filter.OnActionExecuting(context);

            // Assert
            context.Result.Should().BeNull(); // Should not short-circuit
            context.ModelState.IsValid.Should().BeTrue();
        }
    }

    [Fact]
    public void OnActionExecuting_WithValidationErrors_ShortCircuitsWithBadRequest()
    {
        // Arrange
        var filter = new ValueObjectValidationFilter();
        var context = CreateActionExecutingContext();

        using (ValidationErrorsContext.BeginScope())
        {
            ValidationErrorsContext.AddError("Email", "Email is required.");
            ValidationErrorsContext.AddError("Name", "Name cannot be empty.");

            // Act
            filter.OnActionExecuting(context);

            // Assert
            context.Result.Should().NotBeNull();
            context.Result.Should().BeOfType<BadRequestObjectResult>();

            var badRequestResult = (BadRequestObjectResult)context.Result!;
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().BeOfType<ValidationProblemDetails>();

            var problemDetails = (ValidationProblemDetails)badRequestResult.Value!;
            problemDetails.Status.Should().Be(400);
            problemDetails.Title.Should().Be("One or more validation errors occurred.");
            problemDetails.Errors.Should().HaveCount(2);
            problemDetails.Errors["Email"].Should().Contain("Email is required.");
            problemDetails.Errors["Name"].Should().Contain("Name cannot be empty.");
        }
    }

    [Fact]
    public void OnActionExecuting_SingleValidationError_AddsToModelState()
    {
        // Arrange
        var filter = new ValueObjectValidationFilter();
        var context = CreateActionExecutingContext();

        using (ValidationErrorsContext.BeginScope())
        {
            ValidationErrorsContext.AddError("Email", "Email must contain @.");

            // Act
            filter.OnActionExecuting(context);

            // Assert
            context.ModelState.IsValid.Should().BeFalse();
            context.ModelState["Email"]!.Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().Be("Email must contain @.");
        }
    }

    [Fact]
    public void OnActionExecuting_MultipleErrorsForSameField_AddsAllErrors()
    {
        // Arrange
        var filter = new ValueObjectValidationFilter();
        var context = CreateActionExecutingContext();

        using (ValidationErrorsContext.BeginScope())
        {
            ValidationErrorsContext.AddError("Email", "Email is required.");
            ValidationErrorsContext.AddError("Email", "Email must contain @.");
            ValidationErrorsContext.AddError("Email", "Email domain is invalid.");

            // Act
            filter.OnActionExecuting(context);

            // Assert
            context.ModelState.IsValid.Should().BeFalse();
            context.ModelState["Email"]!.Errors.Should().HaveCount(3);
            context.ModelState["Email"]!.Errors.Select(e => e.ErrorMessage).Should().Contain(ExpectedEmailErrors);
        }
    }

    [Fact]
    public void OnActionExecuting_ClearsExistingModelStateForValidationErrorFields()
    {
        // Arrange
        var filter = new ValueObjectValidationFilter();
        var context = CreateActionExecutingContext();

        // Add pre-existing ModelState error (e.g., from ASP.NET Core's "field is required")
        context.ModelState.AddModelError("Email", "The Email field is required.");

        using (ValidationErrorsContext.BeginScope())
        {
            // Add our custom validation error
            ValidationErrorsContext.AddError("Email", "Email must contain @.");

            // Act
            filter.OnActionExecuting(context);

            // Assert
            context.ModelState.IsValid.Should().BeFalse();
            // Should only have our custom error, not the default required error
            context.ModelState["Email"]!.Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().Be("Email must contain @.");
        }
    }

    [Fact]
    public void OnActionExecuting_HandlesCaseInsensitiveFieldNames()
    {
        // Arrange
        var filter = new ValueObjectValidationFilter();
        var context = CreateActionExecutingContext();

        // Add pre-existing error with different casing
        // Note: ModelState is case-insensitive, so "email" and "Email" refer to the same entry
        context.ModelState.AddModelError("email", "Default error");

        using (ValidationErrorsContext.BeginScope())
        {
            ValidationErrorsContext.AddError("Email", "Email is invalid.");

            // Act
            filter.OnActionExecuting(context);

            // Assert
            // ModelState is case-insensitive, so we can access with either casing
            context.ModelState.ContainsKey("email").Should().BeTrue();
            context.ModelState.ContainsKey("Email").Should().BeTrue();
            // The old error should be replaced with the new one
            context.ModelState["Email"]!.Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().Be("Email is invalid.");
        }
    }

    [Fact]
    public void OnActionExecuting_HandlesNestedPropertyNames()
    {
        // Arrange
        var filter = new ValueObjectValidationFilter();
        var context = CreateActionExecutingContext();

        // Add pre-existing error with DTO prefix
        context.ModelState.AddModelError("dto.Email", "Default error");

        using (ValidationErrorsContext.BeginScope())
        {
            ValidationErrorsContext.AddError("Email", "Email is invalid.");

            // Act
            filter.OnActionExecuting(context);

            // Assert
            // Should clear the "dto.Email" entry
            context.ModelState.Keys.Should().NotContain("dto.Email");
            context.ModelState["Email"]!.Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().Be("Email is invalid.");
        }
    }

    [Fact]
    public void OnActionExecuted_DoesNothing()
    {
        // Arrange
        var filter = new ValueObjectValidationFilter();
        var context = CreateActionExecutedContext();

        // Act
        filter.OnActionExecuted(context);

        // Assert - no exception thrown, nothing modified
        context.Result.Should().BeNull();
    }

    [Fact]
    public void OnActionExecuting_EmptyValidationErrorsContext_DoesNotShortCircuit()
    {
        // Arrange
        var filter = new ValueObjectValidationFilter();
        var context = CreateActionExecutingContext();

        using (ValidationErrorsContext.BeginScope())
        {
            // Scope exists but no errors added

            // Act
            filter.OnActionExecuting(context);

            // Assert
            context.Result.Should().BeNull();
            context.ModelState.IsValid.Should().BeTrue();
        }
    }

    [Fact]
    public void OnActionExecuting_ValidationProblemDetails_HasCorrectFormat()
    {
        // Arrange
        var filter = new ValueObjectValidationFilter();
        var context = CreateActionExecutingContext();

        using (ValidationErrorsContext.BeginScope())
        {
            ValidationErrorsContext.AddError("Field1", "Error 1");
            ValidationErrorsContext.AddError("Field2", "Error 2");

            // Act
            filter.OnActionExecuting(context);

            // Assert
            var badRequestResult = context.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();

            var problemDetails = badRequestResult!.Value as ValidationProblemDetails;
            problemDetails.Should().NotBeNull();
            problemDetails!.Title.Should().Be("One or more validation errors occurred.");
            problemDetails.Status.Should().Be(400);
            problemDetails.Errors.Should().HaveCount(2);
            problemDetails.Errors.Should().ContainKey("Field1");
            problemDetails.Errors.Should().ContainKey("Field2");
        }
    }

    #region Helper Methods

    private static ActionExecutingContext CreateActionExecutingContext()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor(),
            new ModelStateDictionary());

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller: null!);
    }

    private static ActionExecutedContext CreateActionExecutedContext()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor(),
            new ModelStateDictionary());

        return new ActionExecutedContext(
            actionContext,
            new List<IFilterMetadata>(),
            controller: null!);
    }

    #endregion
}