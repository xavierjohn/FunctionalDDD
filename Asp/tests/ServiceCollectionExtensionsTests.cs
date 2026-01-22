namespace Asp.Tests;

using System.Linq;
using System.Text.Json;
using FluentAssertions;
using FunctionalDdd;
using FunctionalDdd.Asp.ModelBinding;
using FunctionalDdd.Asp.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

/// <summary>
/// Tests for service collection extension methods that configure value object validation.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    #region Test Value Objects

    public class TestName : ScalarValueObject<TestName, string>, IScalarValueObject<TestName, string>
    {
        private TestName(string value) : base(value) { }

        public static Result<TestName> TryCreate(string? value, string? fieldName = null)
        {
            var field = fieldName ?? "name";
            if (string.IsNullOrWhiteSpace(value))
                return Error.Validation("Name is required.", field);
            return new TestName(value);
        }
    }

    #endregion

    #region AddScalarValueObjectValidation Tests

    [Fact]
    public void AddScalarValueObjectValidation_RegistersMvcJsonOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddControllers()
            .AddScalarValueObjectValidation();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var mvcOptions = serviceProvider.GetRequiredService<IOptions<MvcJsonOptions>>();

        // Assert
        var hasFactory = mvcOptions.Value.JsonSerializerOptions.Converters
            .Any(c => c.GetType() == typeof(ValidatingJsonConverterFactory));
        hasFactory.Should().BeTrue();
    }

    [Fact]
    public void AddScalarValueObjectValidation_RegistersModelBinderProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddControllers()
            .AddScalarValueObjectValidation();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var mvcOptions = serviceProvider.GetRequiredService<IOptions<MvcOptions>>();

        // Assert
        var hasProvider = mvcOptions.Value.ModelBinderProviders
            .Any(p => p.GetType() == typeof(ScalarValueObjectModelBinderProvider));
        hasProvider.Should().BeTrue();

        // Should be at the start (highest priority)
        mvcOptions.Value.ModelBinderProviders.FirstOrDefault()
            .Should().BeOfType<ScalarValueObjectModelBinderProvider>();
    }

    [Fact]
    public void AddScalarValueObjectValidation_RegistersValidationFilter()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddControllers()
            .AddScalarValueObjectValidation();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var mvcOptions = serviceProvider.GetRequiredService<IOptions<MvcOptions>>();

        // Assert
        var hasFilter = mvcOptions.Value.Filters.Any(f =>
            f is TypeFilterAttribute tfa && tfa.ImplementationType == typeof(ValueObjectValidationFilter));
        hasFilter.Should().BeTrue();
    }

    [Fact]
    public void AddScalarValueObjectValidation_SuppressesModelStateInvalidFilter()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddControllers()
            .AddScalarValueObjectValidation();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var apiBehaviorOptions = serviceProvider.GetRequiredService<IOptions<ApiBehaviorOptions>>();

        // Assert
        apiBehaviorOptions.Value.SuppressModelStateInvalidFilter.Should().BeTrue();
    }

    [Fact]
    public void AddScalarValueObjectValidation_ConfiguresTypeInfoResolver()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddControllers()
            .AddScalarValueObjectValidation();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var mvcOptions = serviceProvider.GetRequiredService<IOptions<MvcJsonOptions>>();

        // Assert
        mvcOptions.Value.JsonSerializerOptions.TypeInfoResolver.Should().NotBeNull();
    }

    #endregion

    #region AddValueObjectValidation Tests

    [Fact]
    public void AddValueObjectValidation_ConfiguresMvcJsonOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddControllers();
        services.AddValueObjectValidation();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var mvcOptions = serviceProvider.GetRequiredService<IOptions<MvcJsonOptions>>();

        // Assert
        var hasFactory = mvcOptions.Value.JsonSerializerOptions.Converters
            .Any(c => c.GetType() == typeof(ValidatingJsonConverterFactory));
        hasFactory.Should().BeTrue();
    }

    [Fact]
    public void AddValueObjectValidation_ConfiguresHttpJsonOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddValueObjectValidation();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var httpOptions = serviceProvider.GetRequiredService<IOptions<HttpJsonOptions>>();

        // Assert
        var hasFactory = httpOptions.Value.SerializerOptions.Converters
            .Any(c => c.GetType() == typeof(ValidatingJsonConverterFactory));
        hasFactory.Should().BeTrue();
    }

    [Fact]
    public void AddValueObjectValidation_ConfiguresBothMvcAndMinimalApi()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddControllers();
        services.AddValueObjectValidation();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var mvcOptions = serviceProvider.GetRequiredService<IOptions<MvcJsonOptions>>();
        var httpOptions = serviceProvider.GetRequiredService<IOptions<HttpJsonOptions>>();

        // Assert
        // Both should have the factory
        var mvcHasFactory = mvcOptions.Value.JsonSerializerOptions.Converters
            .Any(c => c.GetType() == typeof(ValidatingJsonConverterFactory));
        mvcHasFactory.Should().BeTrue();

        var httpHasFactory = httpOptions.Value.SerializerOptions.Converters
            .Any(c => c.GetType() == typeof(ValidatingJsonConverterFactory));
        httpHasFactory.Should().BeTrue();

        // Both should have type info resolver
        mvcOptions.Value.JsonSerializerOptions.TypeInfoResolver.Should().NotBeNull();
        httpOptions.Value.SerializerOptions.TypeInfoResolver.Should().NotBeNull();
    }

    [Fact]
    public void AddValueObjectValidation_DoesNotAddModelBindingOrFilters()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddControllers();
        services.AddValueObjectValidation();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var mvcOptions = serviceProvider.GetRequiredService<IOptions<MvcOptions>>();

        // Assert
        // Unified method doesn't add MVC-specific features
        var hasProvider = mvcOptions.Value.ModelBinderProviders
            .Any(p => p.GetType() == typeof(ScalarValueObjectModelBinderProvider));
        hasProvider.Should().BeFalse();

        var hasFilter = mvcOptions.Value.Filters.Any(f =>
            f is TypeFilterAttribute tfa && tfa.ImplementationType == typeof(ValueObjectValidationFilter));
        hasFilter.Should().BeFalse();
    }

    #endregion

    #region AddScalarValueObjectValidationForMinimalApi Tests

    [Fact]
    public void AddScalarValueObjectValidationForMinimalApi_ConfiguresHttpJsonOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScalarValueObjectValidationForMinimalApi();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var httpOptions = serviceProvider.GetRequiredService<IOptions<HttpJsonOptions>>();

        // Assert
        var hasFactory = httpOptions.Value.SerializerOptions.Converters
            .Any(c => c.GetType() == typeof(ValidatingJsonConverterFactory));
        hasFactory.Should().BeTrue();
        httpOptions.Value.SerializerOptions.TypeInfoResolver.Should().NotBeNull();
    }

    [Fact]
    public void AddScalarValueObjectValidationForMinimalApi_DoesNotAffectMvcOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddControllers();
        services.AddScalarValueObjectValidationForMinimalApi();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var mvcOptions = serviceProvider.GetRequiredService<IOptions<MvcJsonOptions>>();

        // Assert
        // MVC options should not be affected
        var hasFactory = mvcOptions.Value.JsonSerializerOptions.Converters
            .Any(c => c.GetType() == typeof(ValidatingJsonConverterFactory));
        hasFactory.Should().BeFalse();
    }

    #endregion

    #region UseValueObjectValidation Tests

    [Fact]
    public void UseValueObjectValidation_AddsMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        var result = app.UseValueObjectValidation();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(app);
    }

    #endregion

    #region Integration Tests - JSON Deserialization

    [Fact]
    public void ConfiguredJsonOptions_DeserializeValidValueObject_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddValueObjectValidation();
        var serviceProvider = services.BuildServiceProvider();
        var httpOptions = serviceProvider.GetRequiredService<IOptions<HttpJsonOptions>>();

        var json = "\"John\"";

        using (ValidationErrorsContext.BeginScope())
        {
            ValidationErrorsContext.CurrentPropertyName = "Name";

            // Act
            var result = JsonSerializer.Deserialize<TestName>(json, httpOptions.Value.SerializerOptions);

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be("John");
            ValidationErrorsContext.GetValidationError().Should().BeNull();
        }
    }

    [Fact]
    public void ConfiguredJsonOptions_DeserializeInvalidValueObject_CollectsErrors()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddValueObjectValidation();
        var serviceProvider = services.BuildServiceProvider();
        var httpOptions = serviceProvider.GetRequiredService<IOptions<HttpJsonOptions>>();

        var json = "\"\""; // Empty string - invalid

        using (ValidationErrorsContext.BeginScope())
        {
            ValidationErrorsContext.CurrentPropertyName = "Name";

            // Act
            var result = JsonSerializer.Deserialize<TestName>(json, httpOptions.Value.SerializerOptions);

            // Assert
            result.Should().BeNull();
            var error = ValidationErrorsContext.GetValidationError();
            error.Should().NotBeNull();
            error!.FieldErrors.Should().ContainSingle()
                .Which.FieldName.Should().Be("Name");
        }
    }

    #endregion
}
