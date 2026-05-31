namespace Trellis.Asp.Tests.Idempotency;

using Trellis.Asp.Idempotency;

/// <summary>
/// Pins the subset of RFC 8941 structured-fields-string parsing used by the idempotency
/// middleware. Keys may arrive as a bare RFC 7230 token or as a quoted string with the
/// limited escape set (<c>\\</c> and <c>\"</c>).
/// </summary>
public sealed class IdempotencyKeyParserTests
{
    [Theory]
    [InlineData("abc")]
    [InlineData("9d6f6c44-1234-5678-9abc-def012345678")]
    [InlineData("a.b_c-d~e!f#g$h%i&j'k*l+m^n`o|p")]
    public void Bare_token_is_returned_verbatim(string token)
    {
        IdempotencyKeyParser.TryParse(token, out var parsed, out _).Should().BeTrue();
        parsed.Should().Be(token);
    }

    [Fact]
    public void Quoted_string_returns_content_without_quotes()
    {
        IdempotencyKeyParser.TryParse("\"hello world\"", out var parsed, out _).Should().BeTrue();
        parsed.Should().Be("hello world");
    }

    [Fact]
    public void Quoted_string_with_escaped_backslash_and_quote_is_unescaped()
    {
        IdempotencyKeyParser.TryParse("\"a\\\"b\\\\c\"", out var parsed, out _).Should().BeTrue();
        parsed.Should().Be("a\"b\\c");
    }

    [Theory]
    [InlineData("")]
    [InlineData("\"unterminated")]
    [InlineData("with space outside quotes")]
    [InlineData("ünicode")]
    [InlineData("\"\\x\"")]
    public void Invalid_inputs_return_false(string input)
    {
        IdempotencyKeyParser.TryParse(input, out _, out var error).Should().BeFalse();
        error.Should().NotBeNullOrEmpty();
    }
}
