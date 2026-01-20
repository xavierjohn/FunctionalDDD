namespace Asp.Tests.ModelBinding;

using System.Globalization;
using Asp.Tests.Validation;
using FunctionalDdd;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Xunit;

/// <summary>
/// Tests for <see cref="ValueObjectModelBinder"/> and <see cref="ValueObjectModelBinderProvider"/>.
/// </summary>
public class ValueObjectModelBinderTests
{
    #region ValueObjectModelBinderProvider Tests

    [Fact]
    public void Provider_ValueObjectType_ReturnsModelBinder()
    {
        // Arrange
        var provider = new ValueObjectModelBinderProvider();
        var context = CreateProviderContext(typeof(EmailAddress));

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        binder.Should().NotBeNull();
        binder.Should().BeOfType<ValueObjectModelBinder>();
    }

    [Fact]
    public void Provider_NonValueObjectType_ReturnsNull()
    {
        // Arrange
        var provider = new ValueObjectModelBinderProvider();
        var context = CreateProviderContext(typeof(string));

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        binder.Should().BeNull();
    }

    [Fact]
    public void Provider_PrimitiveType_ReturnsNull()
    {
        // Arrange
        var provider = new ValueObjectModelBinderProvider();
        var context = CreateProviderContext(typeof(int));

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        binder.Should().BeNull();
    }

    #endregion

    #region ValueObjectModelBinder Tests - Success Cases

    [Fact]
    public async Task Binder_ValidEmail_BindsSuccessfully()
    {
        // Arrange
        var binder = new ValueObjectModelBinder();
        var bindingContext = CreateBindingContext(typeof(EmailAddress), "email", "test@example.com");

        // Act
        await binder.BindModelAsync(bindingContext);

        // Assert
        bindingContext.Result.IsModelSet.Should().BeTrue("binding should succeed for valid email");
        bindingContext.Result.Model.Should().BeOfType<EmailAddress>();
        ((EmailAddress)bindingContext.Result.Model!).Value.Should().Be("test@example.com");
        // Note: ModelState may have validation errors from SetModelValue, check Result instead
    }

    [Fact]
    public async Task Binder_ValidFirstName_BindsSuccessfully()
    {
        // Arrange
        var binder = new ValueObjectModelBinder();
        var bindingContext = CreateBindingContext(typeof(TestFirstName), "firstName", "John");

        // Act
        await binder.BindModelAsync(bindingContext);

        // Assert
        bindingContext.Result.IsModelSet.Should().BeTrue();
        bindingContext.Result.Model.Should().BeOfType<TestFirstName>();
        ((TestFirstName)bindingContext.Result.Model!).Value.Should().Be("John");
    }

    #endregion

    #region ValueObjectModelBinder Tests - Failure Cases

    [Fact]
    public async Task Binder_InvalidEmail_AddsModelError()
    {
        // Arrange
        var binder = new ValueObjectModelBinder();
        var bindingContext = CreateBindingContext(typeof(EmailAddress), "email", "invalid");

        // Act
        await binder.BindModelAsync(bindingContext);

        // Assert
        bindingContext.Result.IsModelSet.Should().BeFalse();
        bindingContext.ModelState.ContainsKey("email").Should().BeTrue();
        bindingContext.ModelState["email"]!.Errors.Should().NotBeEmpty();
        bindingContext.ModelState["email"]!.Errors[0].ErrorMessage.Should().Contain("Email");
    }

    [Fact]
    public async Task Binder_EmptyValue_DoesNotBind()
    {
        // Arrange
        var binder = new ValueObjectModelBinder();
        var bindingContext = CreateBindingContext(typeof(EmailAddress), "email", "");

        // Act
        await binder.BindModelAsync(bindingContext);

        // Assert
        bindingContext.Result.IsModelSet.Should().BeFalse();
        // Empty values result in no binding, but model state may have entries from SetModelValue
    }

