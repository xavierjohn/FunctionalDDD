namespace FunctionalDDD.Results.Asp;

using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Represents an <see cref="ObjectResult"/> that returns a <see cref="StatusCodes.Status206PartialContent"/> response
/// and add a [Content-Range](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Range) header to the response.
/// </summary>
public class PartialObjectResult : ObjectResult
{
    private readonly ContentRangeHeaderValue _contentRangeHeaderValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartialObjectResult"/> class.
    /// </summary>
    /// <param name="rangeStart">An integer in the given unit indicating the start position (zero-indexed & inclusive) of the request range.</param>
    /// <param name="rangeEnd">An integer in the given unit indicating the end position (zero-indexed & inclusive) of the requested range.</param>
    /// <param name="totalLength">Optional total number of items.</param>
    /// <param name="value">Items</param>
    public PartialObjectResult(long rangeStart, long rangeEnd, long? totalLength, object? value)
        : base(value)
    {
        _contentRangeHeaderValue = totalLength == null
            ? new ContentRangeHeaderValue(rangeStart, rangeEnd) { Unit = "items" }
            : new ContentRangeHeaderValue(rangeStart, rangeEnd, totalLength.Value) { Unit = "items" };
        StatusCode = StatusCodes.Status206PartialContent;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PartialObjectResult"/> class.
    /// </summary>
    /// <param name="contentRangeHeaderValue">Content range header value.</param>
    /// <param name="value">Items</param>
    public PartialObjectResult(ContentRangeHeaderValue contentRangeHeaderValue, object? value)
        : base(value)
    {
        _contentRangeHeaderValue = contentRangeHeaderValue;
        StatusCode = StatusCodes.Status206PartialContent;
    }

    public ContentRangeHeaderValue ContentRangeHeaderValue { get => _contentRangeHeaderValue; }

    /// <inheritdoc />
    public override void OnFormatting(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        base.OnFormatting(context);

        context.HttpContext.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.ContentRange] = _contentRangeHeaderValue.ToString();
    }
}
