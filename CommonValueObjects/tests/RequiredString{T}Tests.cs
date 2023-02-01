namespace CommonValueObjects.Tests;
using FunctionalDDD;

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
        trackingId1.Error.Should().BeOfType<Validation>();
        var validation = (Validation)trackingId1.Error;
        validation.Description.Should().Be("Tracking Id cannot be empty");
        validation.FieldName.Should().Be("trackingId");
        validation.Code.Should().Be("validation.error");
    }

    [Fact]
    public void Can_create_RequiredString()
    {
        var trackingId1 = TrackingId.Create("32141sd");
        trackingId1.IsSuccess.Should().BeTrue();
        trackingId1.Ok.Should().BeOfType<TrackingId>();
        trackingId1.Ok.Value.Should().Be("32141sd");
    }

    [Fact]
    public void Two_RequiredString_with_different_value_should_be__not_equal()
    {
        var rTrackingIds = TrackingId.Create("Value1")
            .Combine(TrackingId.Create("Value2"));

        rTrackingIds.IsSuccess.Should().BeTrue();
        (var trackingId1, var trackingId2) = rTrackingIds.Ok;
        trackingId1.Value.Should().NotBe(trackingId2.Value);
    }

    [Fact]
    public void Two_RequiredString_with_same_value_should_be_equal()
    {
        var rTrackingIds = TrackingId.Create("SameValue")
            .Combine(TrackingId.Create("SameValue"));

        rTrackingIds.IsSuccess.Should().BeTrue();
        (var trackingId1, var trackingId2) = rTrackingIds.Ok;
        trackingId1.Value.Should().Be(trackingId2.Value);
    }

    [Fact]
    public void Can_implicitly_cast_to_string()
    {
        // Arrange
        TrackingId trackingId1 = TrackingId.Create("32141sd").Ok;

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
        trackingId1.Should().Be(TrackingId.Create("32141sd").Ok);
    }

    [Fact]
    public void Cannot_cast_empty_to_RequiredString()
    {
        // Arrange
        TrackingId trackingId;
        // Act
        Action act = () => trackingId = (TrackingId)string.Empty;

        // Assert
        act.Should().Throw<ResultFailureException<Err>>()
            .WithMessage("You attempted to access the Ok property for a failed result. A failed result has no Value.")
            .Where(e => e.Error.Description == "Tracking Id cannot be empty");
    }
}
