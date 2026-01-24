using SampleUserLibrary;
using FunctionalDdd;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SampleMinimalApiNoAot.API;

var builder = WebApplication.CreateBuilder(args);

// NO JsonSerializerContext - uses standard JSON serialization with reflection fallback
// This demonstrates that the library works perfectly without source generation!
builder.Services.AddScalarValueObjectValidationForMinimalApi();

Action<ResourceBuilder> configureResource = r => r.AddService(
    serviceName: "SampleMinimalApiNoAot",
    serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown");
builder.Services.AddOpenTelemetry()
    .ConfigureResource(configureResource)
    .WithTracing(tracing
        => tracing.AddSource("SampleMinimalApiNoAot")
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

// NO [GenerateValueObjectConverters] attribute
// NO JsonSerializerContext
// Uses standard reflection-based JSON serialization - works perfectly!
