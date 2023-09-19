namespace FunctionalDDD.Results.Asp;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

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
    /// <param name="from">From index.</param>
    /// <param name="to">To index.</param>
    /// <param name="totalLength">Total number of items.</param>
    /// <param name="value">Items</param>
    public PartialObjectResult(long from, long to, long totalLength, object? value)
        : base(value)
    {
        _contentRangeHeaderValue = new ContentRangeHeaderValue(from, to, totalLength) { Unit = "items" };
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

        context.HttpContext.Response.Headers[HeaderNames.ContentRange] = _contentRangeHeaderValue.ToString();
    }
}
