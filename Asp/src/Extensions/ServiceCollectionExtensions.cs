namespace FunctionalDdd;

using FunctionalDdd.Asp.ModelBinding;
using FunctionalDdd.Asp.Serialization;
using Microsoft.Extensions.DependencyInjection;

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
    /// <item><strong>JSON deserialization:</strong> Values from request body</item>
    /// </list>
    /// </para>
    /// <para>
    /// Validation errors are added to <see cref="Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary"/>,
    /// which integrates with standard ASP.NET Core validation. When used with <c>[ApiController]</c>,
    /// invalid requests automatically return 400 Bad Request with all validation errors.
    /// </para>
    /// </remarks>
    /// <example>
    /// Registration in Program.cs:
    /// <code>
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
        ///         // [ApiController] returns 400 automatically if invalid
        ///         
        ///         var user = new User(dto.Email, dto.FirstName);
        ///         return Ok(new { UserId = user.Id });
        ///     }
        /// }
        /// </code>
        /// </example>
        public static IMvcBuilder AddScalarValueObjectValidation(this IMvcBuilder builder) =>
            builder
                .AddMvcOptions(options => options.ModelBinderProviders.Insert(0, new ScalarValueObjectModelBinderProvider()))
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new ScalarValueObjectJsonConverterFactory()));
    }
