namespace Trellis.Testing;

using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;

/// <summary>
/// Extension methods for <see cref="WebApplicationFactory{TEntryPoint}"/>
/// that simplify creating authenticated HTTP clients for integration tests.
/// </summary>
public static class WebApplicationFactoryExtensions
{
    /// <summary>
    /// Creates an <see cref="HttpClient"/> with the <c>X-Test-Actor</c> header pre-set,
    /// encoding the specified actor identity and permissions as JSON.
    /// </summary>
    /// <typeparam name="TEntryPoint">The entry point class of the web application under test.</typeparam>
    /// <param name="factory">The web application factory.</param>
    /// <param name="actorId">The unique identifier of the test actor.</param>
    /// <param name="permissions">The permissions granted to the test actor.</param>
    /// <returns>An <see cref="HttpClient"/> with the actor header configured.</returns>
    /// <example>
    /// <code>
    /// var client = _factory.CreateClientWithActor("user-1", Permissions.OrdersCreate, Permissions.OrdersRead);
    /// var response = await client.PostAsync("/api/orders", content);
    /// response.StatusCode.Should().Be(HttpStatusCode.OK);
    /// </code>
    /// </example>
    public static HttpClient CreateClientWithActor<TEntryPoint>(
        this WebApplicationFactory<TEntryPoint> factory,
        string actorId,
        params string[] permissions)
        where TEntryPoint : class
    {
        permissions ??= [];

        var client = factory.CreateClient();
        var json = new JsonObject
        {
            ["Id"] = actorId,
            ["Permissions"] = new JsonArray(permissions.Select(p => (JsonNode)JsonValue.Create(p)!).ToArray())
        }.ToJsonString();
        client.DefaultRequestHeaders.Add("X-Test-Actor", json);
        return client;
    }
}