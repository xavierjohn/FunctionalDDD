namespace CommonValueObjects.Tests;
using System;
using FunctionalDDD;

public partial class MyGuidId : RequiredGuid<MyGuidId>
{
}

public class RequiredGuid_T_Tests
{
    [Fact]
    public void Cannot_create_empty_RequiredGuid()
    {
        var guidId1 = MyGuidId.Create(default);
        guidId1.IsFailure.Should().BeTrue();
        guidId1.Errors.Should().HaveCount(1);
        guidId1.Error.Message.Should().Be("My Guid Id cannot be empty");
        guidId1.Error.Code.Should().Be("myGuidId");
    }

    [Fact]
    public void Can_create_RequiredGuid()
    {
        var str = Guid.NewGuid().ToString();
        var guidId1 = MyGuidId.Create(Guid.Parse(str));
        guidId1.IsSuccess.Should().BeTrue();
        guidId1.Value.Should().BeOfType<MyGuidId>();
        guidId1.Value.Value.Should().Be(Guid.Parse(str));
    }

    [Fact]
    public void Two_RequiredGuid_with_different_value_should_be__not_equal()
    {
        var rGuidsIds = MyGuidId.Create(Guid.NewGuid())
            .Combine(MyGuidId.Create(Guid.NewGuid()));

        rGuidsIds.IsSuccess.Should().BeTrue();
        (var guidId1, var trackingId2) = rGuidsIds.Value;
        guidId1.Value.Should().NotBe(trackingId2.Value);
    }

    [Fact]
    public void Two_RequiredGuid_with_same_value_should_be_equal()
    {
        var myGuid = Guid.NewGuid();
        var rGuidsIds = MyGuidId.Create(myGuid)
            .Combine(MyGuidId.Create(myGuid));

        rGuidsIds.IsSuccess.Should().BeTrue();
        (var guidId1, var guidId2) = rGuidsIds.Value;
        guidId1.Value.Should().Be(guidId2.Value);
    }

    [Fact]
    public void Can_implicitly_cast_to_guid()
    {
        // Arrange
        Guid myGuid = Guid.NewGuid();
        MyGuidId myGuidId1 = MyGuidId.Create(myGuid).Value;

        // Act
        Guid primGuid = myGuidId1;

        // Assert
        primGuid.Should().Be(myGuid);
    }

    [Fact]
    public void Can_cast_to_RequiredGuid()
    {
        // Arrange
        Guid myGuid = Guid.NewGuid();

        // Act
        MyGuidId myGuidId1 = (MyGuidId)myGuid;

        // Assert
        myGuidId1.Value.Should().Be(myGuid);
    }

    [Fact]
    public void Cannot_cast_empty_to_RequiredGuid()
    {
        // Arrange
        Guid myGuid = default;
        MyGuidId myGuidId1;

        // Act
        Action act = () => myGuidId1 = (MyGuidId)myGuid;

        // Assert
        act.Should().Throw<ResultFailureException>()
            .WithMessage("You attempted to access the Value property for a failed result. A failed result has no Value.")
            .Where(e => e.Errors[0].Message == "My Guid Id cannot be empty");
    }
}
