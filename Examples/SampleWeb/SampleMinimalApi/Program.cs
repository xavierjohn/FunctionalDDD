using System.Text.Json.Serialization;
using FunctionalDdd;
using FunctionalDdd.PrimitiveValueObjects;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SampleMinimalApi.API;
using SampleUserLibrary;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));
builder.Services.AddScalarValueValidationForMinimalApi();

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

app.UseScalarValueValidation();

// Welcome endpoint with API information
app.MapGet("/", () => Results.Ok(new WelcomeResponse(
    Name: "FunctionalDDD Sample Minimal API",
    Version: "1.0.0",
    Description: "Demonstrates FunctionalDDD Railway Oriented Programming with Minimal APIs, AOT, and source generation",
    Endpoints: new EndpointsInfo(
        Users: new UserEndpoints(
            Register: "POST /users/register - Register user with manual validation (Result.Combine)",
            RegisterCreated: "POST /users/registerCreated - Register user returning 201 Created",
            RegisterAutoValidation: "POST /users/RegisterWithAutoValidation - Register with auto-validation (Maybe<Url> for optional website)",
            Errors: [
                "GET /users/notfound/{id} - Returns 404 Not Found",
                "GET /users/conflict/{id} - Returns 409 Conflict",
                "GET /users/forbidden/{id} - Returns 403 Forbidden",
                "GET /users/unauthorized/{id} - Returns 401 Unauthorized",
                "GET /users/unexpected/{id} - Returns 500 Internal Server Error"
            ]
        )
    ),
    Documentation: "See SampleApi.http for complete API examples"
))).WithName("Welcome");

app.UseUserRoute();
app.UseMoneyRoute();
app.UseOrderRoute();
app.Run();

#pragma warning disable CA1050 // Declare types in namespaces
public record SharedNameTypeResponse(string FirstName, string LastName, string Email, string Message);
public record WelcomeResponse(string Name, string Version, string Description, EndpointsInfo Endpoints, string Documentation);
public record EndpointsInfo(UserEndpoints Users);
public record UserEndpoints(string Register, string RegisterCreated, string RegisterAutoValidation, string[] Errors);
#pragma warning restore CA1050 // Declare types in namespaces

[GenerateScalarValueConverters]
[JsonSerializable(typeof(WelcomeResponse))]
[JsonSerializable(typeof(EndpointsInfo))]
[JsonSerializable(typeof(UserEndpoints))]
[JsonSerializable(typeof(RegisterUserRequest))]
[JsonSerializable(typeof(RegisterUserDto))]
[JsonSerializable(typeof(RegisterWithNameDto))]
[JsonSerializable(typeof(SharedNameTypeResponse))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(FunctionalDdd.PrimitiveValueObjects.Money))]
[JsonSerializable(typeof(FunctionalDdd.PrimitiveValueObjects.Money[]))]
[JsonSerializable(typeof(SampleMinimalApi.API.MoneyDto))]
[JsonSerializable(typeof(SampleMinimalApi.API.CreateMoneyRequest))]
[JsonSerializable(typeof(SampleMinimalApi.API.MoneyOperationRequest))]
[JsonSerializable(typeof(SampleMinimalApi.API.MultiplyMoneyRequest))]
[JsonSerializable(typeof(SampleMinimalApi.API.MultiplyByQuantityRequest))]
[JsonSerializable(typeof(SampleMinimalApi.API.DivideMoneyRequest))]
[JsonSerializable(typeof(SampleMinimalApi.API.AllocateMoneyRequest))]
[JsonSerializable(typeof(SampleMinimalApi.API.CompareMoneyRequest))]
[JsonSerializable(typeof(SampleMinimalApi.API.CartTotalRequest))]
[JsonSerializable(typeof(SampleMinimalApi.API.ApplyDiscountRequest))]
[JsonSerializable(typeof(SampleMinimalApi.API.SplitBillRequest))]
[JsonSerializable(typeof(SampleMinimalApi.API.RevenueShareRequest))]
[JsonSerializable(typeof(SampleMinimalApi.API.RevenueShareResponse))]
[JsonSerializable(typeof(UpdateOrderDto))]
[JsonSerializable(typeof(CreateOrderDto))]
[JsonSerializable(typeof(OrderState))]
[JsonSerializable(typeof(SampleMinimalApi.API.OrderStateInfo))]
[JsonSerializable(typeof(SampleMinimalApi.API.OrderStatesResponse))]
[JsonSerializable(typeof(SampleMinimalApi.API.OrderStateDetailResponse))]
[JsonSerializable(typeof(SampleMinimalApi.API.UpdateOrderResponse))]
[JsonSerializable(typeof(SampleMinimalApi.API.CustomerInfo))]
[JsonSerializable(typeof(SampleMinimalApi.API.CreateOrderResponse))]
[JsonSerializable(typeof(SampleMinimalApi.API.FilterOrdersResponse))]
[JsonSerializable(typeof(Error))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Http.HttpResults.ValidationProblem))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}