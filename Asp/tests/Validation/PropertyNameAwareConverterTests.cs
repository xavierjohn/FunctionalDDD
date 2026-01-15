namespace Asp.Tests.Validation;

using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FunctionalDdd;
using Xunit;

/// <summary>
/// Tests for property-name-aware validation flow.
/// These tests verify that validation errors correctly use the DTO property name
/// rather than the value object type name.
/// </summary>
[Collection("ValidatingConverterRegistry")]
public class PropertyNameAwareConverterTests : IDisposable
{
    public PropertyNameAwareConverterTests() =>
        ValidatingConverterRegistry.Clear();

    public void Dispose()
    {
        ValidatingConverterRegistry.Clear();
        ValidationErrorsContext.CurrentPropertyName = null;
        GC.SuppressFinalize(this);
    }

    #region ValidationErrorsContext.CurrentPropertyName Tests

    [Fact]
    public void CurrentPropertyName_DefaultValue_IsNull() =>
        ValidationErrorsContext.CurrentPropertyName.Should().BeNull();

    [Fact]
    public void CurrentPropertyName_SetValue_ReturnsValue()
    {
        // Arrange & Act
        ValidationErrorsContext.CurrentPropertyName = "testProperty";

        // Assert
        ValidationErrorsContext.CurrentPropertyName.Should().Be("testProperty");
    }

    [Fact]
    public void CurrentPropertyName_SetNull_ClearsValue()
    {
        // Arrange
        ValidationErrorsContext.CurrentPropertyName = "testProperty";

        // Act
        ValidationErrorsContext.CurrentPropertyName = null;

        // Assert
        ValidationErrorsContext.CurrentPropertyName.Should().BeNull();
    }

    [Fact]
    public async Task CurrentPropertyName_AsyncLocal_IsolatedAcrossTasks()
    {
        // Arrange
        var task1Value = "";
        var task2Value = "";

        // Act
        var task1 = Task.Run(() =>
        {
            ValidationErrorsContext.CurrentPropertyName = "task1Property";
            Thread.Sleep(50); // Ensure overlap
            task1Value = ValidationErrorsContext.CurrentPropertyName ?? "";
        });

        var task2 = Task.Run(() =>
        {
            ValidationErrorsContext.CurrentPropertyName = "task2Property";
            Thread.Sleep(50); // Ensure overlap
            task2Value = ValidationErrorsContext.CurrentPropertyName ?? "";
        });

        await Task.WhenAll(task1, task2);

        // Assert - each task should see its own value
        task1Value.Should().Be("task1Property");
        task2Value.Should().Be("task2Property");
    }

    #endregion

    #region ValidatingConverterRegistry.GetWrapperFactory Tests

    [Fact]
    public void GetWrapperFactory_UnregisteredType_ReturnsNull()
    {
        // Act
        var factory = ValidatingConverterRegistry.GetWrapperFactory(typeof(EmailAddress));

        // Assert
        factory.Should().BeNull();
    }

    [Fact]
    public void GetWrapperFactory_RegisteredClassType_ReturnsFactory()
    {
        // Arrange
        ValidatingConverterRegistry.Register<EmailAddress>();

        // Act
        var factory = ValidatingConverterRegistry.GetWrapperFactory(typeof(EmailAddress));

        // Assert
        factory.Should().NotBeNull();
    }

    [Fact]
    public void GetWrapperFactory_RegisteredStructType_ReturnsFactory()
    {
        // Arrange
        ValidatingConverterRegistry.RegisterStruct<TestStructValueObject>();

        // Act
        var factory = ValidatingConverterRegistry.GetWrapperFactory(typeof(TestStructValueObject));

        // Assert
        factory.Should().NotBeNull();
    }

    [Fact]
    public void GetWrapperFactory_NullableOfRegisteredStruct_ReturnsFactory()
    {
        // Arrange
        ValidatingConverterRegistry.RegisterStruct<TestStructValueObject>();

        // Act
        var factory = ValidatingConverterRegistry.GetWrapperFactory(typeof(TestStructValueObject?));

        // Assert
        factory.Should().NotBeNull();
    }

    [Fact]
    public void GetWrapperFactory_CreatesWorkingWrapper()
    {
        // Arrange
        ValidatingConverterRegistry.Register<EmailAddress>();
        var innerConverter = ValidatingConverterRegistry.GetConverter(typeof(EmailAddress))!;
        var factory = ValidatingConverterRegistry.GetWrapperFactory(typeof(EmailAddress))!;

        // Act
        var wrapper = factory(innerConverter, "customFieldName");

        // Assert
        wrapper.Should().NotBeNull();
        wrapper.CanConvert(typeof(EmailAddress)).Should().BeTrue();
    }

