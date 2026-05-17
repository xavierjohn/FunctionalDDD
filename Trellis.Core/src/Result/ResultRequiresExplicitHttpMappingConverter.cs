namespace Trellis;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Default <see cref="JsonConverterFactory"/> attached to <see cref="Result{TValue}"/>,
/// <see cref="IResult"/>, and <see cref="IResult{TValue}"/> that fails fast when a result is
/// JSON-serialized without going through Trellis's HTTP-mapping path
/// (<c>Result&lt;T&gt;.ToHttpResponse()</c> in <c>Trellis.Asp</c>) or an explicit consumer
/// converter. Directly serializing <see cref="Result{TValue}"/> would emit
/// <c>{"IsSuccess": bool, "Value": …, "Error": …}</c> — a struct dump, not a stable wire shape
/// and not the HTTP problem-details / status-code mapping consumers expect from Trellis.
/// </summary>
/// <remarks>
/// <para>
/// The factory matches <see cref="Result{TValue}"/> AND the public result interfaces
/// (<see cref="IResult"/>, <see cref="IResult{TValue}"/>). The attribute lives on both the
/// struct and the interfaces because <c>[JsonConverter]</c> resolution is by static declared
/// type — a controller signature like <c>Task&lt;IResult&lt;int&gt;&gt; GetAsync()</c> picks
/// up the interface attribute, not the struct attribute (the runtime <c>Result&lt;int&gt;</c>
/// is never consulted for converter resolution).
/// </para>
/// <para>
/// The intended controller / minimal-API pattern is to call <c>.ToHttpResponse()</c> on the
/// result, which returns a <c>Microsoft.AspNetCore.Http.IResult</c> that writes the response
/// body itself (so the <see cref="Result{TValue}"/> struct is never serialized by STJ).
/// Returning a raw <see cref="Result{TValue}"/> from a controller action — and letting MVC's
/// default object formatter pick it up — used to silently produce the struct-dump JSON shape
/// with no HTTP status-code mapping for <see cref="Error"/> cases. With this converter
/// attached, that misuse throws <see cref="NotSupportedException"/> on the first request with
/// an actionable message naming <c>.ToHttpResponse()</c>.
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
    /// <summary>
    /// Returns <see langword="true"/> for <see cref="Result{TValue}"/> and
    /// <see cref="IResult{TValue}"/> shapes, and for the non-generic <see cref="IResult"/>
    /// interface.
    /// </summary>
    public override bool CanConvert(Type typeToConvert)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);

        if (typeToConvert == typeof(IResult))
            return true;

        if (!typeToConvert.IsGenericType)
            return false;

        var def = typeToConvert.GetGenericTypeDefinition();
        return def == typeof(Result<>) || def == typeof(IResult<>);
    }

    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage(
        "Trimming", "IL2055",
        Justification = "T comes from Result<T> / IResult<T> the runtime is asking us to convert; the closed converter type is constructable from that T by definition.")]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage(
        "AOT", "IL3050",
        Justification = "The throwing converter has no AOT-incompatible dependencies; the MakeGenericType / Activator path is here only to surface the actionable error at the first STJ call attempt.")]
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);

        if (typeToConvert == typeof(IResult))
            return new NonGenericThrowingConverter();

        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(ThrowingConverter<>).MakeGenericType(valueType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private sealed class ThrowingConverter<TValue> : JsonConverter<Result<TValue>>
    {
        // CanConvert override: the factory advertises Result<T>, IResult<T>, AND IResult,
        // so STJ may ask this typed converter to handle an IResult<T>-declared property too.
        // Allow that — the throw path is identical and Read/Write are gated on assignability.
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert == typeof(Result<TValue>)
            || typeToConvert == typeof(IResult<TValue>);

        public override Result<TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotSupportedException(BuildMessage(typeof(TValue).Name, reading: true));

        public override void Write(Utf8JsonWriter writer, Result<TValue> value, JsonSerializerOptions options) =>
            throw new NotSupportedException(BuildMessage(typeof(TValue).Name, reading: false));
    }

    private sealed class NonGenericThrowingConverter : JsonConverter<IResult>
    {
        public override IResult Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotSupportedException(BuildMessage("?", reading: true));

        public override void Write(Utf8JsonWriter writer, IResult value, JsonSerializerOptions options) =>
            throw new NotSupportedException(BuildMessage("?", reading: false));
    }

    private static string BuildMessage(string valueTypeName, bool reading) =>
        $"Result<{valueTypeName}> / IResult cannot be {(reading ? "deserialized from" : "serialized to")} JSON directly. " +
        "Result<T> is a domain disposition — call .ToHttpResponse() (Trellis.Asp) on a controller / minimal-API " +
        "return to map success → 200/201/204 and Error.* → 4xx/5xx with problem-details, or unwrap the value " +
        "via Match / TryGetValue before serialization. " +
        "If you intentionally want to JSON-serialize a Result<T> (e.g. for logging or IPC), register a custom " +
        "JsonConverter<Result<T>> in JsonSerializerOptions.Converters — converters there take " +
        "precedence over the type's default [JsonConverter] attribute.";
}
