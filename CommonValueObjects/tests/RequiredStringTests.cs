﻿namespace CommonValueObjects.Tests;

using FunctionalDDD.Domain;
using FunctionalDDD.Results;
using FunctionalDDD.Results.Errors;

public partial class TrackingId : RequiredString
{
}

public class RequiredStringTests
{
    [Fact]
    public void Cannot_create_empty_RequiredString()
    {
        var trackingId1 = TrackingId.TryCreate(string.Empty);
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
        TrackingId.TryCreate("32141sd")
            .Tap(trackingId =>
            {
                trackingId.Should().BeOfType<TrackingId>();
                trackingId.ToString().Should().Be("32141sd");
            })
            .IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Two_RequiredString_with_same_value_should_be_equal() =>
        TrackingId.TryCreate("SameValue")
            .Combine(TrackingId.TryCreate("SameValue"))
            .Tap((tr1, tr2) =>
            {
                (tr1 == tr2).Should().BeTrue();
                tr1.Equals(tr2).Should().BeTrue();
            })
            .IsSuccess.Should().BeTrue();

    [Fact]
    public void Two_RequiredString_with_different_value_should_be_not_equal() =>
        TrackingId.TryCreate("Value1")
            .Combine(TrackingId.TryCreate("Value2"))
            .Tap((tr1, tr2) =>
             {
                 (tr1 != tr2).Should().BeTrue();
                 tr1.Equals(tr2).Should().BeFalse();
             })
            .IsSuccess.Should().BeTrue();

    [Fact]
    public void Can_implicitly_cast_to_string()
    {
        // Arrange
        TrackingId trackingId1 = TrackingId.TryCreate("32141sd").Value;

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
        trackingId1.Should().Be(TrackingId.TryCreate("32141sd").Value);
    }

    [Fact]
    public void Can_use_ToString()
    {
        // Arrange
        TrackingId trackingId1 = TrackingId.TryCreate("32141sd").Value;

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
