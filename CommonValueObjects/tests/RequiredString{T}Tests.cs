namespace CommonValueObjects.Tests;
using FunctionalDDD.CommonValueObjects;
using FunctionalDDD;
using Xunit;

public partial class TrackingId : RequiredString<TrackingId>
{
}

public class RequiredString_T_Tests
{
    [Fact]
    public void Cannot_create_empty_RequiredString()
    {
        var trackingId1 = TrackingId.Create("");
        trackingId1.IsFailure.Should().BeTrue();
        trackingId1.Errors.Should().HaveCount(1);
        trackingId1.Error.Message.Should().Be("Tracking Id cannot be empty");
        trackingId1.Error.Code.Should().Be("trackingId");
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

    [Fact]
    public void Can_implicitly_cast_to_string()
    {
        // Arrange
        TrackingId trackingId1 = TrackingId.Create("32141sd").Value;

        // Act
        string strTracking = trackingId1;

        // Assert
        strTracking.Should().Be("32141sd");
    }

    [Fact]
    public void Can_explictly_cast_to_RequiredString()
    {
        // Arrange

        // Act
        TrackingId trackingId1 = (TrackingId)"32141sd";

        // Assert
        trackingId1.Should().Be(TrackingId.Create("32141sd").Value);
    }

    [Fact]
    public void Cannot_cast_empty_to_RequiredString()
    {
        // Arrange
        TrackingId trackingId;
        // Act
        Action act = () => trackingId = (TrackingId)string.Empty;

        // Assert
        act.Should().Throw<ResultFailureException>()
            .WithMessage("You attempted to access the Value property for a failed result. A failed result has no Value.")
            .Where(e => e.Errors[0].Message == "Tracking Id cannot be empty");
    }
}
