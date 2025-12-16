namespace SampleMinimalApi.API;

using FunctionalDdd;
using SampleUserLibrary;
using System.Globalization;

public static class UserRoutes
{
    public static void UseUserRoute(this WebApplication app)
    {
        RouteGroupBuilder userApi = app.MapGroup("/users");

        userApi.MapGet("/", () => "Hello Users");

        userApi.MapGet("/{name}", (string name) => $"Hello {name}").WithName("GetUserById");

        userApi.MapPost("/register", (RegisterUserRequest request) =>
            FirstName.TryCreate(request.firstName)
            .Combine(LastName.TryCreate(request.lastName))
            .Combine(EmailAddress.TryCreate(request.email))
            .Bind((firstName, lastName, email) => User.TryCreate(firstName, lastName, email, request.password))
            .ToHttpResult());

        userApi.MapPost("/registerCreated", (RegisterUserRequest request) =>
            FirstName.TryCreate(request.firstName)
            .Combine(LastName.TryCreate(request.lastName))
            .Combine(EmailAddress.TryCreate(request.email))
            .Bind((firstName, lastName, email) => User.TryCreate(firstName, lastName, email, request.password))
            .Finally(
                    ok => Results.CreatedAtRoute("GetUserById", new RouteValueDictionary { { "name", ok.FirstName } }, ok),
                    err => err.ToHttpResult()));

        userApi.MapGet("/notfound/{id}", (int id) =>
            Result.Failure(Error.NotFound("User not found", id.ToString(CultureInfo.InvariantCulture)))
            .ToHttpResult());

        userApi.MapGet("/conflict/{id}", (int id) =>
            Result.Failure(Error.Conflict("Record has changed.", id.ToString(CultureInfo.InvariantCulture)))
            .ToHttpResult());

        userApi.MapGet("/forbidden/{id}", (int id) =>
            Result.Failure(Error.Forbidden("You do not have access.", id.ToString(CultureInfo.InvariantCulture)))
            .ToHttpResult());

        userApi.MapGet("/unauthorized/{id}", (int id) =>
            Result.Failure(Error.Unauthorized("You have not been authorized.", id.ToString(CultureInfo.InvariantCulture)))
            .ToHttpResult());

        userApi.MapGet("/unexpected/{id}", (int id) =>
            Result.Failure(Error.Unexpected("Internal server error.", id.ToString(CultureInfo.InvariantCulture)))
            .ToHttpResult());

    }

}