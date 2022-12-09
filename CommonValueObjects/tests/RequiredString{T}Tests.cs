using FunctionalDDD.CommonValueObjects;
using FunctionalDDD.Core;

namespace CommonValueObjects.Tests;

public class TrackingId : RequiredString<TrackingId>
{
    private TrackingId(string value) : base(value)
    {
    }

    public static explicit operator TrackingId(string trackingId)
    {
        return Create(trackingId).Value;
    }

    public static implicit operator string(TrackingId trackingId)
    {
        return trackingId.Value;
    }
}

public class RequiredString_T_Tests
{
    [Fact]
    public void Cannot_create_empty_RequiredString()
    {
        var trackingId1 = TrackingId.Create("");
        trackingId1.IsFailure.Should().BeTrue();
        trackingId1.Errors.Should().HaveCount(1);
        trackingId1.Error.Message.Should().Be("TrackingId cannot be empty");
        trackingId1.Error.Code.Should().Be("TrackingId");
    }

    [Fact]
    public void Can_create_RequiredString()
    {
        var trackingId1 = TrackingId.Create("32141sd");
        trackingId1.IsSuccess.Should().BeTrue();
        trackingId1.Value.Should().BeOfType<TrackingId>();
        trackingId1.Value.Value.Should().Be("32141sd");
    }

    [Fact]
    public void Two_RequiredString_with_different_value_should_be__not_equal()
    {
        var rTrackingIds = TrackingId.Create("Value1")
            .Combine(TrackingId.Create("Value2"));

        rTrackingIds.IsSuccess.Should().BeTrue();
        (var trackingId1, var trackingId2) = rTrackingIds.Value;
        trackingId1.Value.Should().NotBe(trackingId2.Value);
    }

    [Fact]
    public void Two_RequiredString_with_same_value_should_be_equal()
    {
        var rTrackingIds = TrackingId.Create("SameValue")
            .Combine(TrackingId.Create("SameValue"));

        rTrackingIds.IsSuccess.Should().BeTrue();
        (var trackingId1, var trackingId2) = rTrackingIds.Value;
        trackingId1.Value.Should().Be(trackingId2.Value);
    }
}
