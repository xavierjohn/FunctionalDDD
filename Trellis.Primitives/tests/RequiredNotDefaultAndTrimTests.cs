namespace Trellis.Primitives.Tests;

using System;
using Trellis.Testing;
using Xunit;

// Lenient-default fixtures — no [NotDefault] / [Trim]. Document the new POLA behavior.
public partial class LenientGuid : RequiredGuid<LenientGuid> { }
public partial class LenientDateTime : RequiredDateTime<LenientDateTime> { }
public partial class LenientString : RequiredString<LenientString> { }
public partial class LenientInt : RequiredInt<LenientInt> { }
public partial class LenientLong : RequiredLong<LenientLong> { }
public partial class LenientDecimal : RequiredDecimal<LenientDecimal> { }

// Strict (opt-in) fixtures with [NotDefault] / [Trim].
[NotDefault] public partial class StrictGuid : RequiredGuid<StrictGuid> { }
[NotDefault] public partial class StrictDateTime : RequiredDateTime<StrictDateTime> { }
[NotDefault] public partial class StrictInt : RequiredInt<StrictInt> { }
[NotDefault] public partial class StrictLong : RequiredLong<StrictLong> { }
[NotDefault] public partial class StrictDecimal : RequiredDecimal<StrictDecimal> { }

[NotDefault] public partial class NoDefaultOnlyString : RequiredString<NoDefaultOnlyString> { }
[Trim] public partial class TrimOnlyString : RequiredString<TrimOnlyString> { }
[Trim, NotDefault] public partial class TrimAndNotDefaultString : RequiredString<TrimAndNotDefaultString> { }

/// <summary>
/// POLA realignment behavior tests: undecorated RequiredXxx types reject only null;
/// per-type sentinel rejection is opt-in via [NotDefault]. String trim is opt-in via [Trim].
/// </summary>
public class RequiredNotDefaultAndTrimTests
{
    // ---------- Lenient defaults: reject only null ----------

    [Fact]
    public void LenientGuid_accepts_Guid_Empty()
    {
        var result = LenientGuid.TryCreate(Guid.Empty);
        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Value.Should().Be(Guid.Empty);
    }

    [Fact]
    public void LenientGuid_accepts_parsed_all_zero_string()
    {
        var result = LenientGuid.TryCreate("00000000-0000-0000-0000-000000000000");
        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Value.Should().Be(Guid.Empty);
    }

