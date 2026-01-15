namespace Asp.Tests.Validation;

using System.Text.Json;
using System.Text.Json.Serialization;
using FunctionalDdd;
using Xunit;

/// <summary>
/// Tests for ValidatingConverterRegistry - the AOT-compatible converter registration system.
/// </summary>
[Collection("ValidatingConverterRegistry")]
public class ValidatingConverterRegistryTests : IDisposable
{
    public ValidatingConverterRegistryTests() =>
        // Clear registry before each test to ensure isolation
        ValidatingConverterRegistry.Clear();

    public void Dispose()
    {
        ValidatingConverterRegistry.Clear();
        GC.SuppressFinalize(this);
    }

    #region Register<T> Tests

    [Fact]
    public void Register_ClassType_AddsConverter()
    {
        // Act
        ValidatingConverterRegistry.Register<EmailAddress>();

        // Assert
        ValidatingConverterRegistry.HasConverter(typeof(EmailAddress)).Should().BeTrue();
    }

    [Fact]
    public void Register_ClassType_ConverterIsCorrectType()
    {
        // Arrange
        ValidatingConverterRegistry.Register<EmailAddress>();

        // Act
        var converter = ValidatingConverterRegistry.GetConverter(typeof(EmailAddress));

        // Assert
        converter.Should().BeOfType<ValidatingJsonConverter<EmailAddress>>();
    }

    [Fact]
    public void Register_SameTypeTwice_DoesNotThrow()
    {
        // Act & Assert - should not throw
        ValidatingConverterRegistry.Register<EmailAddress>();
        ValidatingConverterRegistry.Register<EmailAddress>();

        ValidatingConverterRegistry.HasConverter(typeof(EmailAddress)).Should().BeTrue();
    }

    #endregion

    #region RegisterStruct<T> Tests

    [Fact]
    public void RegisterStruct_ValueType_AddsConverter()
    {
        // Act
        ValidatingConverterRegistry.RegisterStruct<TestStructValueObject>();

        // Assert
        ValidatingConverterRegistry.HasConverter(typeof(TestStructValueObject)).Should().BeTrue();
    }

    [Fact]
    public void RegisterStruct_ValueType_ConverterIsCorrectType()
    {
        // Arrange
        ValidatingConverterRegistry.RegisterStruct<TestStructValueObject>();

        // Act
        var converter = ValidatingConverterRegistry.GetConverter(typeof(TestStructValueObject));

        // Assert
        converter.Should().BeOfType<ValidatingStructJsonConverter<TestStructValueObject>>();
    }

    #endregion

    #region Register(Type, JsonConverter) Tests

    [Fact]
    public void Register_TypeAndConverter_AddsConverter()
    {
        // Arrange
        var converter = new ValidatingJsonConverter<EmailAddress>();

        // Act
        ValidatingConverterRegistry.Register(typeof(EmailAddress), converter);

        // Assert
        ValidatingConverterRegistry.HasConverter(typeof(EmailAddress)).Should().BeTrue();
        ValidatingConverterRegistry.GetConverter(typeof(EmailAddress)).Should().BeSameAs(converter);
    }

    #endregion

    #region HasConverter Tests

    [Fact]
    public void HasConverter_UnregisteredType_ReturnsFalse() =>
        ValidatingConverterRegistry.HasConverter(typeof(EmailAddress)).Should().BeFalse();

    [Fact]
    public void HasConverter_RegisteredType_ReturnsTrue()
    {
        // Arrange
        ValidatingConverterRegistry.Register<EmailAddress>();

        // Assert
        ValidatingConverterRegistry.HasConverter(typeof(EmailAddress)).Should().BeTrue();
    }

    [Fact]
    public void HasConverter_NullableOfRegisteredStruct_ReturnsTrue()
    {
        // Arrange
        ValidatingConverterRegistry.RegisterStruct<TestStructValueObject>();

        // Act & Assert - should find converter for nullable version
        ValidatingConverterRegistry.HasConverter(typeof(TestStructValueObject?)).Should().BeTrue();
    }

    #endregion

    #region GetConverter Tests

    [Fact]
    public void GetConverter_UnregisteredType_ReturnsNull() =>
        ValidatingConverterRegistry.GetConverter(typeof(EmailAddress)).Should().BeNull();

    [Fact]
    public void GetConverter_RegisteredType_ReturnsConverter()
    {
        // Arrange
        ValidatingConverterRegistry.Register<EmailAddress>();

        // Act
        var converter = ValidatingConverterRegistry.GetConverter(typeof(EmailAddress));

        // Assert
        converter.Should().NotBeNull();
        converter.Should().BeOfType<ValidatingJsonConverter<EmailAddress>>();
    }

    [Fact]
    public void GetConverter_NullableOfRegisteredStruct_ReturnsConverter()
    {
        // Arrange
        ValidatingConverterRegistry.RegisterStruct<TestStructValueObject>();

        // Act - get converter for nullable version
        var converter = ValidatingConverterRegistry.GetConverter(typeof(TestStructValueObject?));

        // Assert
        converter.Should().NotBeNull();
        converter.Should().BeOfType<ValidatingStructJsonConverter<TestStructValueObject>>();
    }

    #endregion

    #region Clear Tests

    [Fact]
    public void Clear_RemovesAllConverters()
    {
        // Arrange
        ValidatingConverterRegistry.Register<EmailAddress>();
        ValidatingConverterRegistry.HasConverter(typeof(EmailAddress)).Should().BeTrue();

        // Act
        ValidatingConverterRegistry.Clear();

        // Assert
        ValidatingConverterRegistry.HasConverter(typeof(EmailAddress)).Should().BeFalse();
    }

    #endregion

    #region Integration with ValidatingJsonConverterFactory

    [Fact]
    public void Factory_UsesRegisteredConverter()
    {
        // Arrange
        ValidatingConverterRegistry.Register<EmailAddress>();
        var factory = new ValidatingJsonConverterFactory();

        // Act
        var canConvert = factory.CanConvert(typeof(EmailAddress));

        // Assert
        canConvert.Should().BeTrue();
    }

    [Fact]
    public void Factory_CreateConverter_ReturnsRegisteredConverter()
    {
        // Arrange
        ValidatingConverterRegistry.Register<EmailAddress>();
        var factory = new ValidatingJsonConverterFactory();
        var options = new JsonSerializerOptions();

        // Act
        var converter = factory.CreateConverter(typeof(EmailAddress), options);

        // Assert
        converter.Should().NotBeNull();
        converter.Should().BeOfType<ValidatingJsonConverter<EmailAddress>>();
    }

    [Fact]
    public void Factory_WithoutRegistry_FallsBackToReflection()
    {
        // Arrange - don't register EmailAddress
        var factory = new ValidatingJsonConverterFactory();
        var options = new JsonSerializerOptions();

        // Act - factory should still work via reflection fallback
        var canConvert = factory.CanConvert(typeof(EmailAddress));
        var converter = factory.CreateConverter(typeof(EmailAddress), options);

        // Assert
        canConvert.Should().BeTrue();
        converter.Should().NotBeNull();
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public async Task Register_ConcurrentRegistrations_ThreadSafe()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act - register same type from multiple threads
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(ValidatingConverterRegistry.Register<EmailAddress>));
        }

        await Task.WhenAll(tasks);

        // Assert - should have exactly one converter
        ValidatingConverterRegistry.HasConverter(typeof(EmailAddress)).Should().BeTrue();
        ValidatingConverterRegistry.GetConverter(typeof(EmailAddress)).Should().NotBeNull();
    }

    #endregion
}
