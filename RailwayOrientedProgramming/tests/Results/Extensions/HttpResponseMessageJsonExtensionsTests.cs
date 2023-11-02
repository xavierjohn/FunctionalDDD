namespace RailwayOrientedProgramming.Tests.Results.Extensions;
using System.Net.Http.Json;
using System.Net;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using FunctionalDdd;

public class HttpResponseMessageJsonExtensionsTests
{
    readonly NotFoundError _notFoundError = Error.NotFound("Person not found");

    private bool _callbackCalled;

    [Fact]
    public async Task Will_read_http_content_as_result()
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new camelcasePerson() { firstName = "Xavier", age = 50 })
        };

        // Act
        var result = await httpResponseMessage.ReadResultWithNotFoundAsync<camelcasePerson>(_notFoundError);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.firstName.Should().Be("Xavier");
        result.Value.age.Should().Be(50);
    }

    [Fact]
    public async Task When_NotFound_will_return_NotFound()
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.NotFound);

        // Act
        var result = await httpResponseMessage.ReadResultWithNotFoundAsync<camelcasePerson>(_notFoundError);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(_notFoundError);
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
        Func<Task> act = async () => await httpResponseMessage.ReadResultWithNotFoundAsync<camelcasePerson>(_notFoundError);

        // Assert
        await act.Should().ThrowAsync<JsonException>();
    }

    [Fact]
    public async Task Will_throw_Exception_for_Internal_Server_Error()
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.InternalServerError);

        // Act
        Func<Task> act = async () => await httpResponseMessage.ReadResultWithNotFoundAsync<camelcasePerson>(_notFoundError);

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
            Content = JsonContent.Create(new camelcasePerson() { firstName = "Xavier", age = 50 })
        };
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = propertyNameCaseInsensitive
        };

        // Act
        var result = await httpResponseMessage.ReadResultWithNotFoundAsync<PascalPerson>(_notFoundError, options);

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
            Content = JsonContent.Create(new camelcasePerson() { firstName = "Xavier", age = 50 })
        };
        var task = Task.FromResult(httpResponseMessage);

        // Act
        var result = await task.ReadResultWithNotFoundAsync<camelcasePerson>(_notFoundError);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.firstName.Should().Be("Xavier");
        result.Value.age.Should().Be(50);
    }

    [Fact]
    public async Task When_HttpResponseMessage_is_Task_and_NotFound_will_return_NotFound()
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.NotFound);
        var task = Task.FromResult(httpResponseMessage);

        // Act
        var result = await task.ReadResultWithNotFoundAsync<camelcasePerson>(_notFoundError);

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
        async Task<Error> CallbackFailedStatusCode(HttpResponseMessage response, string context)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Be("Expected space invaders.");
            context.Should().Be("Hello");
            callbackCalled = true;
            return Error.NotFound("Bad request");
        }

        // Act
        var result = await httpResponseMessage.ReadResultAsync<camelcasePerson, string>(CallbackFailedStatusCode, "Hello");

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
        async Task<Error> Callback(HttpResponseMessage response, int context)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Be("Expected space invaders.");
            context.Should().Be(5);
            callbackCalled = true;
            return Error.NotFound("Bad request");
        }

        // Act
        var result = await task.ReadResultAsync<camelcasePerson, int>(Callback, 5);

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
            Content = JsonContent.Create(new camelcasePerson() { firstName = "Chris", age = 18 })
        };

        // Act
        var result = await httpResponseMessage.ReadResultAsync<camelcasePerson, string>(CallbackFailedStatusCode, "Common");

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
            Content = JsonContent.Create(new camelcasePerson() { firstName = "Chris", age = 18 })
        };
        var task = Task.FromResult(httpResponseMessage);

        // Act
        var result = await task.ReadResultAsync<camelcasePerson, string>(CallbackFailedStatusCode, "Common");

        // Assert
        result.IsSuccess.Should().BeTrue();
        _callbackCalled.Should().BeFalse();
        result.Value.firstName.Should().Be("Chris");
        result.Value.age.Should().Be(18);
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
        Func<Task> act = async () => await httpResponseMessage.ReadResultAsync<camelcasePerson, string>(CallbackFailedStatusCode, "Common");

        // Assert
        await act.Should().ThrowAsync<JsonException>();
    }

    private Task<Error> CallbackFailedStatusCode(HttpResponseMessage response, string context)
    {
        _callbackCalled = true;
        context.Should().Be("Common");
        return Task.FromResult((Error)Error.NotFound("Bad request"));
    }

    public class camelcasePerson
    {
        public string firstName { get; set; } = string.Empty;
        public int age { get; set; }
    }
    public class PascalPerson
    {
        public string FirstName { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}
