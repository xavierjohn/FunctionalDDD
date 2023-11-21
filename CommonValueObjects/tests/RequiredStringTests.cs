namespace CommonValueObjects.Tests;

public partial class PublicTrackingId : RequiredString
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
        var trackingId1 = PublicTrackingId.TryCreate(input);
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
        InternalTrackingId.TryCreate("32141sd")
            .Tap(trackingId =>
            {
                trackingId.Should().BeOfType<InternalTrackingId>();
                trackingId.ToString().Should().Be("32141sd");
            })
            .IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Two_RequiredString_with_same_value_should_be_equal() =>
        PublicTrackingId.TryCreate("SameValue")
            .Combine(PublicTrackingId.TryCreate("SameValue"))
            .Tap((tr1, tr2) =>
            {
                (tr1 == tr2).Should().BeTrue();
                tr1.Equals(tr2).Should().BeTrue();
            })
            .IsSuccess.Should().BeTrue();

    [Fact]
    public void Two_RequiredString_with_different_value_should_be_not_equal() =>
        PublicTrackingId.TryCreate("Value1")
            .Combine(PublicTrackingId.TryCreate("Value2"))
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
        PublicTrackingId trackingId1 = PublicTrackingId.TryCreate("32141sd").Value;

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
        PublicTrackingId trackingId1 = (PublicTrackingId)"32141sd";

        // Assert
        trackingId1.Should().Be(PublicTrackingId.TryCreate("32141sd").Value);
    }

    [Fact]
    public void Can_use_ToString()
    {
        // Arrange
        PublicTrackingId trackingId1 = PublicTrackingId.TryCreate("32141sd").Value;

        // Act
        var strTracking = trackingId1.ToString();

        // Assert
        strTracking.Should().Be("32141sd");
    }

    [Fact]
    public void Cannot_cast_empty_to_RequiredString()
    {
        // Arrange
        PublicTrackingId trackingId;
        // Act
        Action act = () => trackingId = (PublicTrackingId)string.Empty;

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
        PublicTrackingId.TryParse(strTrackingId, null, out var trackingId)
            .Should().BeTrue();

        // Assert
        trackingId.Should().BeOfType<PublicTrackingId>();
        trackingId!.ToString().Should().Be(strTrackingId);
    }

    [Theory]
    [MemberData(nameof(GetBadString))]
    public void Cannot_create_RequiredString_from_try_parsing_invalid_string(string? input)
    {
        // Arrange

        // Act
        PublicTrackingId.TryParse(input, null, out var myId)
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
        var trackingId = PublicTrackingId.Parse(strTrackingId, null);

        // Assert
        trackingId.Should().BeOfType<PublicTrackingId>();
        trackingId.ToString().Should().Be(strTrackingId);
    }

    [Theory]
    [MemberData(nameof(GetBadString))]
    public void Cannot_create_RequiredString_from_parsing_invalid_string(string? input)
    {
        // Arrange

        // Act
        Action act = () => PublicTrackingId.Parse(input!, null);

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
