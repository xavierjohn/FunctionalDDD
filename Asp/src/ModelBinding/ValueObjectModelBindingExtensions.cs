namespace FunctionalDdd;

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;

/// <summary>
/// Extension methods for configuring value object validation in MVC applications.
/// </summary>
public static class ValueObjectModelBindingExtensions
{
    /// <summary>
    /// Adds value object validation support for MVC controllers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method configures:
    /// <list type="bullet">
    /// <item><see cref="ValueObjectModelBinderProvider"/> for <c>[FromQuery]</c>, <c>[FromRoute]</c>, <c>[FromForm]</c>, <c>[FromHeader]</c></item>
    /// <item><see cref="ValidatingJsonConverterFactory"/> for <c>[FromBody]</c> JSON deserialization</item>
    /// </list>
    /// </para>
    /// <para>
    /// For <c>[FromBody]</c> with JSON, validation happens during JSON deserialization.
    /// For other binding sources, validation happens during model binding.
    /// Both approaches integrate with <see cref="Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary"/>.
    /// </para>
    /// <para>
    /// <b>Note:</b> This uses reflection and is not AOT-compatible.
    /// For AOT scenarios, use Minimal API with <see cref="ValueObjectValidationExtensions.AddValueObjectValidation"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// 
    /// builder.Services.AddControllers();
    /// builder.Services.AddValueObjectModelBinding();
    /// 
    /// var app = builder.Build();
    /// app.UseValueObjectValidation(); // Required for [FromBody] JSON validation
    /// app.MapControllers();
    /// app.Run();
    /// </code>
    /// </example>
    public static IServiceCollection AddValueObjectModelBinding(this IServiceCollection services)
    {
        // Configure model binder for [FromQuery], [FromRoute], [FromForm], [FromHeader]
        services.Configure<MvcOptions>(options =>
        {
            options.ModelBinderProviders.Insert(0, new ValueObjectModelBinderProvider());

            // Add filter to handle validation errors from JSON deserialization
            options.Filters.Add<ValueObjectValidationFilter>();
        });

        // Configure JSON converter for [FromBody] JSON deserialization
        services.Configure<MvcJsonOptions>(options =>
            ConfigureJsonOptions(options.JsonSerializerOptions));

        return services;
    }

    /// <summary>
    /// Configures JSON serializer options to use property-aware value object validation.
    /// </summary>
    private static void ConfigureJsonOptions(JsonSerializerOptions options)
    {
        // Use TypeInfoResolver modifier to assign converters per-property with property name
#pragma warning disable IL2026, IL3050 // DefaultJsonTypeInfoResolver requires dynamic code
        var existingResolver = options.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver();
#pragma warning restore IL2026, IL3050
        options.TypeInfoResolver = existingResolver.WithAddedModifier(
            ValueObjectValidationExtensions.ModifyTypeInfo);

        // Also add the factory for direct serialization scenarios
        options.Converters.Add(new ValidatingJsonConverterFactory());
    }
}
