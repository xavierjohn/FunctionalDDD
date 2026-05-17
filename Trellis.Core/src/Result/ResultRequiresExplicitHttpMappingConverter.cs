namespace Trellis;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Default <see cref="JsonConverterFactory"/> attached to <see cref="Result{TValue}"/> that
/// fails fast when a result is JSON-serialized without going through Trellis's HTTP-mapping
/// path (<c>Result&lt;T&gt;.ToHttpResponse()</c> in <c>Trellis.Asp</c>) or an explicit consumer
/// converter. Directly serializing <see cref="Result{TValue}"/> would emit
/// <c>{"IsSuccess": bool, "Value": …, "Error": …}</c> — a struct dump, not a stable wire shape
/// and not the HTTP problem-details / status-code mapping consumers expect from Trellis.
/// </summary>
/// <remarks>
/// <para>
/// The intended controller / minimal-API pattern is to call <c>.ToHttpResponse()</c> on the
/// result, which returns an <see cref="System.IDisposable"/>-friendly
/// <c>Microsoft.AspNetCore.Http.IResult</c> that writes the response body itself (so the
/// <see cref="Result{TValue}"/> struct is never serialized by STJ). Returning a raw
/// <see cref="Result{TValue}"/> from a controller action — and letting MVC's default object
/// formatter pick it up — used to silently produce the struct-dump JSON shape with no HTTP
/// status-code mapping for <see cref="Error"/> cases. With this converter attached, that
/// misuse throws <see cref="InvalidOperationException"/> on the first request with an
/// actionable message naming <c>.ToHttpResponse()</c>.
/// </para>
/// <para>
/// Consumers who legitimately want to serialize <see cref="Result{TValue}"/> for logging,
/// IPC, or storage can register their own <see cref="JsonConverter{T}"/> for
/// <see cref="Result{TValue}"/> in <c>options.Converters</c> — <see cref="JsonConverter"/>
/// instances in <c>options.Converters</c> take precedence over the type's
/// <see cref="JsonConverterAttribute"/>.
/// </para>
/// </remarks>
public sealed class ResultRequiresExplicitHttpMappingConverter : JsonConverterFactory
{
    /// <summary>Returns <see langword="true"/> for <see cref="Result{TValue}"/> shapes.</summary>
    public override bool CanConvert(Type typeToConvert)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        return typeToConvert.IsGenericType
            && typeToConvert.GetGenericTypeDefinition() == typeof(Result<>);
    }

    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage(
        "Trimming", "IL2055",
        Justification = "T comes from Result<T> the runtime is asking us to convert; the closed converter type is constructable from that T by definition.")]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage(
        "AOT", "IL3050",
        Justification = "The throwing converter has no AOT-incompatible dependencies; the MakeGenericType / Activator path is here only to surface the actionable error at the first STJ call attempt.")]
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(ThrowingConverter<>).MakeGenericType(valueType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private sealed class ThrowingConverter<TValue> : JsonConverter<Result<TValue>>
    {
        public override Result<TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new InvalidOperationException(BuildMessage(typeof(TValue), reading: true));

        public override void Write(Utf8JsonWriter writer, Result<TValue> value, JsonSerializerOptions options) =>
            throw new InvalidOperationException(BuildMessage(typeof(TValue), reading: false));

        private static string BuildMessage(Type valueType, bool reading) =>
            $"Result<{valueType.Name}> cannot be {(reading ? "deserialized from" : "serialized to")} JSON directly. " +
            "Result<T> is a domain disposition — call .ToHttpResponse() (Trellis.Asp) on a controller / minimal-API " +
            "return to map success → 200/201/204 and Error.* → 4xx/5xx with problem-details, or unwrap the value " +
            "via Match / TryGetValue before serialization. " +
            "If you intentionally want to JSON-serialize a Result<T> (e.g. for logging or IPC), register a custom " +
            $"JsonConverter<Result<{valueType.Name}>> in JsonSerializerOptions.Converters — converters there take " +
            "precedence over the type's default [JsonConverter] attribute.";
    }
}
