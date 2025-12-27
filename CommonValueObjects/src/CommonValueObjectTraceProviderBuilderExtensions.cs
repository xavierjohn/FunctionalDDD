namespace FunctionalDdd;

using OpenTelemetry.Trace;

/// <summary>
/// Provides extension methods for configuring OpenTelemetry tracing for CommonValueObjects operations.
/// </summary>
public static class CommonValueObjectTraceProviderBuilderExtensions
{
    /// <summary>
    /// Adds CommonValueObjects instrumentation to the OpenTelemetry tracing pipeline.
    /// Enables distributed tracing for value object creation, validation, and parsing operations.
    /// </summary>
    /// <param name="builder">The <see cref="TracerProviderBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="TracerProviderBuilder"/> for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// This extension method registers the CommonValueObjects <see cref="ActivitySource"/> with OpenTelemetry,
    /// allowing you to observe and monitor value object operations in your distributed tracing system.
    /// </para>
    /// <para>
    /// Once enabled, operations like <see cref="EmailAddress.TryCreate"/> will automatically create
    /// trace spans with:
    /// <list type="bullet">
    /// <item>Operation name (e.g., "EmailAddress.TryCreate")</item>
    /// <item>Success/error status</item>
    /// <item>Duration metrics</item>
    /// <item>Parent-child relationship with calling code</item>
    /// </list>
    /// </para>
    /// <para>
    /// Benefits of enabling CVO instrumentation:
    /// <list type="bullet">
    /// <item>Monitor validation performance and identify slow operations</item>
    /// <item>Track validation failure rates and patterns</item>
    /// <item>Correlate value object operations with business transactions</item>
    /// <item>Debug distributed systems by following trace hierarchies</item>
    /// <item>Analyze user input validation patterns</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// Basic OpenTelemetry configuration in ASP.NET Core:
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// 
    /// builder.Services.AddOpenTelemetry()
    ///     .WithTracing(tracerProviderBuilder =>
    ///         tracerProviderBuilder
    ///             .AddFunctionalDddCvoInstrumentation()  // Enable CVO tracing
    ///             .AddAspNetCoreInstrumentation()         // Add ASP.NET Core tracing
    ///             .AddHttpClientInstrumentation()         // Add HTTP client tracing
    ///             .AddConsoleExporter());                 // Export to console
    /// 
    /// var app = builder.Build();
    /// app.Run();
    /// </code>
    /// </example>
    /// <example>
    /// Configuration with Application Insights:
    /// <code>
    /// builder.Services.AddOpenTelemetry()
    ///     .WithTracing(tracerProviderBuilder =>
    ///         tracerProviderBuilder
    ///             .AddFunctionalDddCvoInstrumentation()
    ///             .AddAspNetCoreInstrumentation()
    ///             .AddAzureMonitorTraceExporter(options =>
    ///             {
    ///                 options.ConnectionString = 
    ///                     builder.Configuration["ApplicationInsights:ConnectionString"];
    ///             }));
    /// 
    /// // Now you can see CVO traces in Application Insights:
    /// // - Search for operations like "EmailAddress.TryCreate"
    /// // - View validation success/failure rates
    /// // - Analyze performance metrics
    /// </code>
    /// </example>
    /// <example>
    /// Trace hierarchy example in a typical API call:
    /// <code>
    /// // HTTP Request: POST /users
    /// app.MapPost("/users", (CreateUserRequest request) =>
    ///     EmailAddress.TryCreate(request.Email)
    ///         .Combine(FirstName.TryCreate(request.FirstName))
    ///         .Combine(LastName.TryCreate(request.LastName))
    ///         .Bind((email, first, last) => _userService.CreateUser(email, first, last))
    ///         .ToHttpResult());
    /// 
    /// // Resulting trace hierarchy:
    /// // POST /users (200ms)
    /// //   ├─ EmailAddress.TryCreate (1ms) [Status: Ok]
    /// //   ├─ FirstName.TryCreate (0.5ms) [Status: Ok]
    /// //   ├─ LastName.TryCreate (0.5ms) [Status: Ok]
    /// //   └─ UserService.CreateUser (195ms)
    /// //      ├─ Repository.Add (180ms)
    /// //      │  └─ SQL INSERT (175ms)
    /// //      └─ EventBus.Publish (15ms)
    /// </code>
    /// </example>
    /// <example>
    /// Monitoring validation failures:
    /// <code>
    /// // When validation fails, traces show error status:
    /// // EmailAddress.TryCreate (1ms) [Status: Error]
    /// //   └─ Error: "Email address is not valid."
    /// 
    /// // Query Application Insights for failed validations:
    /// // requests
    /// // | where operation_Name == "EmailAddress.TryCreate"
    /// // | where success == false
    /// // | summarize count() by bin(timestamp, 1h)
    /// </code>
    /// </example>
    /// <seealso cref="CommonValueObjectTrace"/>
    /// <seealso cref="TracerProviderBuilder"/>
    public static TracerProviderBuilder AddFunctionalDddCvoInstrumentation(this TracerProviderBuilder builder)
        => builder.AddSource(CommonValueObjectTrace.ActivitySourceName);
}
