namespace FunctionalDDD.Results.Asp;

using Microsoft.Net.Http.Headers;

/// <summary>
/// Holds the <see cref="ContentRangeHeaderValue"/> and the data.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public readonly struct ContentRangeAndData<TValue>
{
    public ContentRangeAndData(ContentRangeHeaderValue contentRangeHeaderValue, TValue data)
    {
        ContentRangeHeaderValue = contentRangeHeaderValue;
        Data = data;
    }

    public ContentRangeHeaderValue ContentRangeHeaderValue { get; }
    public TValue Data { get; }
}
