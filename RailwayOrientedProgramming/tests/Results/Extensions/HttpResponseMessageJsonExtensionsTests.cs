namespace RailwayOrientedProgramming.Tests.Results.Extensions;
using System.Net.Http.Json;
using System.Net;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;

public class HttpResponseMessageJsonExtensionsTests
{
    readonly NotFoundError _notFoundError = Error.NotFound("Person not found");

    private bool _callbackCalled;

    #region ReadResultFromJsonAsync
    [Fact]
    public async Task Will_read_http_content_as_result()
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new camelcasePerson() { firstName = "Xavier", age = 50 }, SourceGenerationContext.Default.camelcasePerson)
        };

        // Act
        var result = await httpResponseMessage.ReadResultFromJsonAsync(SourceGenerationContext.Default.camelcasePerson, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.firstName.Should().Be("Xavier");
        result.Value.age.Should().Be(50);
    }

    [Fact]
    public async Task Will_throw_JsonException_with_wrong_content()
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.OK)
        {
            Content = new StringContent("Bad JSON")
        };

        // Act
        Func<Task> act = async () => await httpResponseMessage.ReadResultFromJsonAsync(SourceGenerationContext.Default.camelcasePerson, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<JsonException>();
    }

    [Fact]
    public async Task Will_not_throw_JsonException_with_wrong_content()
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("Bad JSON")
        };

        // Act
        var result = await httpResponseMessage.ReadResultFromJsonAsync(SourceGenerationContext.Default.camelcasePerson, CancellationToken.None, true);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Http Response is in a failed state for value camelcasePerson. Status code: BadGateway");
    }

    [Fact]
    public async Task Will_throw_JsonException_with_nulll_content()
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.OK)
        {
            Content = null
        };

        // Act
        Func<Task> act = async () => await httpResponseMessage.ReadResultFromJsonAsync(SourceGenerationContext.Default.camelcasePerson, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<JsonException>();
    }

    [Fact]
    public async Task Will_throw_Exception_for_Internal_Server_Error()
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.InternalServerError);

        // Act
        Func<Task> act = async () => await httpResponseMessage.ReadResultFromJsonAsync(SourceGenerationContext.Default.camelcasePerson, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Deserialize_is_case_sensitive(bool propertyNameCaseInsensitive)
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new camelcasePerson() { firstName = "Xavier", age = 50 }, SourceGenerationContext.Default.camelcasePerson)
        };

        // Act
        var result = await httpResponseMessage.ReadResultFromJsonAsync(
            propertyNameCaseInsensitive ? SourceGenerationCaseInsenstiveContext.Default.PascalPerson : SourceGenerationContext.Default.PascalPerson,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        if (propertyNameCaseInsensitive)
        {
            result.Value.FirstName.Should().Be("Xavier");
            result.Value.Age.Should().Be(50);
        }
        else
        {
            result.Value.FirstName.Should().Be(string.Empty);
            result.Value.Age.Should().Be(0);
        }
    }

    [Fact]
    public async Task When_HttpResponseMessage_is_Task_Will_read_http_content_as_result()
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new camelcasePerson() { firstName = "Xavier", age = 50 }, SourceGenerationContext.Default.camelcasePerson)
        };
        var task = Task.FromResult(httpResponseMessage);

        // Act
        var result = await task.ReadResultFromJsonAsync(SourceGenerationContext.Default.camelcasePerson, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.firstName.Should().Be("Xavier");
        result.Value.age.Should().Be(50);
    }

    [Fact]
    public async Task Will_throw_exception_for_null_JSON()
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        // Act
        Func<Task> act = async () => await httpResponseMessage.ReadResultFromJsonAsync(SourceGenerationContext.Default.camelcasePerson, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<JsonException>();
    }
    #endregion

    [Fact]
    public async Task When_HttpResponseMessage_is_Task_and_NotFound_will_return_NotFound()
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.NotFound);
        var task = Task.FromResult(httpResponseMessage);

        // Act
        var result = await task.HandleNotFoundAsync(_notFoundError);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(_notFoundError);
    }

    [Fact]
    public async Task Will_callback_on_failure()
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Expected space invaders.")
        };

        var callbackCalled = false;
        async Task<Error> CallbackFailedStatusCode(HttpResponseMessage response, string context, CancellationToken cancellationToken)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            content.Should().Be("Expected space invaders.");
            context.Should().Be("Hello");
            callbackCalled = true;
            return Error.NotFound("Bad request");
        }

        // Act
        var result = await httpResponseMessage.HandleFailureAsync(CallbackFailedStatusCode, "Hello", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.NotFound("Bad request"));
        callbackCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Will_task_callback_on_failure()
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Expected space invaders.")
        };
        var task = Task.FromResult(httpResponseMessage);

        var callbackCalled = false;
        async Task<Error> Callback(HttpResponseMessage response, int context, CancellationToken cancellationToken)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            content.Should().Be("Expected space invaders.");
            context.Should().Be(5);
            callbackCalled = true;
            return Error.NotFound("Bad request");
        }

        // Act
        var result = await task.HandleFailureAsync(Callback, 5, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.NotFound("Bad request"));
        callbackCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Will_not_callback_on_success()
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new camelcasePerson() { firstName = "Chris", age = 18 }, SourceGenerationContext.Default.camelcasePerson)
        };

        // Act
        var result = await httpResponseMessage
            .HandleFailureAsync(CallbackFailedStatusCode, "Common", CancellationToken.None)
            .ReadResultFromJsonAsync(SourceGenerationContext.Default.camelcasePerson, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _callbackCalled.Should().BeFalse();
        result.Value.firstName.Should().Be("Chris");
        result.Value.age.Should().Be(18);
    }

    [Fact]
    public async Task Will_task_not_callback_on_success()
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new camelcasePerson() { firstName = "Chris", age = 18 }, SourceGenerationContext.Default.camelcasePerson)
        };
        var task = Task.FromResult(httpResponseMessage);

        // Act
        var result = await task
            .HandleFailureAsync(CallbackFailedStatusCode, "Common", CancellationToken.None)
            .ReadResultFromJsonAsync(SourceGenerationContext.Default.camelcasePerson, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _callbackCalled.Should().BeFalse();
        result.Value.firstName.Should().Be("Chris");
        result.Value.age.Should().Be(18);
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
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.NotFound);

        // Act
        var result = httpResponseMessage.HandleNotFound(_notFoundError);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(_notFoundError);
    }
}
