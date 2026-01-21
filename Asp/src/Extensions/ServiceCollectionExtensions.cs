namespace FunctionalDdd;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FunctionalDdd.Asp.ModelBinding;
using FunctionalDdd.Asp.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;

/// <summary>
/// Extension methods for configuring automatic value object validation in ASP.NET Core.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds automatic validation for ScalarValueObject-derived types during
    /// model binding and JSON deserialization.
    /// </summary>
    /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
    /// <returns>The <see cref="IMvcBuilder"/> for chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method configures ASP.NET Core to automatically validate value objects that implement
    /// <see cref="IScalarValueObject{TSelf, TPrimitive}"/> during:
    /// <list type="bullet">
    /// <item><strong>Model binding:</strong> Values from route, query, form, or headers</item>
    /// <item><strong>JSON deserialization:</strong> Values from request body (with error collection)</item>
    /// </list>
    /// </para>
    /// <para>
    /// Unlike traditional validation that throws on first error, this approach:
    /// <list type="bullet">
    /// <item>Collects ALL validation errors during JSON deserialization</item>
    /// <item>Uses property names (not JSON paths) in error messages</item>
    /// <item>Returns comprehensive 400 Bad Request with all field errors</item>
    /// <item>Integrates seamlessly with <c>[ApiController]</c> attribute</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// Registration in Program.cs:
    /// <code>
    /// using FunctionalDdd;
    ///
    /// var builder = WebApplication.CreateBuilder(args);
    ///
    /// builder.Services
    ///     .AddControllers()
    ///     .AddScalarValueObjectValidation();
    ///
    /// var app = builder.Build();
    /// app.MapControllers();
    /// app.Run();
    /// </code>
    /// </example>
    /// <example>
    /// Usage in controllers with automatic validation:
    /// <code>
    /// public record RegisterUserDto
    /// {
    ///     public EmailAddress Email { get; init; } = null!;
    ///     public FirstName FirstName { get; init; } = null!;
    /// }
    ///
    /// [ApiController]
    /// [Route("api/users")]
    /// public class UsersController : ControllerBase
    /// {
    ///     [HttpPost]
    ///     public IActionResult Register(RegisterUserDto dto)
    ///     {
    ///         // If we reach here, dto is fully validated!
    ///         // All value objects passed validation
    ///
    ///         var user = User.TryCreate(dto.Email, dto.FirstName);
    ///         return user.ToActionResult(this);
    ///     }
    /// }
    /// </code>
    /// </example>
    public static IMvcBuilder AddScalarValueObjectValidation(this IMvcBuilder builder)
    {
        builder.Services.Configure<MvcJsonOptions>(options =>
            ConfigureJsonOptions(options.JsonSerializerOptions));

        builder.Services.Configure<MvcOptions>(options =>
            options.Filters.Add<ValueObjectValidationFilter>());

        builder.AddMvcOptions(options =>
            options.ModelBinderProviders.Insert(0, new ScalarValueObjectModelBinderProvider()));

        // Configure [ApiController] to not automatically return 400 for invalid ModelState
        // This allows our ValueObjectValidationFilter to handle validation errors properly
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        return builder;
    }

    /// <summary>
    /// Configures JSON serializer options to use property-aware value object validation.
    /// </summary>
    /// <param name="options">The JSON serializer options to configure.</param>
    /// <remarks>
    /// This method configures a type info modifier that assigns converters per-property,
    /// ensuring that validation error field names match the C# property names.
    /// </remarks>
    private static void ConfigureJsonOptions(JsonSerializerOptions options)
    {
        // Use TypeInfoResolver modifier to assign converters per-property with property name
#pragma warning disable IL2026, IL3050 // DefaultJsonTypeInfoResolver requires dynamic code - this is fallback path
        var existingResolver = options.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver();
#pragma warning restore IL2026, IL3050
        options.TypeInfoResolver = existingResolver.WithAddedModifier(ModifyTypeInfo);

        // Also add the factory for direct serialization scenarios
        options.Converters.Add(new ValidatingJsonConverterFactory());
    }

    /// <summary>
    /// Modifies type info to inject property names into ValidationErrorsContext before deserialization.
    /// </summary>
    private static void ModifyTypeInfo(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        foreach (var property in typeInfo.Properties)
        {
            // Check if it's a value object (IScalarValueObject<TSelf, T>)
            if (!IsScalarValueObjectProperty(property))
                continue;

            var propertyType = property.PropertyType;

            // Create a validating converter for this value object
            var innerConverter = CreateValidatingConverter(propertyType);
            if (innerConverter is null)
                continue;

            // Wrap it with property name awareness
            var wrappedConverter = CreatePropertyNameAwareConverter(innerConverter, property.Name, propertyType);
            if (wrappedConverter is not null)
            {
                property.CustomConverter = wrappedConverter;
            }
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "PropertyType comes from JSON serialization infrastructure which preserves type information")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "PropertyType comes from JSON serialization infrastructure which preserves type information")]
    private static bool IsScalarValueObjectProperty(JsonPropertyInfo property)
    {
        var propertyType = property.PropertyType;
        return ImplementsIScalarValueObject(propertyType);
    }

    private static bool ImplementsIScalarValueObject([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type) =>
        type.GetInterfaces()
            .Any(i => i.IsGenericType &&
                     i.GetGenericTypeDefinition() == typeof(IScalarValueObject<,>) &&
                     i.GetGenericArguments()[0] == type);

#pragma warning disable IL2055, IL2060, IL3050, IL2070 // MakeGenericType and Activator require dynamic code
    private static JsonConverter? CreateValidatingConverter(Type valueObjectType)
    {
        var valueObjectInterface = valueObjectType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType &&
                                i.GetGenericTypeDefinition() == typeof(IScalarValueObject<,>) &&
                                i.GetGenericArguments()[0] == valueObjectType);

        if (valueObjectInterface is null)
            return null;

        var primitiveType = valueObjectInterface.GetGenericArguments()[1];
        var converterType = typeof(ValidatingJsonConverter<,>).MakeGenericType(valueObjectType, primitiveType);

        return Activator.CreateInstance(converterType) as JsonConverter;
    }

    private static JsonConverter? CreatePropertyNameAwareConverter(JsonConverter innerConverter, string propertyName, Type type)
    {
        var wrapperType = typeof(PropertyNameAwareConverter<>).MakeGenericType(type);
        return Activator.CreateInstance(wrapperType, innerConverter, propertyName) as JsonConverter;
    }
