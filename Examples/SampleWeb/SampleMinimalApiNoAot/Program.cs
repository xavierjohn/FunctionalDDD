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

// Welcome endpoint with API information
#pragma warning disable CA1861 // Prefer 'static readonly' fields - one-time startup configuration
app.MapGet("/", () => Results.Ok(new
{
    name = "FunctionalDDD Sample Minimal API (No AOT)",
    version = "1.0.0",
    description = "Demonstrates FunctionalDDD Railway Oriented Programming with Minimal APIs and reflection fallback (no source generation)",
    endpoints = new
    {
        users = new
        {
            register = "POST /users/register - Register user with manual validation (Result.Combine)",
            registerCreated = "POST /users/registerCreated - Register user returning 201 Created",
            registerAutoValidation = "POST /users/RegisterWithAutoValidation - Register with automatic value object validation",
            errors = new string[]
            {
                "GET /users/notfound/{id} - Returns 404 Not Found",
                "GET /users/conflict/{id} - Returns 409 Conflict",
                "GET /users/forbidden/{id} - Returns 403 Forbidden",
                "GET /users/unauthorized/{id} - Returns 401 Unauthorized",
                "GET /users/unexpected/{id} - Returns 500 Internal Server Error"
            }
        }
    },
    documentation = "See SampleApi.http for complete API examples"
})).WithName("Welcome");
#pragma warning restore CA1861

app.UseUserRoute();
app.UseMoneyRoute();
app.Run();

#pragma warning disable CA1050 // Declare types in namespaces
public record SharedNameTypeResponse(string FirstName, string LastName, string Email, string Message);
#pragma warning restore CA1050 // Declare types in namespaces

// NO [GenerateValueObjectConverters] attribute
// NO JsonSerializerContext
// Uses standard reflection-based JSON serialization - works perfectly!
