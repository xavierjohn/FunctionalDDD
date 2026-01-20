namespace FunctionalDdd;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MinimalApiJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

/// <summary>
/// Extension methods for configuring value object validation in Minimal API applications.
/// </summary>
/// <remarks>
/// <para>
/// These extensions enable automatic validation of value objects during JSON deserialization
/// for <b>Minimal API</b> endpoints. For MVC controllers, use <see cref="ValueObjectModelBindingExtensions.AddValueObjectModelBinding"/>
/// which provides a more idiomatic model binding approach.
/// </para>
/// <para>
/// When configured, DTOs can contain value objects directly (like <c>EmailAddress</c>, <c>FirstName</c>),
/// and validation errors are automatically collected and returned as 400 Bad Request responses.
/// </para>
/// </remarks>
public static class ValueObjectValidationExtensions
{
    /// <summary>
    /// Adds value object validation services for Minimal API endpoints.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method configures JSON serializer options with <see cref="ValidatingJsonConverterFactory"/>
    /// for Minimal API endpoints.
    /// </para>
    /// <para>
    /// <b>For MVC controllers</b>, use <see cref="ValueObjectModelBindingExtensions.AddValueObjectModelBinding"/> instead,
    /// which uses model binding and works with all content types (JSON, XML, Form, Query, Route).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// 
    /// // For Minimal API
    /// builder.Services.AddValueObjectValidation();
    /// 
    /// var app = builder.Build();
    /// 
    /// app.UseValueObjectValidation();
    /// 
    /// app.MapPost("/users", (CreateUserRequest request) => ...)
    ///    .WithValueObjectValidation();
    /// 
    /// app.Run();
    /// </code>
    /// </example>
    public static IServiceCollection AddValueObjectValidation(this IServiceCollection services)
    {
        // Configure Minimal API JSON options
        services.Configure<MinimalApiJsonOptions>(options =>
            ConfigureJsonOptions(options.SerializerOptions));

        return services;
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

        // Also add the factory for direct serialization scenarios (e.g., standalone value objects)
        options.Converters.Add(new ValidatingJsonConverterFactory());
    }

    /// <summary>
    /// Modifies type info to inject property names into ValidationErrorsContext before deserialization.
    /// </summary>
    /// <remarks>
    /// This uses a property-name-aware wrapper converter that sets the property name in 
    /// <see cref="ValidationErrorsContext.CurrentPropertyName"/> before the inner converter reads the value.
    /// This approach is AOT-compatible because it uses cached converters from the registry.
    /// </remarks>
    internal static void ModifyTypeInfo(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        foreach (var property in typeInfo.Properties)
        {
            var propertyType = property.PropertyType;

            // Handle nullable value types
            var underlyingType = Nullable.GetUnderlyingType(propertyType);
            var actualType = underlyingType ?? propertyType;

            // Check if it's a value object (ITryCreatable<T>)
            if (!ValidatingConverterRegistry.HasConverter(actualType) &&
                !ImplementsITryCreatable(actualType))
                continue;

            // Get the cached converter (or create via factory)
            var innerConverter = ValidatingConverterRegistry.GetConverter(actualType)
                                 ?? CreateConverterWithReflection(actualType);

            if (innerConverter is null)
                continue;

            // Wrap with property name awareness
            var wrapper = PropertyNameAwareConverterFactory.Create(innerConverter, property.Name, propertyType);
            if (wrapper is not null)
                property.CustomConverter = wrapper;
        }
    }

    /// <summary>
    /// Checks if a type implements ITryCreatable&lt;T&gt; where T is itself.
    /// </summary>
    private static bool ImplementsITryCreatable(Type type)
    {
        var iTryCreatableType = typeof(ITryCreatable<>);
#pragma warning disable IL2070 // GetInterfaces requires dynamic code - this is fallback path
        return type
            .GetInterfaces()
            .Any(i => i.IsGenericType &&
                     i.GetGenericTypeDefinition() == iTryCreatableType &&
                     i.GetGenericArguments()[0] == type);
#pragma warning restore IL2070
    }

    /// <summary>
    /// Creates a validating converter using reflection (fallback for non-AOT scenarios).
    /// </summary>
    private static JsonConverter? CreateConverterWithReflection(Type type)
    {
#pragma warning disable IL2070, IL2071, IL3050 // MakeGenericType and Activator require dynamic code - this is fallback path
        if (type.IsValueType)
        {
            var converterType = typeof(ValidatingStructJsonConverter<>).MakeGenericType(type);
            return Activator.CreateInstance(converterType) as JsonConverter;
        }
        else
        {
            var converterType = typeof(ValidatingJsonConverter<>).MakeGenericType(type);
            return Activator.CreateInstance(converterType) as JsonConverter;
        }
#pragma warning restore IL2070, IL2071, IL3050
    }

    /// <summary>
    /// Adds value object validation middleware to the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    /// <remarks>
    /// <para>
    /// This middleware creates a validation error collection scope for each request,
    /// enabling <see cref="ValidatingJsonConverter{T}"/> to collect errors across
    /// the entire deserialization process.
    /// </para>
    /// <para>
    /// Call this method early in the middleware pipeline, before routing:
    /// <code>
    /// app.UseValueObjectValidation();
    /// app.UseRouting();
    /// app.MapControllers();
    /// </code>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var app = builder.Build();
    /// 
    /// app.UseValueObjectValidation();
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
}
