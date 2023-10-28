namespace CommonValueObjects.Tests;
using System;
using FunctionalDDD.Domain;
using FunctionalDDD.Results;
using FunctionalDDD.Results.Errors;

public partial class EmployeeId : RequiredGuid
{
}

public class RequiredGuid_T_Tests
{
    [Fact]
    public void Cannot_create_empty_RequiredGuid()
    {
        var guidId1 = EmployeeId.New(default(Guid));
        guidId1.IsFailure.Should().BeTrue();
        guidId1.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)guidId1.Error;
        validation.Message.Should().Be("Employee Id cannot be empty.");
        validation.FieldName.Should().Be("employeeId");
        validation.Code.Should().Be("validation.error");
    }

    [Fact]
    public void Can_create_RequiredGuid()
    {
        var str = Guid.NewGuid().ToString();
        var guidId1 = EmployeeId.New(Guid.Parse(str));
        guidId1.IsSuccess.Should().BeTrue();
        guidId1.Value.Should().BeOfType<EmployeeId>();
        guidId1.Value.Value.Should().Be(Guid.Parse(str));
    }

    [Fact]
    public void Two_RequiredGuid_with_different_value_should_be__not_equal()
    {
        var rGuidsIds = EmployeeId.New(Guid.NewGuid())
            .Combine(EmployeeId.New(Guid.NewGuid()));

        rGuidsIds.IsSuccess.Should().BeTrue();
        (var guidId1, var guidId2) = rGuidsIds.Value;
        guidId1.Value.Should().NotBe(guidId2.Value);
    }

    [Fact]
    public void Two_RequiredGuid_with_same_value_should_be_equal()
    {
        var myGuid = Guid.NewGuid();
        var rGuidsIds = EmployeeId.New(myGuid)
            .Combine(EmployeeId.New(myGuid));

        rGuidsIds.IsSuccess.Should().BeTrue();
        (var guidId1, var guidId2) = rGuidsIds.Value;
        guidId1.Value.Should().Be(guidId2.Value);
    }

    [Fact]
    public void Can_use_ToString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var myGuid = EmployeeId.New(guid).Value;

        // Act
        var actual = myGuid.ToString();

        // Assert
        actual.Should().Be(guid.ToString());
    }

    [Fact]
    public void Can_implicitly_cast_to_guid()
    {
        // Arrange
        Guid myGuid = Guid.NewGuid();
        EmployeeId myGuidId1 = EmployeeId.New(myGuid).Value;

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
        EmployeeId myGuidId1 = (EmployeeId)myGuid;

        // Assert
        myGuidId1.Value.Should().Be(myGuid);
    }

    [Fact]
    public void Cannot_cast_empty_to_RequiredGuid()
    {
        // Arrange
        Guid myGuid = default;
        EmployeeId myGuidId1;

        // Act
        Action act = () => myGuidId1 = (EmployeeId)myGuid;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Attempted to access the Value for a failed result. A failed result has no Value.");
    }

    [Fact]
    public void Can_create_RequiredGuid_from_valid_string()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var myGuidResult = EmployeeId.New(guid.ToString());

        // Assert
        myGuidResult.IsSuccess.Should().BeTrue();
        (myGuidResult.Value == guid).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Invalid")]
    public void Cannot_create_RequiredGuid_from_invalid_string(string value)
    {
        // Act
        var myGuidResult = EmployeeId.New(value);

        // Assert
        myGuidResult.IsFailure.Should().BeTrue();
        myGuidResult.Error.Should().BeOfType<ValidationError>();
        ValidationError ve = (ValidationError)myGuidResult.Error;
        ve.Message.Should().Be("string is not in valid format.");
        ve.FieldName.Should().Be("employeeId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void Cannot_create_RequiredGuid_from_empty_string(string? value)
    {
        // Act
        var myGuidResult = EmployeeId.New(value);

        // Assert
        myGuidResult.IsFailure.Should().BeTrue();
        myGuidResult.Error.Should().BeOfType<ValidationError>();
        ValidationError ve = (ValidationError)myGuidResult.Error;
        ve.Message.Should().Be("Employee Id cannot be empty.");
        ve.FieldName.Should().Be("employeeId");
    }
}
