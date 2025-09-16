﻿namespace CommonValueObjects.Tests;
using System;
using System.Globalization;
using System.Text.Json;
using Xunit;

public partial class EmployeeId : RequiredGuid
{
}

public class RequiredGuidTests
{
    [Fact]
    public void Cannot_create_empty_RequiredGuid()
    {
        var guidId1 = EmployeeId.TryCreate(default(Guid));
        guidId1.IsFailure.Should().BeTrue();
        guidId1.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)guidId1.Error;
        validation.FieldErrors[0].FieldName.Should().Be("employeeId");
        validation.FieldErrors[0].Details[0].Should().Be("Employee Id cannot be empty.");
        validation.Code.Should().Be("validation.error");
    }

    [Fact]
    public void Can_create_RequiredGuid_from_Guid()
    {
        var guid = Guid.NewGuid();
        EmployeeId.TryCreate(guid)
            .Tap(empId =>
            {
                empId.Should().BeOfType<EmployeeId>();
                ((Guid)empId).Should().Be(guid);
            })
            .IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Can_create_RequiredGuid_from_valid_string()
    {
        // Arrange
        var strGuid = Guid.NewGuid().ToString();

        // Act
        EmployeeId.TryCreate(strGuid)
            .Tap(empId =>
            {
                empId.Should().BeOfType<EmployeeId>();
                empId.ToString(CultureInfo.InvariantCulture).Should().Be(strGuid);
            })
            .IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Two_RequiredGuid_with_different_values_should_not_be_equal() =>
        EmployeeId.TryCreate(Guid.NewGuid())
            .Combine(EmployeeId.TryCreate(Guid.NewGuid()))
            .Tap((emp1, emp2) =>
            {
                (emp1 != emp2).Should().BeTrue();
                emp1.Equals(emp2).Should().BeFalse();
            })
            .IsSuccess.Should().BeTrue();

    [Fact]
    public void Two_RequiredGuid_with_same_value_should_be_equal()
    {
        var myGuid = Guid.NewGuid();
        EmployeeId.TryCreate(myGuid)
            .Combine(EmployeeId.TryCreate(myGuid))
            .Tap((emp1, emp2) =>
            {
                (emp1 == emp2).Should().BeTrue();
                emp1.Equals(emp2).Should().BeTrue();
            })
            .IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Can_use_ToString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var myGuid = EmployeeId.TryCreate(guid).Value;

        // Act
        var actual = myGuid.ToString(CultureInfo.InvariantCulture);

        // Assert
        actual.Should().Be(guid.ToString());
    }

    [Fact]
    public void Can_implicitly_cast_to_guid()
    {
        // Arrange
        Guid myGuid = Guid.NewGuid();
        EmployeeId myGuidId1 = EmployeeId.TryCreate(myGuid).Value;

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

    [Theory]
    [InlineData("")]
    [InlineData("Invalid")]
    public void Cannot_create_RequiredGuid_from_invalid_string(string value)
    {
        // Act
        var myGuidResult = EmployeeId.TryCreate(value);

        // Assert
        myGuidResult.IsFailure.Should().BeTrue();
        myGuidResult.Error.Should().BeOfType<ValidationError>();
        ValidationError ve = (ValidationError)myGuidResult.Error;
        ve.FieldErrors[0].FieldName.Should().Be("employeeId");
        ve.FieldErrors[0].Details[0].Should().Be("Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)");

    }

    [Theory]
    [InlineData(null)]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void Cannot_create_RequiredGuid_from_empty_string(string? value)
    {
        // Act
        var myGuidResult = EmployeeId.TryCreate(value);

        // Assert
        myGuidResult.IsFailure.Should().BeTrue();
        myGuidResult.Error.Should().BeOfType<ValidationError>();
        ValidationError ve = (ValidationError)myGuidResult.Error;
        ve.FieldErrors[0].FieldName.Should().Be("employeeId");
        ve.FieldErrors[0].Details[0].Should().Be("Employee Id cannot be empty.");
    }

    [Fact]
    public void Can_create_RequiredGuid_from_try_parsing_valid_string()
    {
        // Arrange
        var strGuid = Guid.NewGuid().ToString();

        // Act
        EmployeeId.TryParse(strGuid, null, out var myGuid)
            .Should().BeTrue();

        // Assert
        myGuid.Should().BeOfType<EmployeeId>();
        myGuid!.ToString(CultureInfo.InvariantCulture).Should().Be(strGuid);
    }

    [Fact]
    public void Cannot_create_RequiredGuid_from_try_parsing_invalid_string()
    {
        // Arrange
        var strGuid = "bad string";

        // Act
        EmployeeId.TryParse(strGuid, null, out var myGuid)
            .Should().BeFalse();

        // Assert
        myGuid.Should().BeNull();
    }

    [Fact]
    public void Can_create_RequiredGuid_from_parsing_valid_string()
    {
        // Arrange
        var strGuid = Guid.NewGuid().ToString();

        // Act
        var myGuid = EmployeeId.Parse(strGuid, null);

        // Assert
        myGuid.Should().BeOfType<EmployeeId>();
        myGuid.ToString(CultureInfo.InvariantCulture).Should().Be(strGuid);
    }

    [Fact]
    public void Cannot_create_RequiredGuid_from_parsing_invalid_string()
    {
        // Arrange
        var strGuid = "bad string";

        // Act
        Action act = () => EmployeeId.Parse(strGuid, null);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)");
    }

    [Fact]
    public void Can_use_Contains()
    {
        // Arrange
        var employeeId1 = EmployeeId.NewUnique();
        var employeeId2 = EmployeeId.NewUnique();
        IReadOnlyList<EmployeeId> employeeIds = new List<EmployeeId> { employeeId1, employeeId2 };

        // Act
        var actual = employeeIds.Contains(employeeId1);

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public void ConvertToJson()
    {
        // Arrange
        var employeeId = EmployeeId.NewUnique();
        Guid primEmployeeId = employeeId.Value;
        var expected = JsonSerializer.Serialize(primEmployeeId);

        // Act
        var actual = JsonSerializer.Serialize(employeeId);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void ConvertFromJson()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        var json = JsonSerializer.Serialize(guid);

        // Act
        EmployeeId actual = JsonSerializer.Deserialize<EmployeeId>(json)!;

        // Assert
        actual.Value.Should().Be(guid);
    }

    [Fact]
    public void Cannot_create_RequiredGuid_from_parsing_invalid_string_in_json()
    {
        // Arrange
        var strGuid = JsonSerializer.Serialize("bad guid");

        // Act
        Action act = () => JsonSerializer.Deserialize<EmployeeId>(strGuid);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)");
    }
}
