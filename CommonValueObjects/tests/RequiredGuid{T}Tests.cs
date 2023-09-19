namespace CommonValueObjects.Tests;
using System;
using FunctionalDDD.Domain.ValueObjects;
using FunctionalDDD.Results;
using FunctionalDDD.Results.Errors;

public partial class MyGuidId : RequiredGuid<MyGuidId>
{
}

public class RequiredGuid_T_Tests
{
    [Fact]
    public void Cannot_create_empty_RequiredGuid()
    {
        var guidId1 = MyGuidId.New(default(Guid));
        guidId1.IsFailure.Should().BeTrue();
        guidId1.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)guidId1.Error;
        validation.Message.Should().Be("My Guid Id cannot be empty.");
        validation.FieldName.Should().Be("myGuidId");
        validation.Code.Should().Be("validation.error");
    }

    [Fact]
    public void Can_create_RequiredGuid()
    {
        var str = Guid.NewGuid().ToString();
        var guidId1 = MyGuidId.New(Guid.Parse(str));
        guidId1.IsSuccess.Should().BeTrue();
        guidId1.Value.Should().BeOfType<MyGuidId>();
        guidId1.Value.Value.Should().Be(Guid.Parse(str));
    }

    [Fact]
    public void Two_RequiredGuid_with_different_value_should_be__not_equal()
    {
        var rGuidsIds = MyGuidId.New(Guid.NewGuid())
            .Combine(MyGuidId.New(Guid.NewGuid()));

        rGuidsIds.IsSuccess.Should().BeTrue();
        (var guidId1, var guidId2) = rGuidsIds.Value;
        guidId1.Value.Should().NotBe(guidId2.Value);
    }

    [Fact]
    public void Two_RequiredGuid_with_same_value_should_be_equal()
    {
        var myGuid = Guid.NewGuid();
        var rGuidsIds = MyGuidId.New(myGuid)
            .Combine(MyGuidId.New(myGuid));

        rGuidsIds.IsSuccess.Should().BeTrue();
        (var guidId1, var guidId2) = rGuidsIds.Value;
        guidId1.Value.Should().Be(guidId2.Value);
    }

    [Fact]
    public void Can_implicitly_cast_to_guid()
    {
        // Arrange
        Guid myGuid = Guid.NewGuid();
        MyGuidId myGuidId1 = MyGuidId.New(myGuid).Value;

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
            .WithMessage("You attempted to access the Value for a failed result. A failed result has no Value.")
            .Where(e => e.Error.Message == "My Guid Id cannot be empty.");
    }

    [Fact]
    public void Can_create_RequiredGuid_from_valid_string()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var myGuidResult = MyGuidId.New(guid.ToString());

        // Assert
        myGuidResult.IsSuccess.Should().BeTrue();
        myGuidResult.Value.Value.Should().Be(guid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Invalid")]
    public void Cannot_create_RequiredGuid_from_invalid_string(string value)
    {
        // Act
        var myGuidResult = MyGuidId.New(value);

        // Assert
        myGuidResult.IsFailure.Should().BeTrue();
        myGuidResult.Error.Should().BeOfType<ValidationError>();
        ValidationError ve = (ValidationError)myGuidResult.Error;
        ve.Message.Should().Be("string is not in valid format.");
        ve.FieldName.Should().Be("myGuidId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void Cannot_create_RequiredGuid_from_empty_string(string? value)
    {
        // Act
        var myGuidResult = MyGuidId.New(value);

        // Assert
        myGuidResult.IsFailure.Should().BeTrue();
        myGuidResult.Error.Should().BeOfType<ValidationError>();
        ValidationError ve = (ValidationError)myGuidResult.Error;
        ve.Message.Should().Be("My Guid Id cannot be empty.");
        ve.FieldName.Should().Be("myGuidId");
    }
}
