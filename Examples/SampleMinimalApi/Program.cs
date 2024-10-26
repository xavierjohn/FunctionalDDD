using SampleUserLibrary;
using System.Text.Json.Serialization;
using FunctionalDdd;
using FunctionalDdd.Http;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));
builder.Services.AddProblemDetails();

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
userApi.MapPost("/register", (RegisterUserRequest request) =>
    FirstName.TryCreate(request.firstName)
    .Combine(LastName.TryCreate(request.lastName))
    .Combine(EmailAddress.TryCreate(request.email))
    .Bind((firstName, lastName, email) => User.TryCreate(firstName, lastName, email, request.password))
    .Map(user => new RegisterUserResponse(user.Id, user.FirstName, user.LastName, user.Email, user.Password))
    .ToOkResult());

app.Run();

#pragma warning disable CA1050 // Declare types in namespaces
public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);
#pragma warning restore CA1050 // Declare types in namespaces

[JsonSerializable(typeof(Todo[]))]
[JsonSerializable(typeof(RegisterUserRequest))]
[JsonSerializable(typeof(RegisterUserResponse))]
[JsonSerializable(typeof(Error))]
[JsonSerializable(typeof(List<ValidationResult>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