    [Fact]
    public async Task Binder_NoValue_DoesNotBind()
    {
        // Arrange
        var binder = new ValueObjectModelBinder();
        var bindingContext = CreateBindingContextWithNoValue(typeof(EmailAddress), "email");

        // Act
        await binder.BindModelAsync(bindingContext);

        // Assert
        bindingContext.Result.IsModelSet.Should().BeFalse();
        bindingContext.ModelState.IsValid.Should().BeTrue();
    }

    #endregion

    #region ValueObjectModelBinder Tests - Field Names

    [Fact]
    public async Task Binder_UsesModelNameForFieldName()
    {
        // Arrange
        var binder = new ValueObjectModelBinder();
        var bindingContext = CreateBindingContext(typeof(EmailAddress), "userEmail", "invalid");

        // Act
        await binder.BindModelAsync(bindingContext);

        // Assert
        bindingContext.ModelState.IsValid.Should().BeFalse();
        bindingContext.ModelState.ContainsKey("userEmail").Should().BeTrue();
        bindingContext.ModelState["userEmail"]!.Errors.Should().NotBeEmpty();
    }

    #endregion

    #region Helper Methods

    private static TestModelBinderProviderContext CreateProviderContext(Type modelType)
    {
        var metadataProvider = new EmptyModelMetadataProvider();
        var metadata = metadataProvider.GetMetadataForType(modelType);
        return new TestModelBinderProviderContext(metadata);
    }

    private static DefaultModelBindingContext CreateBindingContext(Type modelType, string modelName, string value)
    {
        var metadataProvider = new EmptyModelMetadataProvider();
        var metadata = metadataProvider.GetMetadataForType(modelType);

        var valueProvider = new QueryStringValueProvider(
            BindingSource.Query,
            new QueryCollection(new Dictionary<string, StringValues>
            {
                [modelName] = value
            }),
            CultureInfo.InvariantCulture);

        return new DefaultModelBindingContext
        {
            ModelMetadata = metadata,
            ModelName = modelName,
            ModelState = new ModelStateDictionary(),
            ValueProvider = valueProvider
        };
    }

    private static DefaultModelBindingContext CreateBindingContextWithNoValue(Type modelType, string modelName)
    {
        var metadataProvider = new EmptyModelMetadataProvider();
        var metadata = metadataProvider.GetMetadataForType(modelType);

        var valueProvider = new QueryStringValueProvider(
            BindingSource.Query,
            new QueryCollection(new Dictionary<string, StringValues>()),
            CultureInfo.InvariantCulture);

        return new DefaultModelBindingContext
        {
            ModelMetadata = metadata,
            ModelName = modelName,
            ModelState = new ModelStateDictionary(),
            ValueProvider = valueProvider
        };
    }

    #endregion

    #region Test Helper Classes

    private class TestModelBinderProviderContext : ModelBinderProviderContext
    {
        public TestModelBinderProviderContext(ModelMetadata metadata) => Metadata = metadata;

        public override BindingInfo BindingInfo => new();
        public override ModelMetadata Metadata { get; }
        public override IModelMetadataProvider MetadataProvider => new EmptyModelMetadataProvider();

        public override IModelBinder CreateBinder(ModelMetadata metadata) =>
            throw new NotImplementedException();

        public override IModelBinder CreateBinder(ModelMetadata metadata, BindingInfo bindingInfo) =>
            throw new NotImplementedException();
    }

    #endregion
}

/// <summary>
/// Tests for <see cref="ValueObjectModelBindingExtensions"/>.
/// </summary>
public class ValueObjectModelBindingExtensionsTests
{
    [Fact]
    public void AddValueObjectModelBinding_RegistersModelBinderProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddControllers();

        // Act
        services.AddValueObjectModelBinding();
        var provider = services.BuildServiceProvider();

        // Assert - MVC options should have the model binder provider
        var mvcOptions = provider.GetService<Microsoft.Extensions.Options.IOptions<MvcOptions>>();
        mvcOptions.Should().NotBeNull();
        mvcOptions!.Value.ModelBinderProviders.Should().Contain(p => p is ValueObjectModelBinderProvider);
    }

    [Fact]
    public void AddValueObjectModelBinding_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddValueObjectModelBinding();

        // Assert
        result.Should().BeSameAs(services);
    }
}