    #endregion

    #region Converter Uses CurrentPropertyName Tests

    [Fact]
    public void ValidatingJsonConverter_UsesCurrentPropertyName_ForErrors()
    {
        // Arrange
        ValidatingConverterRegistry.Register<EmailAddress>();
        var converter = ValidatingConverterRegistry.GetConverter(typeof(EmailAddress))!;
        var options = new JsonSerializerOptions();

        var invalidJson = "\"invalid\""u8;
        var reader = new Utf8JsonReader(invalidJson);
        reader.Read();

        using var scope = ValidationErrorsContext.BeginScope();
        ValidationErrorsContext.CurrentPropertyName = "myCustomField";

        // Act
        var genericConverter = (JsonConverter<EmailAddress?>)converter;
        genericConverter.Read(ref reader, typeof(EmailAddress), options);

        // Assert - error should use "myCustomField" not "emailAddress"
        var error = ValidationErrorsContext.GetValidationError();
        error.Should().NotBeNull();
        error!.FieldErrors.Should().Contain(fe => fe.FieldName == "myCustomField");
        error.FieldErrors.Should().NotContain(fe => fe.FieldName == "emailAddress");
    }

    [Fact]
    public void ValidatingJsonConverter_WithoutCurrentPropertyName_UsesTypeName()
    {
        // Arrange
        ValidatingConverterRegistry.Register<EmailAddress>();
        var converter = ValidatingConverterRegistry.GetConverter(typeof(EmailAddress))!;
        var options = new JsonSerializerOptions();

        var invalidJson = "\"invalid\""u8;
        var reader = new Utf8JsonReader(invalidJson);
        reader.Read();

        using var scope = ValidationErrorsContext.BeginScope();
        // Don't set CurrentPropertyName

        // Act
        var genericConverter = (JsonConverter<EmailAddress?>)converter;
        genericConverter.Read(ref reader, typeof(EmailAddress), options);

        // Assert - error should use type-derived name "emailAddress"
        var error = ValidationErrorsContext.GetValidationError();
        error.Should().NotBeNull();
        error!.FieldErrors.Should().Contain(fe => fe.FieldName == "emailAddress");
    }

    [Fact]
    public void ValidatingStructJsonConverter_UsesCurrentPropertyName_ForErrors()
    {
        // Arrange
        ValidatingConverterRegistry.RegisterStruct<TestStructValueObject>();
        var converter = ValidatingConverterRegistry.GetConverter(typeof(TestStructValueObject))!;
        var options = new JsonSerializerOptions();

        var invalidJson = "\"\""u8; // Empty string should fail validation
        var reader = new Utf8JsonReader(invalidJson);
        reader.Read();

        using var scope = ValidationErrorsContext.BeginScope();
        ValidationErrorsContext.CurrentPropertyName = "myStructField";

        // Act
        var genericConverter = (JsonConverter<TestStructValueObject?>)converter;
        genericConverter.Read(ref reader, typeof(TestStructValueObject), options);

        // Assert - error should use "myStructField"
        var error = ValidationErrorsContext.GetValidationError();
        error.Should().NotBeNull();
        error!.FieldErrors.Should().Contain(fe => fe.FieldName == "myStructField");
    }

    #endregion

    #region WrapperFactory Integration Tests

