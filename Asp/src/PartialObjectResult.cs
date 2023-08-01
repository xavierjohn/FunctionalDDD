namespace FunctionalDDD;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

/// The Content-Range response HTTP header indicates where in a full body message a partial message belongs.
/// Content-Range: <unit> <range-start>-<range-end>/<size>
/// Content-Range: <unit> <range-start>-<range-end>/*
/// Content-Range: <unit> */<size>
/// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Range
/// Common units are bytes, items or seconds.
public class PartialObjectResult : ObjectResult
{
    private readonly string _contentRangeHeaderValue;

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
        _contentRangeHeaderValue = (new ContentRangeHeaderValue(from, to, totalLength) { Unit = "items" }).ToString();
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
        _contentRangeHeaderValue = contentRangeHeaderValue.ToString();
        StatusCode = StatusCodes.Status206PartialContent;
    }

    /// <inheritdoc />
    public override void OnFormatting(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        base.OnFormatting(context);

        context.HttpContext.Response.Headers[HeaderNames.ContentRange] = _contentRangeHeaderValue;
    }
}
