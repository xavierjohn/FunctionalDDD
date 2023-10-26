namespace CommonValueObjects.Tests;

using FunctionalDDD.Domain;
using FunctionalDDD.Results;
using FunctionalDDD.Results.Errors;

public partial class TrackingId : RequiredString<TrackingId>
{
}

public class RequiredString_T_Tests
{
    [Fact]
    public void Cannot_create_empty_RequiredString()
    {
        var trackingId1 = TrackingId.New("");
        trackingId1.IsFailure.Should().BeTrue();
        trackingId1.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)trackingId1.Error;
        validation.Message.Should().Be("Tracking Id cannot be empty.");
        validation.FieldName.Should().Be("trackingId");
        validation.Code.Should().Be("validation.error");
    }

    [Fact]
    public void Can_create_RequiredString()
    {
        var trackingId1 = TrackingId.New("32141sd");
        trackingId1.IsSuccess.Should().BeTrue();
        trackingId1.Value.Should().BeOfType<TrackingId>();
        trackingId1.Value.Value.Should().Be("32141sd");
    }

    [Fact]
    public void Two_RequiredString_with_different_value_should_be_not_equal()
    {
        var rTrackingIds = TrackingId.New("Value1")
            .Combine(TrackingId.New("Value2"));

        rTrackingIds.IsSuccess.Should().BeTrue();
        (var trackingId1, var trackingId2) = rTrackingIds.Value;
        trackingId1.Value.Should().NotBe(trackingId2.Value);
    }

    [Fact]
    public void Two_RequiredString_with_same_value_should_be_equal()
    {
        var rTrackingIds = TrackingId.New("SameValue")
            .Combine(TrackingId.New("SameValue"));

        rTrackingIds.IsSuccess.Should().BeTrue();
        (var trackingId1, var trackingId2) = rTrackingIds.Value;
        trackingId1.Value.Should().Be(trackingId2.Value);
    }

    [Fact]
    public void Can_implicitly_cast_to_string()
    {
        // Arrange
        TrackingId trackingId1 = TrackingId.New("32141sd").Value;

        // Act
        string strTracking = trackingId1;

        // Assert
        strTracking.Should().Be("32141sd");
    }

    [Fact]
    public void Can_explicitly_cast_to_RequiredString()
    {
        // Arrange

        // Act
        TrackingId trackingId1 = (TrackingId)"32141sd";

        // Assert
        trackingId1.Should().Be(TrackingId.New("32141sd").Value);
    }

    [Fact]
    public void Can_use_ToString()
    {
        // Arrange
        TrackingId trackingId1 = TrackingId.New("32141sd").Value;

        // Act
        var strTracking = trackingId1.ToString();

        // Assert
        strTracking.Should().Be("32141sd");
    }

    [Fact]
    public void Cannot_cast_empty_to_RequiredString()
    {
        // Arrange
        TrackingId trackingId;
        // Act
        Action act = () => trackingId = (TrackingId)string.Empty;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Attempted to access the Value for a failed result. A failed result has no Value.");
    }
}
