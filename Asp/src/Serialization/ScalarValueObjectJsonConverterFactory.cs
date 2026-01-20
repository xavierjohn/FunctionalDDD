namespace FunctionalDdd.Asp.Serialization;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using FunctionalDdd;

/// <summary>
/// Factory for creating JSON converters for ScalarValueObject-derived types.
/// </summary>
/// <remarks>
/// <para>
/// This factory is registered with <see cref="JsonSerializerOptions"/> and automatically
/// creates <see cref="ScalarValueObjectJsonConverter{TValueObject, TPrimitive}"/> instances
/// for any type implementing <see cref="IScalarValueObject{TSelf, TPrimitive}"/>.
/// </para>
/// <para>
/// Register using <c>AddScalarValueObjectValidation()</c> extension method.
/// </para>
/// </remarks>
public class ScalarValueObjectJsonConverterFactory : JsonConverterFactory
{
    /// <summary>
        /// Determines whether this factory can create a converter for the specified type.
        /// </summary>
        /// <param name="typeToConvert">The type to check.</param>
        /// <returns><c>true</c> if the type implements <see cref="IScalarValueObject{TSelf, TPrimitive}"/>.</returns>
        public override bool CanConvert(Type typeToConvert) =>
            GetScalarValueObjectInterface(typeToConvert) is not null;

        /// <summary>
        /// Creates a converter for the specified value object type.
        /// </summary>
        /// <param name="typeToConvert">The value object type.</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>A JSON converter for the value object type.</returns>
    #pragma warning disable IL3050 // Uses MakeGenericType which is not AOT compatible
    #pragma warning disable IL2070 // GetInterfaces requires DynamicallyAccessedMembers
        public override JsonConverter? CreateConverter(
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var valueObjectInterface = GetScalarValueObjectInterface(typeToConvert);
            if (valueObjectInterface is null)
                return null;

            var primitiveType = valueObjectInterface.GetGenericArguments()[1];

            var converterType = typeof(ScalarValueObjectJsonConverter<,>)
                .MakeGenericType(typeToConvert, primitiveType);

            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }
    #pragma warning restore IL2070
    #pragma warning restore IL3050

    #pragma warning disable IL2070 // GetInterfaces requires DynamicallyAccessedMembers
        private static Type? GetScalarValueObjectInterface(Type typeToConvert) =>
            typeToConvert
                .GetInterfaces()
                .FirstOrDefault(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IScalarValueObject<,>) &&
                    i.GetGenericArguments()[0] == typeToConvert);
    #pragma warning restore IL2070
    }
