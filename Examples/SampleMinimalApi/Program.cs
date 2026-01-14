using SampleUserLibrary;
using System.Text.Json.Serialization;
using FunctionalDdd;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SampleMinimalApi.API;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

// Add value object validation for Minimal APIs
builder.Services.AddValueObjectValidation();

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

// Enable value object validation middleware (creates validation scope per request)
app.UseValueObjectValidation();

app.UseToDoRoute();
app.UseUserRoute();
app.Run();

#pragma warning disable CA1050 // Declare types in namespaces
public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);
#pragma warning restore CA1050 // Declare types in namespaces

[JsonSerializable(typeof(Todo[]))]
[JsonSerializable(typeof(RegisterUserRequest))]
[JsonSerializable(typeof(CreateUserWithValidationRequest))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(Error))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Http.HttpValidationProblemDetails))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Http.HttpResults.ValidationProblem))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
