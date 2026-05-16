namespace Trellis.Asp.ModelBinding;

using System;
using System.Collections.Frozen;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Trellis;

/// <summary>
/// Model binder for <c>Maybe&lt;T&gt;</c> parameters where <typeparamref name="T"/> is an
/// STJ-native primitive in the closed whitelist
/// (<see cref="SupportedPrimitives"/>). Binds optional route, query, form, and header
/// parameters into <see cref="Maybe{T}"/>.
/// </summary>
/// <typeparam name="T">
/// One of: <see cref="string"/>, <see cref="decimal"/>, <see cref="int"/>, <see cref="long"/>,
/// <see cref="short"/>, <see cref="byte"/>, <see cref="double"/>, <see cref="float"/>,
/// <see cref="bool"/>, <see cref="System.Guid"/>, <see cref="System.DateTime"/>,
/// <see cref="System.DateTimeOffset"/>.
/// </typeparam>
/// <remarks>
/// <list type="bullet">
///   <item>Parameter absent or empty → <c>Maybe&lt;T&gt;.None</c> (success, value not provided).</item>
///   <item>Parameter present and parsable as <typeparamref name="T"/> → <c>Maybe.From(parsedValue)</c>.</item>
///   <item>Parameter present but unparsable → model-state error; result is <see cref="ModelBindingResult.Failed"/>.</item>
/// </list>
/// <para>
/// Closes the binder-side parity gap with <c>MaybeScalarValueJsonConverterFactory</c>: route /
/// query / header parameters typed <c>Maybe&lt;long&gt;</c> / <c>Maybe&lt;string&gt;</c> / etc.
/// now bind directly without a wire-shape DTO + adapter at the seam.
/// </para>
/// </remarks>
public sealed class MaybePrimitiveModelBinder<T> : IModelBinder
    where T : notnull
{
    /// <summary>
    /// The closed set of primitive types this binder supports, mirroring
    /// <see cref="Trellis.Asp.Validation.MaybePrimitiveJsonConverterFactory"/>'s whitelist.
    /// </summary>
    public static readonly FrozenSet<Type> SupportedPrimitives = new HashSet<Type>
    {
        typeof(string),
        typeof(decimal),
        typeof(int),
        typeof(long),
        typeof(short),
        typeof(byte),
        typeof(double),
        typeof(float),
        typeof(bool),
        typeof(Guid),
        typeof(DateTime),
        typeof(DateTimeOffset),
    }.ToFrozenSet();

    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var providerResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (providerResult == ValueProviderResult.None)
        {
            bindingContext.Result = ModelBindingResult.Success(Maybe<T>.None);
            return Task.CompletedTask;
        }

        var raw = providerResult.FirstValue;
        if (string.IsNullOrEmpty(raw))
        {
            bindingContext.Result = ModelBindingResult.Success(Maybe<T>.None);
            return Task.CompletedTask;
        }

        if (TryParse(raw, out var value))
        {
            bindingContext.Result = ModelBindingResult.Success(Maybe.From(value));
            return Task.CompletedTask;
        }

        bindingContext.ModelState.TryAddModelError(
            bindingContext.ModelName,
            $"The value '{raw}' is not valid for {typeof(T).Name}.");
        bindingContext.Result = ModelBindingResult.Failed();
        return Task.CompletedTask;
    }

    private static bool TryParse(string raw, out T value)
    {
        // Dispatch on the closed primitive whitelist via typed parse methods. Same shape
        // as the JSON converter's read path — no reflection, no JsonSerializer, AOT-safe.
        if (typeof(T) == typeof(string))
        {
            value = (T)(object)raw;
            return true;
        }

        if (typeof(T) == typeof(int) && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
        {
            value = (T)(object)i;
            return true;
        }

        if (typeof(T) == typeof(long) && long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l))
        {
            value = (T)(object)l;
            return true;
        }

        if (typeof(T) == typeof(short) && short.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var s))
        {
            value = (T)(object)s;
            return true;
        }

        if (typeof(T) == typeof(byte) && byte.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var b))
        {
            value = (T)(object)b;
            return true;
        }

        if (typeof(T) == typeof(decimal) && decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var dec))
        {
            value = (T)(object)dec;
            return true;
        }

        if (typeof(T) == typeof(double) && double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var dbl))
        {
            value = (T)(object)dbl;
            return true;
        }

        if (typeof(T) == typeof(float) && float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
        {
            value = (T)(object)f;
            return true;
        }

        if (typeof(T) == typeof(bool) && bool.TryParse(raw, out var bo))
        {
            value = (T)(object)bo;
            return true;
        }

        if (typeof(T) == typeof(Guid) && Guid.TryParse(raw, CultureInfo.InvariantCulture, out var g))
        {
            value = (T)(object)g;
            return true;
        }

        if (typeof(T) == typeof(DateTime) && DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
        {
            value = (T)(object)dt;
            return true;
        }

        if (typeof(T) == typeof(DateTimeOffset) && DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
        {
            value = (T)(object)dto;
            return true;
        }

        value = default!;
        return false;
    }
}