#pragma warning restore IL2055, IL2060, IL3050, IL2070

    /// <summary>
    /// Adds middleware that creates a validation error collection scope for each request.
    /// This middleware must be registered in the pipeline to enable validation error collection.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    /// <remarks>
    /// <para>
    /// This middleware creates a <see cref="ValidationErrorsContext"/> scope for each request,
    /// allowing <see cref="ValidatingJsonConverter{TValueObject,TPrimitive}"/> to collect
    /// validation errors during JSON deserialization.
    /// </para>
    /// <para>
    /// Register this middleware early in the pipeline, before any middleware that deserializes
    /// JSON request bodies (such as routing or MVC).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var app = builder.Build();
    ///
    /// app.UseValueObjectValidation(); // ← Add this before routing
    /// app.UseRouting();
    /// app.UseAuthentication();
    /// app.UseAuthorization();
    /// app.MapControllers();
    ///
    /// app.Run();
    /// </code>
    /// </example>
    public static IApplicationBuilder UseValueObjectValidation(this IApplicationBuilder app) =>
        app.UseMiddleware<ValueObjectValidationMiddleware>();

    /// <summary>
    /// Configures HTTP JSON options to use property-aware value object validation for Minimal APIs.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method configures Minimal API JSON serialization to automatically validate value objects
    /// that implement <see cref="IScalarValueObject{TSelf, TPrimitive}"/> during JSON deserialization.
    /// </para>
    /// <para>
    /// For Minimal APIs, also use <see cref="UseValueObjectValidation"/> middleware and
    /// <see cref="WithValueObjectValidation"/> on your route handlers.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// builder.Services.AddScalarValueObjectValidationForMinimalApi();
    ///
    /// var app = builder.Build();
    /// app.UseValueObjectValidation();
    ///
    /// app.MapPost("/users", (RegisterUserDto dto) => ...)
    ///    .WithValueObjectValidation();
    /// </code>
    /// </example>
    public static IServiceCollection AddScalarValueObjectValidationForMinimalApi(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
            ConfigureJsonOptions(options.SerializerOptions));
        return services;
    }

    /// <summary>
    /// Adds the value object validation endpoint filter to the route handler.
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>The route handler builder for chaining.</returns>
    /// <remarks>
    /// <para>
    /// This extension adds <see cref="ValueObjectValidationEndpointFilter"/> to check for
    /// validation errors collected during JSON deserialization.
    /// </para>
    /// <para>
    /// Ensure <see cref="UseValueObjectValidation"/> middleware is registered and
    /// <see cref="AddScalarValueObjectValidationForMinimalApi"/> is called for full functionality.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// app.MapPost("/users/register", (RegisterUserDto dto) =>
    /// {
    ///     // dto is already validated
    ///     return Results.Ok(dto);
    /// }).WithValueObjectValidation();
    /// </code>
    /// </example>
    public static RouteHandlerBuilder WithValueObjectValidation(this RouteHandlerBuilder builder) =>
        builder.AddEndpointFilter<ValueObjectValidationEndpointFilter>();
}
