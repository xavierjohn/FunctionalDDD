namespace Trellis.Primitives.Tests;

using System;
using Trellis.Testing;
using Xunit;

// Plain RequiredDateTimeOffset — lenient default (only null rejected).
public partial class EventOccurredAt : RequiredDateTimeOffset<EventOccurredAt> { }

// Strict RequiredDateTimeOffset — [NotDefault] rejects DateTimeOffset.MinValue.
[NotDefault]
public partial class StrictOccurredAt : RequiredDateTimeOffset<StrictOccurredAt> { }

// Numeric convenience attributes on the three numeric Required bases.
[Positive] public partial class PositiveInt : RequiredInt<PositiveInt> { }
[NonNegative] public partial class NonNegativeInt : RequiredInt<NonNegativeInt> { }
[Negative] public partial class NegativeInt : RequiredInt<NegativeInt> { }
[NonPositive] public partial class NonPositiveInt : RequiredInt<NonPositiveInt> { }

[Positive] public partial class PositiveLong : RequiredLong<PositiveLong> { }
[NonNegative] public partial class NonNegativeLong : RequiredLong<NonNegativeLong> { }
[Negative] public partial class NegativeLong : RequiredLong<NegativeLong> { }
[NonPositive] public partial class NonPositiveLong : RequiredLong<NonPositiveLong> { }

[Positive] public partial class PositiveDecimal : RequiredDecimal<PositiveDecimal> { }
[NonNegative] public partial class NonNegativeDecimal : RequiredDecimal<NonNegativeDecimal> { }
[Negative] public partial class NegativeDecimal : RequiredDecimal<NegativeDecimal> { }
[NonPositive] public partial class NonPositiveDecimal : RequiredDecimal<NonPositiveDecimal> { }

/// <summary>
/// Tests for the additive Required<T> primitives added in PR2a:
/// the <c>RequiredDateTimeOffset&lt;T&gt;</c> base class and the
/// <c>[Positive]</c> / <c>[NonNegative]</c> / <c>[Negative]</c> / <c>[NonPositive]</c>
/// numeric convenience attributes (which translate to existing <c>[Range]</c> semantics).
/// </summary>
public class RequiredDateTimeOffsetAndNumericConvenienceTests
{
    // ---------- RequiredDateTimeOffset ----------

    [Fact]
    public void EventOccurredAt_accepts_present_value()
    {
        var now = DateTimeOffset.UtcNow;
        var result = EventOccurredAt.TryCreate(now);
        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Value.Should().Be(now);
    }

    [Fact]
    public void EventOccurredAt_accepts_MinValue_when_lenient()
    {
        var result = EventOccurredAt.TryCreate(DateTimeOffset.MinValue);
        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Value.Should().Be(DateTimeOffset.MinValue);
    }

