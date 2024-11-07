using SampleUserLibrary;
using System.Text.Json.Serialization;
using FunctionalDdd;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

var app = builder.Build();

var sampleTodos = new Todo[] {
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", () => sampleTodos);
todosApi.MapGet("/{id}", (int id) =>
    sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

var userApi = app.MapGroup("/users");
userApi.MapGet("/", () => "Hello Users");

userApi.MapGet("/{name}", (string name) => $"Hello {name}").WithName("GetUserById");

userApi.MapPost("/register", (RegisterUserRequest request) =>
    FirstName.TryCreate(request.firstName)
    .Combine(LastName.TryCreate(request.lastName))
    .Combine(EmailAddress.TryCreate(request.email))
    .Bind((firstName, lastName, email) => User.TryCreate(firstName, lastName, email, request.password))
    .ToOkResult());

userApi.MapPost("/registerCreated", (RegisterUserRequest request) =>
    FirstName.TryCreate(request.firstName)
    .Combine(LastName.TryCreate(request.lastName))
    .Combine(EmailAddress.TryCreate(request.email))
    .Bind((firstName, lastName, email) => User.TryCreate(firstName, lastName, email, request.password))
    .Finally(
            ok => Results.CreatedAtRoute("GetUserById", new RouteValueDictionary { { "name", ok.FirstName } }, ok),
            err => err.ToErrorResult()));

userApi.MapGet("/notfound/{id}", (int id) =>
    Result.Failure(Error.NotFound("User not found", id.ToString()))
    .ToOkResult());

userApi.MapGet("/conflict/{id}", (int id) =>
    Result.Failure(Error.Conflict("Record has changed.", id.ToString()))
    .ToOkResult());

userApi.MapGet("/forbidden/{id}", (int id) =>
    Result.Failure(Error.Forbidden("You do not have access.", id.ToString()))
    .ToOkResult());

userApi.MapGet("/unauthorized/{id}", (int id) =>
    Result.Failure(Error.Unauthorized("You have not been authorized.", id.ToString()))
    .ToOkResult());

userApi.MapGet("/unexpected/{id}", (int id) =>
    Result.Failure(Error.Unexpected("Internal server error.", id.ToString()))
    .ToOkResult());

app.Run();

#pragma warning disable CA1050 // Declare types in namespaces
public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);
#pragma warning restore CA1050 // Declare types in namespaces

[JsonSerializable(typeof(Todo[]))]
[JsonSerializable(typeof(RegisterUserRequest))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(Error))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Http.HttpResults.ValidationProblem))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
