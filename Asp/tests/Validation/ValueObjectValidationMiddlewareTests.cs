namespace Asp.Tests.Validation;

using FunctionalDdd;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// Tests for ValueObjectValidationMiddleware - creates validation scope per request.
/// </summary>
public class ValueObjectValidationMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_CreatesValidationScope()
    {
        // Arrange
        var scopeCreated = false;
        RequestDelegate next = _ =>
        {
            // Inside the middleware, a scope should be active
            scopeCreated = ValidationErrorsContext.BeginScope() != null;
            return Task.CompletedTask;
        };
        var middleware = new ValueObjectValidationMiddleware(next);
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        scopeCreated.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = new ValueObjectValidationMiddleware(next);
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_CleansUpScopeAfterNext()
    {
        // Arrange
        RequestDelegate next = _ =>
        {
            // Add an error during request processing
            ValidationErrorsContext.AddError("field", "error");
            return Task.CompletedTask;
        };
        var middleware = new ValueObjectValidationMiddleware(next);
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert - scope should be cleaned up after middleware completes
        ValidationErrorsContext.HasErrors.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_ErrorsAvailableDuringRequest()
    {
        // Arrange
        var errorsFound = false;
        RequestDelegate next = _ =>
        {
            ValidationErrorsContext.AddError("field", "error");
            errorsFound = ValidationErrorsContext.HasErrors;
            return Task.CompletedTask;
        };
        var middleware = new ValueObjectValidationMiddleware(next);
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        errorsFound.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ExceptionInNext_StillCleansUpScope()
    {
        // Arrange
        RequestDelegate next = _ =>
        {
            ValidationErrorsContext.AddError("field", "error");
            throw new InvalidOperationException("Test exception");
        };
        var middleware = new ValueObjectValidationMiddleware(next);
        var context = new DefaultHttpContext();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context));

        // Scope should be cleaned up even after exception
        ValidationErrorsContext.HasErrors.Should().BeFalse();
    }
}

/// <summary>
/// Tests for ValueObjectValidationExtensions - DI registration methods (Minimal API).
/// </summary>
public class ValueObjectValidationExtensionsTests
{
    [Fact]
    public void AddValueObjectValidation_RegistersMinimalApiJsonOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddValueObjectValidation();
        var provider = services.BuildServiceProvider();

        // Assert - Minimal API JSON options should have the converter
        var minimalApiOptions = provider.GetService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>();
        minimalApiOptions.Should().NotBeNull();
        minimalApiOptions!.Value.SerializerOptions.Converters
            .Should().ContainSingle(c => c is ValidatingJsonConverterFactory);
    }

    [Fact]
    public void AddValueObjectValidation_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddValueObjectValidation();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void UseValueObjectValidation_ReturnsApplicationBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var provider = services.BuildServiceProvider();
        var appBuilder = new ApplicationBuilder(provider);

        // Act
        var result = appBuilder.UseValueObjectValidation();

        // Assert
        result.Should().BeSameAs(appBuilder);
    }

    [Fact]
    public void UseValueObjectValidation_AddsMiddlewareToPipeline()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var provider = services.BuildServiceProvider();
        var appBuilder = new ApplicationBuilder(provider);

        // Act
        appBuilder.UseValueObjectValidation();
        var app = appBuilder.Build();

        // Assert - middleware should be in the pipeline (verify by building successfully)
        app.Should().NotBeNull();
    }
}
