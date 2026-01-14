namespace FunctionalDdd;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;
using MinimalApiJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

/// <summary>
/// Extension methods for configuring value object validation in ASP.NET Core applications.
/// </summary>
/// <remarks>
/// <para>
/// These extensions enable automatic validation of value objects during JSON deserialization.
/// When configured, DTOs can contain value objects directly (like <c>EmailAddress</c>, <c>FirstName</c>),
/// and validation errors are automatically collected and returned as 400 Bad Request responses.
/// </para>
/// <para>
/// This eliminates the need for manual validation chains in every controller action:
/// <code>
/// // Before: Manual validation in every action
/// [HttpPost]
/// public ActionResult&lt;User&gt; Register([FromBody] RegisterRequest request) =>
///     FirstName.TryCreate(request.firstName)
///         .Combine(LastName.TryCreate(request.lastName))
///         .Combine(EmailAddress.TryCreate(request.email))
///         .Bind((first, last, email) => User.TryCreate(first, last, email))
///         .ToActionResult(this);
/// 
/// // After: Value objects in DTO, automatic validation
/// public record CreateUserRequest(FirstName FirstName, LastName LastName, EmailAddress Email);
/// 
/// [HttpPost]
/// public ActionResult&lt;User&gt; Register([FromBody] CreateUserRequest request) =>
///     User.TryCreate(request.FirstName, request.LastName, request.Email)
///         .ToActionResult(this);
/// </code>
/// </para>
/// </remarks>
public static class ValueObjectValidationExtensions
{
    /// <summary>
    /// Adds value object validation services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method configures:
    /// <list type="bullet">
    /// <item>JSON serializer options with <see cref="ValidatingJsonConverterFactory"/></item>
    /// <item>MVC options with <see cref="ValueObjectValidationFilter"/></item>
    /// </list>
    /// </para>
    /// <para>
    /// Call this method after <c>AddControllers()</c> or <c>AddControllersWithViews()</c>:
    /// <code>
    /// builder.Services.AddControllers();
    /// builder.Services.AddValueObjectValidation();
    /// </code>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// 
    /// builder.Services.AddControllers();
    /// builder.Services.AddValueObjectValidation();
    /// 
    /// var app = builder.Build();
    /// 
    /// app.UseValueObjectValidation();
    /// app.MapControllers();
    /// 
    /// app.Run();
    /// </code>
    /// </example>
    public static IServiceCollection AddValueObjectValidation(this IServiceCollection services)
    {
        // Configure MVC JSON options (for controllers)
        services.Configure<MvcJsonOptions>(options =>
            options.JsonSerializerOptions.Converters.Add(new ValidatingJsonConverterFactory()));

        // Configure Minimal API JSON options
        services.Configure<MinimalApiJsonOptions>(options =>
            options.SerializerOptions.Converters.Add(new ValidatingJsonConverterFactory()));

        // Add the validation filter for MVC
        services.Configure<MvcOptions>(options =>
            options.Filters.Add<ValueObjectValidationFilter>());

        return services;
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
