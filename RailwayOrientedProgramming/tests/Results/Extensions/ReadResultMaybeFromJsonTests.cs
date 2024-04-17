namespace RailwayOrientedProgramming.Tests.Results.Extensions;
using System.Net.Http.Json;
using System.Net;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;

public class ReadResultMaybeFromJsonTests
{
    readonly NotFoundError _notFoundError = Error.NotFound("Person not found");

    private bool _callbackCalled;

    [Fact]
    public async Task Will_read_http_content_as_result()
    {
        // Arrange
        using HttpResponseMessage httpResponseMessage = new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new camelcasePerson() { firstName = "Xavier", age = 50 }, SourceGenerationContext.Default.camelcasePerson)
        };

        // Act
        var result = await httpResponseMessage.ReadResultMaybeFromJsonAsync(SourceGenerationContext.Default.camelcasePerson, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var maybePerson = result.Value;
        maybePerson.HasValue.Should().BeTrue();
        maybePerson.Value.firstName.Should().Be("Xavier");
        maybePerson.Value.age.Should().Be(50);
    }

    [Fact]
    public async Task Will_throw_JsonException_with_wrong_content()
    {
        // Arrange
        using HttpResponseMessage httpResponseMessage = new(HttpStatusCode.OK)
        {
            Content = new StringContent("Bad JSON")
        };

        // Act
        Func<Task> act = async () => await httpResponseMessage.ReadResultMaybeFromJsonAsync(SourceGenerationContext.Default.camelcasePerson, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<JsonException>();
    }

    [Fact]
    public async Task Will_not_throw_JsonException_with_wrong_content()
    {
        // Arrange
        using HttpResponseMessage httpResponseMessage = new(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("Bad JSON")
        };

        // Act
        var result = await httpResponseMessage.ReadResultMaybeFromJsonAsync(SourceGenerationContext.Default.camelcasePerson, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Detail.Should().Be("Http Response is in a failed state for value camelcasePerson. Status code: BadGateway");
    }

    [Fact]
    public async Task Will_throw_JsonException_with_nulll_content()
    {
        // Arrange
        using HttpResponseMessage httpResponseMessage = new(HttpStatusCode.OK)
        {
            Content = null
        };

        // Act
        Func<Task> act = async () => await httpResponseMessage.ReadResultMaybeFromJsonAsync(SourceGenerationContext.Default.camelcasePerson, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<JsonException>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Deserialize_is_case_sensitive(bool propertyNameCaseInsensitive)
    {
        // Arrange
        using HttpResponseMessage httpResponseMessage = new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new camelcasePerson() { firstName = "Xavier", age = 50 }, SourceGenerationContext.Default.camelcasePerson)
        };

        // Act
        var result = await httpResponseMessage.ReadResultMaybeFromJsonAsync(
            propertyNameCaseInsensitive ? SourceGenerationCaseInsenstiveContext.Default.PascalPerson : SourceGenerationContext.Default.PascalPerson,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var maybePerson = result.Value;
        maybePerson.HasValue.Should().BeTrue();
        if (propertyNameCaseInsensitive)
        {
            maybePerson.Value.FirstName.Should().Be("Xavier");
            maybePerson.Value.Age.Should().Be(50);
        }
        else
        {
            maybePerson.Value.FirstName.Should().Be(string.Empty);
            maybePerson.Value.Age.Should().Be(0);
        }
    }

    [Fact]
    public async Task When_HttpResponseMessage_is_Task_Will_read_http_content_as_result()
    {
        // Arrange
        using HttpResponseMessage httpResponseMessage = new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new camelcasePerson() { firstName = "Xavier", age = 50 }, SourceGenerationContext.Default.camelcasePerson)
        };
        var taskHttpResponseMessage = Task.FromResult(httpResponseMessage);

        // Act
        var result = await taskHttpResponseMessage.ReadResultMaybeFromJsonAsync(SourceGenerationContext.Default.camelcasePerson, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var maybePerson = result.Value;
        maybePerson.Value.firstName.Should().Be("Xavier");
        maybePerson.Value.age.Should().Be(50);
    }

    [Fact]
    public async Task Will_not_throw_exception_for_null_JSON()
    {
        // Arrange
        using HttpResponseMessage httpResponseMessage = new(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        // Act
        var result = await httpResponseMessage.ReadResultMaybeFromJsonAsync(SourceGenerationContext.Default.camelcasePerson, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var maybePerson = result.Value;
        maybePerson.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task Will_not_callback_on_success()
    {
        // Arrange
        using HttpResponseMessage httpResponseMessage = new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new camelcasePerson() { firstName = "Chris", age = 18 }, SourceGenerationContext.Default.camelcasePerson)
        };

        // Act
        var result = await httpResponseMessage
            .HandleFailureAsync(CallbackFailedStatusCode, "Common", CancellationToken.None)
            .ReadResultMaybeFromJsonAsync(SourceGenerationContext.Default.camelcasePerson, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _callbackCalled.Should().BeFalse();
        var maybePerson = result.Value;
        maybePerson.HasValue.Should().BeTrue();
        maybePerson.Value.firstName.Should().Be("Chris");
        maybePerson.Value.age.Should().Be(18);
    }

    [Fact]
    public async Task Will_task_not_callback_on_success()
    {
        // Arrange
        using HttpResponseMessage httpResponseMessage = new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new camelcasePerson() { firstName = "Chris", age = 18 }, SourceGenerationContext.Default.camelcasePerson)
        };
        var httpResponseMessageTask = Task.FromResult(httpResponseMessage);

        // Act
        var result = await httpResponseMessageTask
            .HandleFailureAsync(CallbackFailedStatusCode, "Common", CancellationToken.None)
            .ReadResultMaybeFromJsonAsync(SourceGenerationContext.Default.camelcasePerson, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _callbackCalled.Should().BeFalse();
        var maybePerson = result.Value;
        maybePerson.HasValue.Should().BeTrue();
        maybePerson.Value.firstName.Should().Be("Chris");
        maybePerson.Value.age.Should().Be(18);
    }

    private Task<Error> CallbackFailedStatusCode(HttpResponseMessage response, string context, CancellationToken cancellationToken)
    {
        _callbackCalled = true;
        context.Should().Be("Common");
        return Task.FromResult((Error)Error.NotFound("Bad request"));
    }

    [Fact]
    public void When_NotFound_will_return_NotFound()
    {
        // Arrange
        using HttpResponseMessage httpResponseMessage = new(HttpStatusCode.NotFound);

        // Act
        var result = httpResponseMessage.HandleNotFound(_notFoundError);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(_notFoundError);
    }
}

#pragma warning disable IDE1006 // Naming Styles
public class camelcasePerson
#pragma warning restore IDE1006 // Naming Styles
{
    public string firstName { get; set; } = string.Empty;
    public int age { get; set; }
}

public class PascalPerson
{
    public string FirstName { get; set; } = string.Empty;
    public int Age { get; set; }
}

[JsonSerializable(typeof(camelcasePerson))]
[JsonSerializable(typeof(PascalPerson))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(camelcasePerson))]
[JsonSerializable(typeof(PascalPerson))]
internal partial class SourceGenerationCaseInsenstiveContext : JsonSerializerContext
{
}