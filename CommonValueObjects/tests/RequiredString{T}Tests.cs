using FunctionalDDD.CommonValueObjects;

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

    /* TODO    
        [Fact]
        public void Two_RequiredString_with_different_value_should_be__not_equal()
        {
            var trackingId1 = TrackingId.Create("Value1");
            var trackingId2 = TrackingId.Create("Value2");
            var result = Result.Combine(trackingId2, trackingId1);
            Assert.True(result.IsSuccess);
            Assert.NotEqual(trackingId1.Value, trackingId2.Value);
        }

        [Fact]
        public void Two_RequiredString_with_same_value_should_be_equal()
        {
            var trackingId1 = TrackingId.Create("SameValue");
            var trackingId2 = TrackingId.Create("SameValue");
            var result = Result.Combine(trackingId2, trackingId1);
            Assert.True(result.IsSuccess);
            Assert.Equal(trackingId1.Value, trackingId2.Value);
        }
    */
}
