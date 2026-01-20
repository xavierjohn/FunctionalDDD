namespace FunctionalDdd.Asp.ModelBinding;

using System.Diagnostics.CodeAnalysis;
using FunctionalDdd;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Detects ScalarValueObject-derived types and provides model binders for them.
/// </summary>
/// <remarks>
/// <para>
/// This provider checks if a model type implements <see cref="IScalarValueObject{TSelf, TPrimitive}"/>
/// and creates an appropriate <see cref="ScalarValueObjectModelBinder{TValueObject, TPrimitive}"/> for it.
/// </para>
/// <para>
/// Register this provider using <c>AddScalarValueObjectValidation()</c> extension method
/// on <see cref="IMvcBuilder"/>.
/// </para>
/// </remarks>
/// <example>
/// Registration in Program.cs:
/// <code>
/// builder.Services
///     .AddControllers()
///     .AddScalarValueObjectValidation();
/// </code>
/// </example>
public class ScalarValueObjectModelBinderProvider : IModelBinderProvider
{
    /// <summary>
        /// Returns a model binder for ScalarValueObject types, or null for other types.
        /// </summary>
        /// <param name="context">The model binder provider context.</param>
        /// <returns>A model binder for the type, or null if not applicable.</returns>
    #pragma warning disable IL3050 // Uses MakeGenericType which is not AOT compatible
    #pragma warning disable IL2075 // GetInterfaces requires DynamicallyAccessedMembers
    #pragma warning disable IL2070 // GetInterfaces requires DynamicallyAccessedMembers
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var modelType = context.Metadata.ModelType;

            // Check if implements IScalarValueObject<TSelf, TPrimitive>
            var valueObjectInterface = GetScalarValueObjectInterface(modelType);

            if (valueObjectInterface is null)
                return null;

            var primitiveType = valueObjectInterface.GetGenericArguments()[1];

            var binderType = typeof(ScalarValueObjectModelBinder<,>)
                .MakeGenericType(modelType, primitiveType);

            return (IModelBinder)Activator.CreateInstance(binderType)!;
                }

            #pragma warning disable IL2070 // GetInterfaces requires DynamicallyAccessedMembers
                private static Type? GetScalarValueObjectInterface(Type modelType) =>
                    modelType
                        .GetInterfaces()
                        .FirstOrDefault(i =>
                            i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IScalarValueObject<,>) &&
                            i.GetGenericArguments()[0] == modelType);
            #pragma warning restore IL2070
            #pragma warning restore IL2075
            #pragma warning restore IL3050
            }
