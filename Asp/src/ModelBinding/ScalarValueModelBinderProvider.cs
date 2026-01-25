namespace FunctionalDdd.Asp.ModelBinding;

using FunctionalDdd;
using FunctionalDdd.Asp.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Detects scalar value types and provides model binders for them.
/// </summary>
/// <remarks>
/// <para>
/// This provider checks if a model type implements <see cref="IScalarValue{TSelf, TPrimitive}"/>
/// and creates an appropriate <see cref="ScalarValueModelBinder{TValueObject, TPrimitive}"/> for it.
/// </para>
/// <para>
/// Register this provider using <c>AddScalarValueValidation()</c> extension method
/// on <c>IMvcBuilder</c>.
/// </para>
/// </remarks>
/// <example>
/// Registration in Program.cs:
/// <code>
/// builder.Services
///     .AddControllers()
///     .AddScalarValueValidation();
/// </code>
/// </example>
public class ScalarValueModelBinderProvider : IModelBinderProvider
{
    /// <summary>
    /// Returns a model binder for scalar value types, or null for other types.
    /// </summary>
    /// <param name="context">The model binder provider context.</param>
    /// <returns>A model binder for the type, or null if not applicable.</returns>
    /// <remarks>
    /// This method uses reflection to detect value object types and create binders dynamically.
    /// It is not compatible with Native AOT scenarios.
    /// </remarks>
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Value object types are preserved by model binding infrastructure")]
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Value object types are preserved by model binding infrastructure")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Value object types are preserved by model binding infrastructure")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Model binding is not compatible with Native AOT")]
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var modelType = context.Metadata.ModelType;
        var primitiveType = ScalarValueTypeHelper.GetPrimitiveType(modelType);

        return primitiveType is null
            ? null
            : ScalarValueTypeHelper.CreateGenericInstance<IModelBinder>(
                typeof(ScalarValueModelBinder<,>),
                modelType,
                primitiveType);
    }
}