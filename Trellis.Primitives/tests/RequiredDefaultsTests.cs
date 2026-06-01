namespace Trellis.Primitives.Tests;

using System;
using Trellis.Testing;

public partial class DefaultGuid : RequiredGuid<DefaultGuid> { }
[AllowEmpty] public partial class EmptyAllowedGuid : RequiredGuid<EmptyAllowedGuid> { }

public partial class DefaultDateTime : RequiredDateTime<DefaultDateTime> { }
[AllowMinValue] public partial class MinValueAllowedDateTime : RequiredDateTime<MinValueAllowedDateTime> { }

public partial class DefaultDateTimeOffset : RequiredDateTimeOffset<DefaultDateTimeOffset> { }
[AllowMinValue] public partial class MinValueAllowedDateTimeOffset : RequiredDateTimeOffset<MinValueAllowedDateTimeOffset> { }

public partial class DefaultString : RequiredString<DefaultString> { }
[AllowEmpty] public partial class EmptyAllowedString : RequiredString<EmptyAllowedString> { }
[AllowWhitespace] public partial class WhitespaceAllowedString : RequiredString<WhitespaceAllowedString> { }
[NoTrim] public partial class NoTrimString : RequiredString<NoTrimString> { }
[AllowEmpty, AllowWhitespace] public partial class EmptyWhitespaceAllowedString : RequiredString<EmptyWhitespaceAllowedString> { }
[AllowEmpty, NoTrim] public partial class EmptyNoTrimString : RequiredString<EmptyNoTrimString> { }
[AllowWhitespace, NoTrim] public partial class WhitespaceNoTrimString : RequiredString<WhitespaceNoTrimString> { }
[AllowEmpty, AllowWhitespace, NoTrim] public partial class EmptyWhitespaceNoTrimString : RequiredString<EmptyWhitespaceNoTrimString> { }

public partial class DefaultInt : RequiredInt<DefaultInt> { }
[AllowZero] public partial class ZeroAllowedInt : RequiredInt<ZeroAllowedInt> { }

public partial class DefaultLong : RequiredLong<DefaultLong> { }
[AllowZero] public partial class ZeroAllowedLong : RequiredLong<ZeroAllowedLong> { }

public partial class DefaultDecimal : RequiredDecimal<DefaultDecimal> { }
[AllowZero] public partial class ZeroAllowedDecimal : RequiredDecimal<ZeroAllowedDecimal> { }

[Range(1, 100)] public partial class DefaultRangedInt : RequiredInt<DefaultRangedInt> { }
[Range(1L, 100L)] public partial class DefaultRangedLong : RequiredLong<DefaultRangedLong> { }
[Range(1, 100)] public partial class DefaultRangedDecimal : RequiredDecimal<DefaultRangedDecimal> { }

public class RequiredDefaultsTests
{
    [Fact]
    public void RequiredGuid_Default_RejectsGuidEmpty()
    {
        var result = DefaultGuid.TryCreate(Guid.Empty);

        result.IsFailure.Should().BeTrue();
        var validation = (Error.InvalidInput)result.UnwrapError();
        validation.Fields[0].Detail.Should().Be("Default Guid cannot be Guid.Empty.");
    }

    [Fact]
    public void RequiredGuid_Default_RejectsParsedAllZeroString()
    {
        var result = DefaultGuid.TryCreate("00000000-0000-0000-0000-000000000000");

        result.IsFailure.Should().BeTrue();
        var validation = (Error.InvalidInput)result.UnwrapError();
        validation.Fields[0].Detail.Should().Be("Default Guid cannot be Guid.Empty.");
    }

    [Fact]
    public void RequiredGuid_AllowEmpty_AcceptsGuidEmpty()
    {
        var result = EmptyAllowedGuid.TryCreate(Guid.Empty);

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Value.Should().Be(Guid.Empty);
    }

    [Fact]
    public void RequiredDateTime_Default_RejectsMinValue()
    {
        var result = DefaultDateTime.TryCreate(DateTime.MinValue);

        result.IsFailure.Should().BeTrue();
        var validation = (Error.InvalidInput)result.UnwrapError();
        validation.Fields[0].Detail.Should().Be("Default Date Time cannot be DateTime.MinValue.");
    }