    [Fact]
    public void WrapperFactory_ValidValue_ReturnsValue()
    {
        // Arrange
        ValidatingConverterRegistry.Register<EmailAddress>();
        var innerConverter = ValidatingConverterRegistry.GetConverter(typeof(EmailAddress))!;
        var factory = ValidatingConverterRegistry.GetWrapperFactory(typeof(EmailAddress))!;
        var wrapper = factory(innerConverter, "email");

        var options = new JsonSerializerOptions();
        var json = "\"test@example.com\""u8;
        var reader = new Utf8JsonReader(json);
        reader.Read();

        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var genericWrapper = (JsonConverter<EmailAddress?>)wrapper;
        var result = genericWrapper.Read(ref reader, typeof(EmailAddress), options);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be("test@example.com");
        ValidationErrorsContext.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void WrapperFactory_InvalidValue_UsesPropertyName()
    {
        // Arrange
        ValidatingConverterRegistry.Register<EmailAddress>();
        var innerConverter = ValidatingConverterRegistry.GetConverter(typeof(EmailAddress))!;
        var factory = ValidatingConverterRegistry.GetWrapperFactory(typeof(EmailAddress))!;
        var wrapper = factory(innerConverter, "userEmail");

        var options = new JsonSerializerOptions();
        var json = "\"invalid\""u8;
        var reader = new Utf8JsonReader(json);
        reader.Read();

        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var genericWrapper = (JsonConverter<EmailAddress?>)wrapper;
        genericWrapper.Read(ref reader, typeof(EmailAddress), options);

        // Assert
        var error = ValidationErrorsContext.GetValidationError();
        error.Should().NotBeNull();
        error!.FieldErrors.Should().Contain(fe => fe.FieldName == "userEmail");
    }

    [Fact]
    public void WrapperFactory_NullValue_ReturnsNull()
    {
        // Arrange
        ValidatingConverterRegistry.Register<EmailAddress>();
        var innerConverter = ValidatingConverterRegistry.GetConverter(typeof(EmailAddress))!;
        var factory = ValidatingConverterRegistry.GetWrapperFactory(typeof(EmailAddress))!;
        var wrapper = factory(innerConverter, "email");

        var options = new JsonSerializerOptions();
        var json = "null"u8;
        var reader = new Utf8JsonReader(json);
        reader.Read();

        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var genericWrapper = (JsonConverter<EmailAddress?>)wrapper;
        var result = genericWrapper.Read(ref reader, typeof(EmailAddress), options);

        // Assert
        result.Should().BeNull();
        ValidationErrorsContext.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void WrapperFactory_Write_DelegatesToInnerConverter()
    {
        // Arrange
        ValidatingConverterRegistry.Register<EmailAddress>();
        var innerConverter = ValidatingConverterRegistry.GetConverter(typeof(EmailAddress))!;
        var factory = ValidatingConverterRegistry.GetWrapperFactory(typeof(EmailAddress))!;
        var wrapper = factory(innerConverter, "email");

        var options = new JsonSerializerOptions();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        var email = EmailAddress.TryCreate("test@example.com").Value;

        // Act
        var genericWrapper = (JsonConverter<EmailAddress?>)wrapper;
        genericWrapper.Write(writer, email, options);
        writer.Flush();

        // Assert
        var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        json.Should().Be("\"test@example.com\"");
    }

    #endregion

    #region End-to-End Property Name Validation Tests

    [Fact]
    public void EndToEnd_SameTypeMultipleProperties_ErrorsUseCorrectPropertyNames()
    {
        // This test simulates the scenario where the same value object type
        // is used for multiple properties in a DTO

        // Arrange
        ValidatingConverterRegistry.Register<TestFirstName>();
        var innerConverter = ValidatingConverterRegistry.GetConverter(typeof(TestFirstName))!;
        var factory = ValidatingConverterRegistry.GetWrapperFactory(typeof(TestFirstName))!;

        var wrapper1 = (JsonConverter<TestFirstName?>)factory(innerConverter, "firstName");
        var wrapper2 = (JsonConverter<TestFirstName?>)factory(innerConverter, "middleName");

        var options = new JsonSerializerOptions();
        using var scope = ValidationErrorsContext.BeginScope();

        // Act - simulate reading two empty values
        var emptyJson = "\"\""u8;

        var reader1 = new Utf8JsonReader(emptyJson);
        reader1.Read();
        wrapper1.Read(ref reader1, typeof(TestFirstName), options);

        var reader2 = new Utf8JsonReader(emptyJson);
        reader2.Read();
        wrapper2.Read(ref reader2, typeof(TestFirstName), options);

        // Assert
        var error = ValidationErrorsContext.GetValidationError();
        error.Should().NotBeNull();
        error!.FieldErrors.Should().HaveCount(2);
        error.FieldErrors.Should().Contain(fe => fe.FieldName == "firstName");
        error.FieldErrors.Should().Contain(fe => fe.FieldName == "middleName");

        // Should NOT have errors with "testFirstName" (the type-derived name)
        error.FieldErrors.Should().NotContain(fe => fe.FieldName == "testFirstName");
    }

    [Fact]
    public void EndToEnd_WrapperRestoresPreviousPropertyName()
    {
        // Arrange
        ValidatingConverterRegistry.Register<EmailAddress>();
        var innerConverter = ValidatingConverterRegistry.GetConverter(typeof(EmailAddress))!;
        var factory = ValidatingConverterRegistry.GetWrapperFactory(typeof(EmailAddress))!;
        var wrapper = (JsonConverter<EmailAddress?>)factory(innerConverter, "nestedEmail");

        var options = new JsonSerializerOptions();
        var json = "\"test@example.com\""u8;

        // Simulate nested scenario
        ValidationErrorsContext.CurrentPropertyName = "parentProperty";

        using var scope = ValidationErrorsContext.BeginScope();
        var reader = new Utf8JsonReader(json);
        reader.Read();

        // Act
        wrapper.Read(ref reader, typeof(EmailAddress), options);

        // Assert - after read, the previous property name should be restored
        ValidationErrorsContext.CurrentPropertyName.Should().Be("parentProperty");
    }

    [Fact]
    public void EndToEnd_StructType_WrapperWorksCorrectly()
    {
        // Arrange
        ValidatingConverterRegistry.RegisterStruct<TestStructValueObject>();
        var innerConverter = ValidatingConverterRegistry.GetConverter(typeof(TestStructValueObject))!;
        var factory = ValidatingConverterRegistry.GetWrapperFactory(typeof(TestStructValueObject))!;
        var wrapper = factory(innerConverter, "structField");

        var options = new JsonSerializerOptions();
        var json = "\"\""u8; // Empty should fail
        var reader = new Utf8JsonReader(json);
        reader.Read();

        using var scope = ValidationErrorsContext.BeginScope();

        // Act
        var genericWrapper = (JsonConverter<TestStructValueObject?>)wrapper;
        genericWrapper.Read(ref reader, typeof(TestStructValueObject), options);

        // Assert
        var error = ValidationErrorsContext.GetValidationError();
        error.Should().NotBeNull();
        error!.FieldErrors.Should().Contain(fe => fe.FieldName == "structField");
    }

    #endregion

    #region Parallel Request Simulation Tests

    [Fact]
    public async Task ParallelRequests_20Concurrent_EachRequestGetsIsolatedErrors()
    {
        // This test simulates 20 parallel HTTP requests, each deserializing
        // a DTO with value objects. Each request should have isolated errors
        // that don't bleed into other requests.

        // Arrange
        ValidatingConverterRegistry.Register<EmailAddress>();
        ValidatingConverterRegistry.Register<TestFirstName>();

        const int requestCount = 20;
        var results = new ConcurrentBag<(int RequestId, List<string> FieldNames, bool Success)>();

        // Act - simulate 20 parallel requests
        var tasks = Enumerable.Range(0, requestCount).Select(async requestId =>
        {
            await Task.Yield(); // Force async execution

            // Simulate middleware creating a scope (like ValueObjectValidationMiddleware)
            using var scope = ValidationErrorsContext.BeginScope();

            // Each request has unique field names
            var emailFieldName = $"email_{requestId}";
            var nameFieldName = $"name_{requestId}";

            // Simulate deserializing with property-name-aware converters
            var emailConverter = ValidatingConverterRegistry.GetConverter(typeof(EmailAddress))!;
            var emailFactory = ValidatingConverterRegistry.GetWrapperFactory(typeof(EmailAddress))!;
            var emailWrapper = (JsonConverter<EmailAddress?>)emailFactory(emailConverter, emailFieldName);

            var nameConverter = ValidatingConverterRegistry.GetConverter(typeof(TestFirstName))!;
            var nameFactory = ValidatingConverterRegistry.GetWrapperFactory(typeof(TestFirstName))!;
            var nameWrapper = (JsonConverter<TestFirstName?>)nameFactory(nameConverter, nameFieldName);

            var options = new JsonSerializerOptions();

            // Simulate invalid values - each request gets different field names
            var invalidEmailJson = "\"invalid\""u8;
            var emailReader = new Utf8JsonReader(invalidEmailJson);
            emailReader.Read();
            emailWrapper.Read(ref emailReader, typeof(EmailAddress), options);

            // Add some delay to increase chance of interleaving
            await Task.Delay(Random.Shared.Next(1, 10));

            var emptyNameJson = "\"\""u8;
            var nameReader = new Utf8JsonReader(emptyNameJson);
            nameReader.Read();
            nameWrapper.Read(ref nameReader, typeof(TestFirstName), options);

            // Collect the errors for this request
            var error = ValidationErrorsContext.GetValidationError();
            var fieldNames = error?.FieldErrors.Select(fe => fe.FieldName).ToList() ?? [];

            // Verify this request only sees its own errors
            var success = fieldNames.Count == 2 &&
                          fieldNames.Contains(emailFieldName) &&
                          fieldNames.Contains(nameFieldName) &&
                          !fieldNames.Any(fn => !fn.EndsWith($"_{requestId}", StringComparison.Ordinal));

            results.Add((requestId, fieldNames, success));
        });

        await Task.WhenAll(tasks.ToArray());

        // Assert
        results.Should().HaveCount(requestCount);

        // Every request should have succeeded (seen only its own errors)
        var failedRequests = results.Where(r => !r.Success).ToList();
        if (failedRequests.Count != 0)
        {
            var failureDetails = string.Join("\n", failedRequests.Select(r =>
                $"Request {r.RequestId}: Fields=[{string.Join(", ", r.FieldNames)}]"));
            Assert.Fail($"Some requests saw wrong field names:\n{failureDetails}");
        }

        // Each request should have exactly 2 errors with correct field names
        foreach (var result in results)
        {
            result.FieldNames.Should().HaveCount(2, $"Request {result.RequestId} should have 2 errors");
            result.FieldNames.Should().Contain($"email_{result.RequestId}");
            result.FieldNames.Should().Contain($"name_{result.RequestId}");
        }
    }

    [Fact]
    public async Task ParallelRequests_MixOfValidAndInvalid_NoErrorLeakage()
    {
        // Some requests have valid data (no errors), some have invalid data
        // Errors should not leak between requests

        // Arrange
        ValidatingConverterRegistry.Register<EmailAddress>();

        const int requestCount = 20;
        var results = new ConcurrentBag<(int RequestId, bool IsValid, int ErrorCount)>();

        // Act
        var tasks = Enumerable.Range(0, requestCount).Select(async requestId =>
        {
            await Task.Yield();

            using var scope = ValidationErrorsContext.BeginScope();

            var fieldName = $"email_{requestId}";
            var converter = ValidatingConverterRegistry.GetConverter(typeof(EmailAddress))!;
            var factory = ValidatingConverterRegistry.GetWrapperFactory(typeof(EmailAddress))!;
            var wrapper = (JsonConverter<EmailAddress?>)factory(converter, fieldName);

            var options = new JsonSerializerOptions();

            // Alternate between valid and invalid values
            var isValid = requestId % 2 == 0;
            var json = isValid ? "\"test@example.com\""u8 : "\"invalid\""u8;
            var reader = new Utf8JsonReader(json);
            reader.Read();
            wrapper.Read(ref reader, typeof(EmailAddress), options);

            await Task.Delay(Random.Shared.Next(1, 5));

            var error = ValidationErrorsContext.GetValidationError();
            var errorCount = error?.FieldErrors.Length ?? 0;

            results.Add((requestId, isValid, errorCount));
        });

        await Task.WhenAll(tasks.ToArray());

        // Assert
        results.Should().HaveCount(requestCount);

        foreach (var result in results)
        {
            if (result.IsValid)
                result.ErrorCount.Should().Be(0, $"Valid request {result.RequestId} should have no errors");
            else
                result.ErrorCount.Should().Be(1, $"Invalid request {result.RequestId} should have 1 error");
        }
    }

    [Fact]
    public async Task ParallelRequests_NestedObjects_PropertyNamesRestored()
    {
        // Simulate nested object deserialization across parallel requests
        // Property names should be correctly saved/restored per request

        // Arrange
        ValidatingConverterRegistry.Register<EmailAddress>();

        const int requestCount = 20;
        var results = new ConcurrentBag<(int RequestId, string? FinalPropertyName, bool Correct)>();

        // Act
        var tasks = Enumerable.Range(0, requestCount).Select(async requestId =>
        {
            await Task.Yield();

            using var scope = ValidationErrorsContext.BeginScope();

            // Simulate parent setting property name
            var parentProperty = $"parent_{requestId}";
            ValidationErrorsContext.CurrentPropertyName = parentProperty;

            // Simulate nested object deserialization
            var converter = ValidatingConverterRegistry.GetConverter(typeof(EmailAddress))!;
            var factory = ValidatingConverterRegistry.GetWrapperFactory(typeof(EmailAddress))!;
            var childWrapper = (JsonConverter<EmailAddress?>)factory(converter, $"child_{requestId}");

            var options = new JsonSerializerOptions();
            var json = "\"test@example.com\""u8;
            var reader = new Utf8JsonReader(json);
            reader.Read();
            childWrapper.Read(ref reader, typeof(EmailAddress), options);

            await Task.Delay(Random.Shared.Next(1, 5));

            // After nested read, parent property name should be restored
            var finalPropertyName = ValidationErrorsContext.CurrentPropertyName;
            var correct = finalPropertyName == parentProperty;

            results.Add((requestId, finalPropertyName, correct));
        });

        await Task.WhenAll(tasks.ToArray());

        // Assert
        results.Should().HaveCount(requestCount);

        var incorrectResults = results.Where(r => !r.Correct).ToList();
        if (incorrectResults.Count != 0)
        {
            var details = string.Join("\n", incorrectResults.Select(r =>
                $"Request {r.RequestId}: Expected 'parent_{r.RequestId}', got '{r.FinalPropertyName}'"));
            Assert.Fail($"Property name restoration failed:\n{details}");
        }
    }

    #endregion
}
