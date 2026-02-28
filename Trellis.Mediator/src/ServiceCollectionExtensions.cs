namespace Trellis.Mediator;

using global::Mediator;
using Microsoft.Extensions.DependencyInjection;
using Trellis.Authorization;

/// <summary>
/// Extension methods for registering Trellis.Mediator pipeline behaviors.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Gets the ordered array of Trellis Result-aware pipeline behavior types.
    /// Assign this to <c>MediatorOptions.PipelineBehaviors</c> in your <c>AddMediator</c> call.
    /// <para>Behaviors execute in this order (outermost to innermost):</para>
    /// <list type="number">
    ///   <item><description><see cref="ExceptionBehavior{TMessage, TResponse}"/> — catches unhandled exceptions</description></item>
    ///   <item><description><see cref="TracingBehavior{TMessage, TResponse}"/> — OpenTelemetry activity span</description></item>
    ///   <item><description><see cref="LoggingBehavior{TMessage, TResponse}"/> — structured logging with duration</description></item>
    ///   <item><description><see cref="AuthorizationBehavior{TMessage, TResponse}"/> — checks static permissions (<see cref="IAuthorize"/>)</description></item>
    ///   <item><description><see cref="ResourceAuthorizationBehavior{TMessage, TResponse}"/> — checks resource-based auth (<see cref="IAuthorizeResource"/>)</description></item>
    ///   <item><description><see cref="ValidationBehavior{TMessage, TResponse}"/> — short-circuits on validation failure</description></item>
    /// </list>
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddMediator(options =>
    /// {
    ///     options.Assemblies = [typeof(MyCommand).Assembly];
    ///     options.PipelineBehaviors = ServiceCollectionExtensions.PipelineBehaviors;
    /// });
    /// </code>
    /// </example>
    public static Type[] PipelineBehaviors =>
    [
        typeof(ExceptionBehavior<,>),
        typeof(TracingBehavior<,>),
        typeof(LoggingBehavior<,>),
        typeof(AuthorizationBehavior<,>),
        typeof(ResourceAuthorizationBehavior<,>),
        typeof(ValidationBehavior<,>),
    ];

    /// <summary>
    /// Registers Trellis Result-aware pipeline behaviors as open generic
    /// <see cref="IPipelineBehavior{TMessage, TResponse}"/> implementations.
    /// Use this when NOT using <c>MediatorOptions.PipelineBehaviors</c> (non-AOT scenario).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTrellisBehaviors(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ExceptionBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(TracingBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ResourceAuthorizationBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
