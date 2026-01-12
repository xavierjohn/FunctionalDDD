using System.Diagnostics.CodeAnalysis;

namespace FunctionalDdd.Asp.ModelBinding;

using Microsoft.AspNetCore.Mvc.ModelBinding;

/// <summary>
/// Model binder provider that supplies <see cref="ValueObjectModelBinder{T}"/> for types implementing <see cref="ITryCreatable{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// This provider is registered in the MVC options and automatically provides the appropriate
/// model binder for any parameter or DTO property that implements ITryCreatable.
/// </para>
/// <para>
/// The provider:
/// <list type="bullet">
/// <item>Inspects the model type to see if it implements ITryCreatable&lt;T&gt;</item>
/// <item>If yes: Creates a ValueObjectModelBinder&lt;T&gt; for that type</item>
/// <item>If no: Returns null to allow other providers to handle the type</item>
/// </list>
/// </para>
/// <para>
/// Registration is done once at startup via <see cref="MvcOptionsExtensions.AddValueObjectModelBinding"/>:
/// <code>
/// builder.Services.AddControllers(options =>
/// {
///     options.AddValueObjectModelBinding();
/// });
/// </code>
/// </para>
/// </remarks>
public class ValueObjectModelBinderProvider : IModelBinderProvider
{
    /// <summary>
    /// Gets the appropriate model binder for the given context.
    /// </summary>
    /// <param name="context">The model binder provider context.</param>
    /// <returns>
    /// A <see cref="ValueObjectModelBinder{T}"/> if the model type implements ITryCreatable;
    /// otherwise null to allow other providers to handle the type.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method uses reflection to:
    /// <list type="number">
    /// <item>Get all interfaces implemented by the model type</item>
    /// <item>Find ITryCreatable&lt;T&gt; if it exists</item>
    /// <item>Create ValueObjectModelBinder&lt;T&gt; using the model type as T</item>
    /// </list>
    /// </para>
    /// <para>
    /// The reflection overhead is minimal as:
    /// <list type="bullet">
    /// <item>This method is called once per parameter type during application startup</item>
    /// <item>ASP.NET Core caches the model binder instances</item>
    /// <item>No reflection occurs during request processing</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// Automatic selection:
    /// <code>
    /// // FirstName implements ITryCreatable&lt;FirstName&gt;
    /// // Provider detects this and returns ValueObjectModelBinder&lt;FirstName&gt;
    /// [HttpPost]
    /// public ActionResult Create(FirstName firstName) { }
    /// 
    /// // string doesn't implement ITryCreatable
    /// // Provider returns null, default string binder is used
    /// [HttpGet]
    /// public ActionResult Get(string id) { }
    /// </code>
    /// </example>
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Model types are registered with MVC at compile time")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Model binding is resolved at startup, not in AOT-compiled paths")]
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var modelType = context.Metadata.ModelType;

        // Check if the type implements ITryCreatable<T>
        var tryCreatableInterface = modelType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType &&
                                 i.GetGenericTypeDefinition() == typeof(ITryCreatable<>));

        if (tryCreatableInterface == null)
            return null;

        // Create ValueObjectModelBinder<T> where T is the model type
        var binderType = typeof(ValueObjectModelBinder<>).MakeGenericType(modelType);
        var binder = (IModelBinder?)Activator.CreateInstance(binderType);

        return binder;
    }
}
