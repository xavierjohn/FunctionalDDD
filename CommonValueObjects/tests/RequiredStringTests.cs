﻿namespace CommonValueObjects.Tests;

using System.Globalization;
using System.Text.Json;

public partial class TrackingId : RequiredString
{
}
internal partial class InternalTrackingId : RequiredString
{
}

public class RequiredStringTests
{
    [Theory]
    [MemberData(nameof(GetBadString))]
    public void Cannot_create_empty_RequiredString(string? input)
    {
        var trackingId1 = TrackingId.TryCreate(input);
        trackingId1.IsFailure.Should().BeTrue();
        trackingId1.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)trackingId1.Error;
        validation.Errors[0].Name.Should().Be("trackingId");
        validation.Errors[0].Details[0].Should().Be("Tracking Id cannot be empty.");
        validation.Code.Should().Be("validation.error");
    }

    [Fact]
    public void Can_create_RequiredString() =>
        InternalTrackingId.TryCreate("32141sd")
            .Tap(trackingId =>
            {
                trackingId.Should().BeOfType<InternalTrackingId>();
                trackingId.ToString(CultureInfo.InvariantCulture).Should().Be("32141sd");
            })
            .IsSuccess.Should().BeTrue();

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
        var strTracking = trackingId1.ToString(CultureInfo.InvariantCulture);

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

    [Fact]
    public void Can_create_RequiredString_from_try_parsing_valid_string()
    {
        // Arrange
        var strTrackingId = "32141sd";

        // Act
        TrackingId.TryParse(strTrackingId, null, out var trackingId)
            .Should().BeTrue();

        // Assert
        trackingId.Should().BeOfType<TrackingId>();
        trackingId!.ToString(CultureInfo.InvariantCulture).Should().Be(strTrackingId);
    }

    [Theory]
    [MemberData(nameof(GetBadString))]
    public void Cannot_create_RequiredString_from_try_parsing_invalid_string(string? input)
    {
        // Arrange

        // Act
        TrackingId.TryParse(input, null, out var myId)
            .Should().BeFalse();

        // Assert
        myId.Should().BeNull();
    }

    [Fact]
    public void Can_create_RequiredString_from_parsing_valid_string()
    {
        // Arrange
        var strTrackingId = "32141sd";

        // Act
        var trackingId = TrackingId.Parse(strTrackingId, null);

        // Assert
        trackingId.Should().BeOfType<TrackingId>();
        trackingId.ToString(CultureInfo.InvariantCulture).Should().Be(strTrackingId);
    }

    [Theory]
    [MemberData(nameof(GetBadString))]
    public void Cannot_create_RequiredString_from_parsing_invalid_string(string? input)
    {
        // Arrange

        // Act
        Action act = () => TrackingId.Parse(input!, null);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Tracking Id cannot be empty.");
    }

    [Fact]
    public void Can_use_Contains()
    {
        // Arrange
        var id1 = TrackingId.TryCreate("id1").Value;
        var id2 = TrackingId.TryCreate("id2").Value;
        IReadOnlyList<TrackingId> ids = new List<TrackingId> { id1, id2 };

        // Act
        var actual = ids.Contains(id1);

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public void ConvertToJson()
    {
        // Arrange
        var strTrackingId = "MyTrackingId";
        TrackingId trackingId = TrackingId.TryCreate(strTrackingId).Value;
        var expected = JsonSerializer.Serialize(strTrackingId);

        // Act
        var actual = JsonSerializer.Serialize(trackingId);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void ConvertFromJson()
    {
        // Arrange
        string str = "MyTrackingId";
        var json = JsonSerializer.Serialize(str);

        // Act
        TrackingId actual = JsonSerializer.Deserialize<TrackingId>(json)!;

        // Assert
        actual.Value.Should().Be(str);
    }

    [Fact]
    public void Cannot_create_RequiredString_from_parsing_empty_string_in_json()
    {
        // Arrange
        var strGuid = JsonSerializer.Serialize(string.Empty);

        // Act
        Action act = () => JsonSerializer.Deserialize<TrackingId>(strGuid);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Tracking Id cannot be empty.");
    }

    public static TheoryData<string?> GetBadString() =>
      new TheoryData<string?>
      {
              null,
              string.Empty
      };
}
