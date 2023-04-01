namespace RailwayOrientedProgramming.Tests.Results.Extensions;
using System.Net.Http.Json;
using System.Net;
using System.Threading.Tasks;
using FunctionalDDD;
using System.Text.Json;

public class HttpResponseMessageJsonExtensionsTests
{
    public class Person
    {
        public string firstName { get; set; } = string.Empty;
        public int age { get; set; }
    }

    readonly NotFoundError _notFoundError = Error.NotFound("Person not found");

    [Fact]
    public async Task Will_read_http_content_as_result()
    {
        // Assign
        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new Person() { firstName = "Xavier", age = 50 })
        };

        // Act
        var result = await httpResponseMessage.ReadResultWithNotFoundAsync<Person>(_notFoundError);

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
        var result = await httpResponseMessage.ReadResultWithNotFoundAsync<Person>(_notFoundError);

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
        Func<Task> act = async () => await httpResponseMessage.ReadResultWithNotFoundAsync<Person>(_notFoundError);

        // Assert
        await act.Should().ThrowAsync<JsonException>();
    }
}