    [Fact]
    public void RequiredDateTime_AllowMinValue_AcceptsMinValue()
    {
        var result = MinValueAllowedDateTime.TryCreate(DateTime.MinValue);

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Value.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void RequiredDateTimeOffset_Default_RejectsMinValue()
    {
        var result = DefaultDateTimeOffset.TryCreate(DateTimeOffset.MinValue);

        result.IsFailure.Should().BeTrue();
        var validation = (Error.InvalidInput)result.UnwrapError();
        validation.Fields[0].Detail.Should().Be("Default Date Time Offset cannot be DateTimeOffset.MinValue.");
    }

    [Fact]
    public void RequiredDateTimeOffset_AllowMinValue_AcceptsMinValue()
    {
        var result = MinValueAllowedDateTimeOffset.TryCreate(DateTimeOffset.MinValue);

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Value.Should().Be(DateTimeOffset.MinValue);
    }

    [Fact]
    public void RequiredString_Default_RejectsSentinelsAndTrims()
    {
        AssertStringFailure(DefaultString.TryCreate((string?)null), "Default String cannot be null.");
        AssertStringFailure(DefaultString.TryCreate(""), "Default String cannot be empty.");
        AssertStringFailure(DefaultString.TryCreate("   "), "Default String cannot be whitespace-only.");
        DefaultString.TryCreate(" a ").Unwrap().Value.Should().Be("a");
        DefaultString.TryCreate("a").Unwrap().Value.Should().Be("a");
    }

    [Fact]
    public void RequiredString_AllowEmpty_AcceptsEmptyButRejectsWhitespaceOnly()
    {
        EmptyAllowedString.TryCreate("").Unwrap().Value.Should().Be("");
        AssertStringFailure(EmptyAllowedString.TryCreate("   "), "Empty Allowed String cannot be whitespace-only.");
        EmptyAllowedString.TryCreate(" a ").Unwrap().Value.Should().Be("a");
    }

    [Fact]
    public void RequiredString_AllowWhitespace_AcceptsWhitespaceOnlyAsTrimmedEmpty()
    {
        AssertStringFailure(WhitespaceAllowedString.TryCreate(""), "Whitespace Allowed String cannot be empty.");
        WhitespaceAllowedString.TryCreate("   ").Unwrap().Value.Should().Be("");
        WhitespaceAllowedString.TryCreate(" a ").Unwrap().Value.Should().Be("a");
    }

    [Fact]
    public void RequiredString_NoTrim_PreservesPaddingButRejectsSentinels()
    {
        AssertStringFailure(NoTrimString.TryCreate(""), "No Trim String cannot be empty.");
        AssertStringFailure(NoTrimString.TryCreate("   "), "No Trim String cannot be whitespace-only.");
        NoTrimString.TryCreate(" a ").Unwrap().Value.Should().Be(" a ");
    }

    [Fact]
    public void RequiredString_AllowEmptyAndAllowWhitespace_AcceptsBothAsTrimmedEmpty()
    {
        EmptyWhitespaceAllowedString.TryCreate("").Unwrap().Value.Should().Be("");
        EmptyWhitespaceAllowedString.TryCreate("   ").Unwrap().Value.Should().Be("");
        EmptyWhitespaceAllowedString.TryCreate(" a ").Unwrap().Value.Should().Be("a");
    }

    [Fact]
    public void RequiredString_AllowEmptyAndNoTrim_AcceptsEmptyAndPreservesPadding()
    {
        EmptyNoTrimString.TryCreate("").Unwrap().Value.Should().Be("");
        AssertStringFailure(EmptyNoTrimString.TryCreate("   "), "Empty No Trim String cannot be whitespace-only.");
        EmptyNoTrimString.TryCreate(" a ").Unwrap().Value.Should().Be(" a ");
    }

    [Fact]
    public void RequiredString_AllowWhitespaceAndNoTrim_AcceptsWhitespaceAndPreservesPadding()
    {
        AssertStringFailure(WhitespaceNoTrimString.TryCreate(""), "Whitespace No Trim String cannot be empty.");
        WhitespaceNoTrimString.TryCreate("   ").Unwrap().Value.Should().Be("   ");
        WhitespaceNoTrimString.TryCreate(" a ").Unwrap().Value.Should().Be(" a ");
    }

    [Fact]
    public void RequiredString_AllOptOuts_AcceptsAndPreservesSentinelValues()
    {
        EmptyWhitespaceNoTrimString.TryCreate("").Unwrap().Value.Should().Be("");
        EmptyWhitespaceNoTrimString.TryCreate("   ").Unwrap().Value.Should().Be("   ");
        EmptyWhitespaceNoTrimString.TryCreate(" a ").Unwrap().Value.Should().Be(" a ");
    }

    [Fact]
    public void RequiredInt_Default_RejectsZero()
    {
        var result = DefaultInt.TryCreate(0);

        result.IsFailure.Should().BeTrue();
        var validation = (Error.InvalidInput)result.UnwrapError();
        validation.Fields[0].Detail.Should().Be("Default Int cannot be zero.");
    }

    [Fact]
    public void RequiredInt_AllowZero_AcceptsZero()
    {
        var result = ZeroAllowedInt.TryCreate(0);

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Value.Should().Be(0);
    }

    [Fact]
    public void RequiredLong_Default_RejectsZero()
    {
        var result = DefaultLong.TryCreate(0L);

        result.IsFailure.Should().BeTrue();
        var validation = (Error.InvalidInput)result.UnwrapError();
        validation.Fields[0].Detail.Should().Be("Default Long cannot be zero.");
    }

    [Fact]
    public void RequiredLong_AllowZero_AcceptsZero()
    {
        var result = ZeroAllowedLong.TryCreate(0L);

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Value.Should().Be(0L);
    }

    [Fact]
    public void RequiredDecimal_Default_RejectsZero()
    {
        var result = DefaultDecimal.TryCreate(0m);

        result.IsFailure.Should().BeTrue();
        var validation = (Error.InvalidInput)result.UnwrapError();
        validation.Fields[0].Detail.Should().Be("Default Decimal cannot be zero.");
    }

    [Fact]
    public void RequiredDecimal_AllowZero_AcceptsZero()
    {
        var result = ZeroAllowedDecimal.TryCreate(0m);

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Value.Should().Be(0m);
    }

    [Fact]
    public void RequiredInt_WithRange_ZeroSurfacesDefaultMessageBeforeRange()
    {
        var result = DefaultRangedInt.TryCreate(0);

        result.IsFailure.Should().BeTrue();
        var validation = (Error.InvalidInput)result.UnwrapError();
        validation.Fields[0].Detail.Should().Be("Default Ranged Int cannot be zero.");
    }

    [Fact]
    public void RequiredInt_WithRange_NullableZeroSurfacesDefaultMessageBeforeRange()
    {
        var result = DefaultRangedInt.TryCreate((int?)0);

        result.IsFailure.Should().BeTrue();
        var validation = (Error.InvalidInput)result.UnwrapError();
        validation.Fields[0].Detail.Should().Be("Default Ranged Int cannot be zero.");
    }

    [Fact]
    public void RequiredInt_WithRange_StringZeroSurfacesDefaultMessageBeforeRange()
    {
        var result = DefaultRangedInt.TryCreate("0");

        result.IsFailure.Should().BeTrue();
        var validation = (Error.InvalidInput)result.UnwrapError();
        validation.Fields[0].Detail.Should().Be("Default Ranged Int cannot be zero.");
    }

    [Fact]
    public void RequiredLong_WithRange_ZeroSurfacesDefaultMessageBeforeRange()
    {
        var result = DefaultRangedLong.TryCreate(0L);

        result.IsFailure.Should().BeTrue();
        var validation = (Error.InvalidInput)result.UnwrapError();
        validation.Fields[0].Detail.Should().Be("Default Ranged Long cannot be zero.");
    }

    [Fact]
    public void RequiredDecimal_WithRange_ZeroSurfacesDefaultMessageBeforeRange()
    {
        var result = DefaultRangedDecimal.TryCreate(0m);

        result.IsFailure.Should().BeTrue();
        var validation = (Error.InvalidInput)result.UnwrapError();
        validation.Fields[0].Detail.Should().Be("Default Ranged Decimal cannot be zero.");
    }

    [Fact]
    public void RequiredInt_WithRange_NonZeroBelowMinimumStillSurfacesRangeMessage()
    {
        var result = DefaultRangedInt.TryCreate(-5);

        result.IsFailure.Should().BeTrue();
        var validation = (Error.InvalidInput)result.UnwrapError();
        validation.Fields[0].Detail.Should().Be("Default Ranged Int must be at least 1.");
    }

    private static void AssertStringFailure<T>(Result<T> result, string expectedDetail)
    {
        result.IsFailure.Should().BeTrue();
        var validation = (Error.InvalidInput)result.UnwrapError();
        validation.Fields[0].Detail.Should().Be(expectedDetail);
    }
}