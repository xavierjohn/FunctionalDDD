using SampleUserLibrary;
using System.Text.Json.Serialization;
using FunctionalDdd;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SampleMinimalApi.API;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));
builder.Services.AddScalarValueObjectValidationForMinimalApi();

Action<ResourceBuilder> configureResource = r => r.AddService(
    serviceName: "SampleMinimalApi",
    serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown");
builder.Services.AddOpenTelemetry()
    .ConfigureResource(configureResource)
    .WithTracing(tracing
        => tracing.AddSource("SampleMinimalApi")
            .SetSampler(new AlwaysOnSampler())
            .AddPrimitiveValueObjectInstrumentation()
            .AddOtlpExporter());

var app = builder.Build();

app.UseValueObjectValidation();
app.UseToDoRoute();
app.UseUserRoute();
app.Run();

#pragma warning disable CA1050 // Declare types in namespaces
public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);
public record SharedNameTypeResponse(string FirstName, string LastName, string Email, string Message);
#pragma warning restore CA1050 // Declare types in namespaces

[GenerateValueObjectConverters]
[JsonSerializable(typeof(Todo[]))]
[JsonSerializable(typeof(RegisterUserRequest))]
[JsonSerializable(typeof(RegisterUserDto))]
[JsonSerializable(typeof(RegisterWithNameDto))]
[JsonSerializable(typeof(SharedNameTypeResponse))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(Error))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Http.HttpResults.ValidationProblem))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
