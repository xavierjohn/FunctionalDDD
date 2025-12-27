namespace FunctionalDdd;

using System.Diagnostics;
using System.Reflection;

/// <summary>
/// Provides OpenTelemetry activity tracing for Common Value Objects operations.
/// Enables monitoring and diagnostics of value object creation, validation, and parsing activities.
/// </summary>
/// <remarks>
/// <para>
/// This static class configures distributed tracing support for the CommonValueObjects library,
/// allowing you to observe value object operations in Application Insights, Jaeger, Zipkin,
/// or other OpenTelemetry-compatible observability platforms.
/// </para>
/// <para>
/// The <see cref="ActivitySource"/> enables tracing for:
/// <list type="bullet">
/// <item>Email address validation (EmailAddress.TryCreate)</item>
/// <item>Value object parsing and creation</item>
/// <item>Validation success and failure rates</item>
/// <item>Performance metrics for value object operations</item>
/// </list>
/// </para>
/// <para>
/// To enable tracing in your application, register the activity source with your
/// OpenTelemetry configuration using <see cref="CommonValueObjectTraceProviderBuilderExtensions.AddCommonValueObjectInstrumentation"/>.
/// </para>
/// </remarks>
/// <example>
/// Enabling tracing in an ASP.NET Core application:
/// <code>
/// var builder = WebApplication.CreateBuilder(args);
/// 
/// builder.Services.AddOpenTelemetry()
///     .WithTracing(tracerProviderBuilder =>
///         tracerProviderBuilder
///             .AddCommonValueObjectInstrumentation()  // Adds CVO activity source
///             .AddAspNetCoreInstrumentation()
///             .AddHttpClientInstrumentation()
///             .AddConsoleExporter());
/// 
/// var app = builder.Build();
/// 
/// // Now EmailAddress.TryCreate and other CVO operations will be traced
/// app.MapPost("/users", (CreateUserRequest request) =>
///     EmailAddress.TryCreate(request.Email) // This creates a traced activity
///         .Bind(email => _userService.CreateUser(email))
///         .ToHttpResult());
/// </code>
/// </example>
/// <example>
/// Viewing traces in Application Insights:
/// <code>
/// // Trace hierarchy example:
/// // POST /users
/// //   └─ EmailAddress.TryCreate
/// //      ├─ Status: Ok (if valid)
/// //      └─ Status: Error (if invalid)
/// //   └─ UserService.CreateUser
/// //      └─ Database operation
/// 
/// // Each activity includes:
/// // - Operation name: "EmailAddress.TryCreate"
/// // - Duration
/// // - Status (Ok/Error)
/// // - Parent/child relationships
/// </code>
/// </example>
/// <seealso cref="CommonValueObjectTraceProviderBuilderExtensions"/>
/// <seealso cref="ActivitySource"/>
public static class CommonValueObjectTrace
{
    /// <summary>
    /// Gets the assembly name of the CommonValueObjects library.
    /// Used for versioning and metadata in traces.
    /// </summary>
    internal static readonly AssemblyName AssemblyName = typeof(CommonValueObjectTrace).Assembly.GetName();
    
    /// <summary>
    /// Gets the name of the activity source used for CommonValueObjects tracing.
    /// Value: "Functional DDD CVO"
    /// </summary>
    /// <remarks>
    /// This name is used to identify traces from this library in observability platforms.
    /// Register this name when configuring OpenTelemetry tracing.
    /// </remarks>
    internal static readonly string ActivitySourceName = "Functional DDD CVO";
    
    /// <summary>
    /// Gets the version of the CommonValueObjects library.
    /// </summary>
    /// <remarks>
    /// The version is included in trace metadata to help correlate behavior with specific library versions.
    /// </remarks>
    internal static readonly Version Version = AssemblyName.Version!;
    
    /// <summary>
    /// Gets the <see cref="ActivitySource"/> for tracing CommonValueObjects operations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This activity source is used by value objects (like <see cref="EmailAddress"/>) to create
    /// tracing spans for operations such as validation and parsing.
    /// </para>
    /// <para>
    /// To enable tracing, add this source to your OpenTelemetry configuration:
    /// <code>
    /// builder.Services.AddOpenTelemetry()
    ///     .WithTracing(b => b.AddCommonValueObjectInstrumentation());
    /// </code>
    /// </para>
    /// <para>
    /// Activities created include:
    /// <list type="bullet">
    /// <item>Activity name: "{ValueObjectType}.{MethodName}" (e.g., "EmailAddress.TryCreate")</item>
    /// <item>Status: Ok for successful operations, Error for failures</item>
    /// <item>Duration: Time taken for the operation</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName, Version.ToString());
}