    [Fact]
    public void EventOccurredAt_rejects_null()
    {
        var result = EventOccurredAt.TryCreate((DateTimeOffset?)null);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void EventOccurredAt_round_trips_through_string_parse_preserving_offset()
    {
        var fixedOffset = new DateTimeOffset(2026, 6, 1, 12, 30, 0, TimeSpan.FromHours(-5));
        var parsed = EventOccurredAt.TryCreate(fixedOffset.ToString("O", System.Globalization.CultureInfo.InvariantCulture));
        parsed.IsSuccess.Should().BeTrue();
        parsed.Unwrap().Value.Should().Be(fixedOffset);
        parsed.Unwrap().Value.Offset.Should().Be(TimeSpan.FromHours(-5));
    }

    [Fact]
    public void StrictOccurredAt_rejects_MinValue()
    {
        var result = StrictOccurredAt.TryCreate(DateTimeOffset.MinValue);
        result.IsFailure.Should().BeTrue();
        result.UnwrapError().GetDisplayMessage().Should().Contain("DateTimeOffset.MinValue");
    }

    [Fact]
    public void StrictOccurredAt_accepts_present_value()
    {
        var now = DateTimeOffset.UtcNow;
        var result = StrictOccurredAt.TryCreate(now);
        result.IsSuccess.Should().BeTrue();
    }

    // ---------- [Positive] ----------

    [Fact]
    public void PositiveInt_accepts_positives_rejects_zero_and_negatives()
    {
        PositiveInt.TryCreate(1).IsSuccess.Should().BeTrue();
        PositiveInt.TryCreate(int.MaxValue).IsSuccess.Should().BeTrue();
        PositiveInt.TryCreate(0).IsFailure.Should().BeTrue();
        PositiveInt.TryCreate(-1).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void PositiveLong_accepts_positives_rejects_zero_and_negatives()
    {
        PositiveLong.TryCreate(1L).IsSuccess.Should().BeTrue();
        PositiveLong.TryCreate(0L).IsFailure.Should().BeTrue();
        PositiveLong.TryCreate(-1L).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void PositiveDecimal_accepts_positives_rejects_zero_and_negatives()
    {
        PositiveDecimal.TryCreate(0.01m).IsSuccess.Should().BeTrue();
        PositiveDecimal.TryCreate(0m).IsFailure.Should().BeTrue();
        PositiveDecimal.TryCreate(-0.01m).IsFailure.Should().BeTrue();
    }

    // ---------- [NonNegative] ----------

    [Fact]
    public void NonNegativeInt_accepts_zero_and_positives_rejects_negatives()
    {
        NonNegativeInt.TryCreate(0).IsSuccess.Should().BeTrue();
        NonNegativeInt.TryCreate(1).IsSuccess.Should().BeTrue();
        NonNegativeInt.TryCreate(-1).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void NonNegativeLong_accepts_zero_and_positives_rejects_negatives()
    {
        NonNegativeLong.TryCreate(0L).IsSuccess.Should().BeTrue();
        NonNegativeLong.TryCreate(-1L).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void NonNegativeDecimal_accepts_zero_and_positives_rejects_negatives()
    {
        NonNegativeDecimal.TryCreate(0m).IsSuccess.Should().BeTrue();
        NonNegativeDecimal.TryCreate(0.01m).IsSuccess.Should().BeTrue();
        NonNegativeDecimal.TryCreate(-0.01m).IsFailure.Should().BeTrue();
    }

    // ---------- [Negative] ----------

    [Fact]
    public void NegativeInt_accepts_negatives_rejects_zero_and_positives()
    {
        NegativeInt.TryCreate(-1).IsSuccess.Should().BeTrue();
        NegativeInt.TryCreate(int.MinValue).IsSuccess.Should().BeTrue();
        NegativeInt.TryCreate(0).IsFailure.Should().BeTrue();
        NegativeInt.TryCreate(1).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void NegativeLong_accepts_negatives_rejects_zero_and_positives()
    {
        NegativeLong.TryCreate(-1L).IsSuccess.Should().BeTrue();
        NegativeLong.TryCreate(0L).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void NegativeDecimal_accepts_negatives_rejects_zero_and_positives()
    {
        NegativeDecimal.TryCreate(-0.01m).IsSuccess.Should().BeTrue();
        NegativeDecimal.TryCreate(0m).IsFailure.Should().BeTrue();
    }

    // ---------- [NonPositive] ----------

    [Fact]
    public void NonPositiveInt_accepts_zero_and_negatives_rejects_positives()
    {
        NonPositiveInt.TryCreate(0).IsSuccess.Should().BeTrue();
        NonPositiveInt.TryCreate(-1).IsSuccess.Should().BeTrue();
        NonPositiveInt.TryCreate(1).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void NonPositiveLong_accepts_zero_and_negatives_rejects_positives()
    {
        NonPositiveLong.TryCreate(0L).IsSuccess.Should().BeTrue();
        NonPositiveLong.TryCreate(-1L).IsSuccess.Should().BeTrue();
        NonPositiveLong.TryCreate(1L).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void NonPositiveDecimal_accepts_zero_and_negatives_rejects_positives()
    {
        NonPositiveDecimal.TryCreate(0m).IsSuccess.Should().BeTrue();
        NonPositiveDecimal.TryCreate(-0.01m).IsSuccess.Should().BeTrue();
        NonPositiveDecimal.TryCreate(0.01m).IsFailure.Should().BeTrue();
    }
}