    [Fact]
    public void LenientDateTime_accepts_MinValue()
    {
        var result = LenientDateTime.TryCreate(DateTime.MinValue);
        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Value.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void LenientString_accepts_empty()
    {
        var result = LenientString.TryCreate("");
        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Value.Should().Be("");
    }

    [Fact]
    public void LenientString_accepts_whitespace_verbatim_without_Trim()
    {
        var result = LenientString.TryCreate("  hi  ");
        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Value.Should().Be("  hi  ");
    }

    [Fact]
    public void LenientInt_accepts_zero() => LenientInt.TryCreate(0).IsSuccess.Should().BeTrue();

    [Fact]
    public void LenientLong_accepts_zero() => LenientLong.TryCreate(0L).IsSuccess.Should().BeTrue();

    [Fact]
    public void LenientDecimal_accepts_zero() => LenientDecimal.TryCreate(0m).IsSuccess.Should().BeTrue();

    // All lenient types still reject null
    [Fact]
    public void LenientGuid_rejects_null() => LenientGuid.TryCreate((Guid?)null).IsFailure.Should().BeTrue();

    [Fact]
    public void LenientInt_rejects_null() => LenientInt.TryCreate((int?)null).IsFailure.Should().BeTrue();

    [Fact]
    public void LenientString_rejects_null() => LenientString.TryCreate((string?)null).IsFailure.Should().BeTrue();

    // ---------- [NotDefault] opt-in: reject the per-type zero value ----------

    [Fact]
    public void StrictGuid_rejects_Guid_Empty_with_per_type_message()
    {
        var result = StrictGuid.TryCreate(Guid.Empty);
        result.IsFailure.Should().BeTrue();
        var ve = (Error.UnprocessableContent)result.UnwrapError();
        ve.Fields[0].Detail.Should().Be("Strict Guid cannot be Guid.Empty.");
    }

    [Fact]
    public void StrictGuid_rejects_parsed_all_zero_string_with_per_type_message()
    {
        // Critical regression test for the parse-overload sentinel disambiguation:
        // "00000000-..." parses successfully, then the [NotDefault] ensure fires
        // with the per-type "cannot be Guid.Empty." message — not the parse-failure
        // "Guid should contain 32 digits..." message.
        var result = StrictGuid.TryCreate("00000000-0000-0000-0000-000000000000");
        result.IsFailure.Should().BeTrue();
        var ve = (Error.UnprocessableContent)result.UnwrapError();
        ve.Fields[0].Detail.Should().Be("Strict Guid cannot be Guid.Empty.");
    }

    [Fact]
    public void StrictDateTime_rejects_MinValue_with_per_type_message()
    {
        var result = StrictDateTime.TryCreate(DateTime.MinValue);
        result.IsFailure.Should().BeTrue();
        var ve = (Error.UnprocessableContent)result.UnwrapError();
        ve.Fields[0].Detail.Should().Be("Strict Date Time cannot be DateTime.MinValue.");
    }

    [Fact]
    public void StrictInt_rejects_zero_with_per_type_message()
    {
        var result = StrictInt.TryCreate(0);
        result.IsFailure.Should().BeTrue();
        var ve = (Error.UnprocessableContent)result.UnwrapError();
        ve.Fields[0].Detail.Should().Be("Strict Int cannot be zero.");
    }

    [Fact]
    public void StrictLong_rejects_zero_with_per_type_message()
    {
        var result = StrictLong.TryCreate(0L);
        result.IsFailure.Should().BeTrue();
        var ve = (Error.UnprocessableContent)result.UnwrapError();
        ve.Fields[0].Detail.Should().Be("Strict Long cannot be zero.");
    }

    [Fact]
    public void StrictDecimal_rejects_zero_with_per_type_message()
    {
        var result = StrictDecimal.TryCreate(0m);
        result.IsFailure.Should().BeTrue();
        var ve = (Error.UnprocessableContent)result.UnwrapError();
        ve.Fields[0].Detail.Should().Be("Strict Decimal cannot be zero.");
    }

    // Strict types still accept non-zero values
    [Fact]
    public void StrictGuid_accepts_non_empty() => StrictGuid.TryCreate(Guid.NewGuid()).IsSuccess.Should().BeTrue();

    [Fact]
    public void StrictInt_accepts_non_zero() => StrictInt.TryCreate(42).IsSuccess.Should().BeTrue();

    // ---------- [Trim] / [NotDefault] string combinations ----------

    [Fact]
    public void NoDefaultOnlyString_rejects_empty_but_keeps_whitespace()
    {
        // [NotDefault] alone rejects exact "" but stores "   " verbatim because [Trim] is not applied.
        NoDefaultOnlyString.TryCreate("").IsFailure.Should().BeTrue();
        var ws = NoDefaultOnlyString.TryCreate("   ");
        ws.IsSuccess.Should().BeTrue();
        ws.Unwrap().Value.Should().Be("   ");
    }

    [Fact]
    public void TrimOnlyString_trims_but_accepts_resulting_empty()
    {
        // [Trim] alone trims and accepts the (possibly empty) result.
        var ws = TrimOnlyString.TryCreate("   ");
        ws.IsSuccess.Should().BeTrue();
        ws.Unwrap().Value.Should().Be("");
        TrimOnlyString.TryCreate("  hello  ").Unwrap().Value.Should().Be("hello");
    }

    [Fact]
    public void TrimAndNotDefaultString_trims_then_rejects_empty()
    {
        // Combined behavior recovers the pre-realignment RequiredString defaults.
        TrimAndNotDefaultString.TryCreate("   ").IsFailure.Should().BeTrue();
        TrimAndNotDefaultString.TryCreate("").IsFailure.Should().BeTrue();
        TrimAndNotDefaultString.TryCreate("  John  ").Unwrap().Value.Should().Be("John");
    }
}
