namespace Asp.Tests;

using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

public class TestHttpResponseStreamWriterFactory : IHttpResponseStreamWriterFactory
{
    public const int DefaultBufferSize = 16 * 1024;

    public TextWriter CreateWriter(Stream stream, Encoding encoding) =>
        new HttpResponseStreamWriter(stream, encoding, DefaultBufferSize);
}
