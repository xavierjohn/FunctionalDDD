namespace RailwayOrientedProgramming.Tests.Results.Extensions;
using System.Net.Http.Json;
using System.Net;
using System.Threading.Tasks;
using FunctionalDDD;
using System.Text.Json;

public class HttpResponseMessageJsonExtensionsTests
{
    readonly NotFoundError _notFoundError = Error.NotFound("Person not found");

    [Fact]
    public async Task Will_read_http_content_as_result()
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new CamelPerson() { firstName = "Xavier", age = 50 })
        };

        // Act
        var result = await httpResponseMessage.ReadResultWithNotFoundAsync<CamelPerson>(_notFoundError);

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
        var result = await httpResponseMessage.ReadResultWithNotFoundAsync<CamelPerson>(_notFoundError);

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
        Func<Task> act = async () => await httpResponseMessage.ReadResultWithNotFoundAsync<CamelPerson>(_notFoundError);

        // Assert
        await act.Should().ThrowAsync<JsonException>();
    }

    [Fact]
    public async Task Will_throw_Exception_for_Internal_Server_Error()
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.InternalServerError);

        // Act
        Func<Task> act = async () => await httpResponseMessage.ReadResultWithNotFoundAsync<CamelPerson>(_notFoundError);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task Will_throw_Exception_for_Bad_Request()
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Expected space invaders.")
        };

        var callbackCalled = false;
        async Task callbackFailedStatusCode(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Be("Expected space invaders.");
            callbackCalled = true;
        }

        // Act
        Func<Task> act = async () => await httpResponseMessage.ReadResultWithNotFoundAsync<CamelPerson>(_notFoundError, callbackFailedStatusCode);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
        callbackCalled.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Deserialize_is_case_sensitive(bool propertyNameCaseInsensitive)
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new CamelPerson() { firstName = "Xavier", age = 50 })
        };
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = propertyNameCaseInsensitive
        };

        // Act
        var result = await httpResponseMessage.ReadResultWithNotFoundAsync<PascalPerson>(_notFoundError, jsonSerializerOptions: options);

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
            Content = JsonContent.Create(new CamelPerson() { firstName = "Xavier", age = 50 })
        };
        var task = Task.FromResult(httpResponseMessage);

        // Act
        var result = await task.ReadResultWithNotFoundAsync<CamelPerson>(_notFoundError);

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
        var result = await task.ReadResultWithNotFoundAsync<CamelPerson>(_notFoundError);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(_notFoundError);
    }

    public class CamelPerson
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
