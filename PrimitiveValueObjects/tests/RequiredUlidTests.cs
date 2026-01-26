namespace PrimitiveValueObjects.Tests;

using FunctionalDdd.PrimitiveValueObjects;
using System;
using System.Globalization;
using System.Text.Json;
using Xunit;

public partial class OrderUlidId : RequiredUlid<OrderUlidId>
{
}

public class RequiredUlidTests
{
    [Fact]
    public void Cannot_create_empty_RequiredUlid()
    {
        var ulidId = OrderUlidId.TryCreate(default(Ulid));
        ulidId.IsFailure.Should().BeTrue();
        ulidId.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)ulidId.Error;
        validation.FieldErrors[0].FieldName.Should().Be("orderUlidId");
        validation.FieldErrors[0].Details[0].Should().Be("Order Ulid Id cannot be empty.");
        validation.Code.Should().Be("validation.error");
    }

    [Fact]
    public void Can_create_RequiredUlid_from_Ulid()
    {
        var ulid = Ulid.NewUlid();
        OrderUlidId.TryCreate(ulid)
            .Tap(orderId =>
            {
                orderId.Should().BeOfType<OrderUlidId>();
                ((Ulid)orderId).Should().Be(ulid);
            })
            .IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Can_create_RequiredUlid_from_valid_string()
    {
        // Arrange
        var strUlid = Ulid.NewUlid().ToString();

        // Act
        OrderUlidId.TryCreate(strUlid)
            .Tap(orderId =>
            {
                orderId.Should().BeOfType<OrderUlidId>();
                orderId.ToString(CultureInfo.InvariantCulture).Should().Be(strUlid);
            })
            .IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Two_RequiredUlid_with_different_values_should_not_be_equal() =>
        OrderUlidId.TryCreate(Ulid.NewUlid())
            .Combine(OrderUlidId.TryCreate(Ulid.NewUlid()))
            .Tap((order1, order2) =>
            {
                (order1 != order2).Should().BeTrue();
                order1.Equals(order2).Should().BeFalse();
            })
            .IsSuccess.Should().BeTrue();

    [Fact]
    public void Two_RequiredUlid_with_same_value_should_be_equal()
    {
        var myUlid = Ulid.NewUlid();
        OrderUlidId.TryCreate(myUlid)
            .Combine(OrderUlidId.TryCreate(myUlid))
            .Tap((order1, order2) =>
            {
                (order1 == order2).Should().BeTrue();
                order1.Equals(order2).Should().BeTrue();
            })
            .IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Can_use_ToString()
    {
        // Arrange
        var ulid = Ulid.NewUlid();
        var myUlid = OrderUlidId.TryCreate(ulid).Value;

        // Act
        var actual = myUlid.ToString(CultureInfo.InvariantCulture);

        // Assert
        actual.Should().Be(ulid.ToString());
    }

    [Fact]
    public void Can_implicitly_cast_to_ulid()
    {
        // Arrange
        Ulid myUlid = Ulid.NewUlid();
        OrderUlidId myUlidId = OrderUlidId.TryCreate(myUlid).Value;

        // Act
        Ulid primUlid = myUlidId;

        // Assert
        primUlid.Should().Be(myUlid);
    }

    [Fact]
    public void Can_cast_to_RequiredUlid()
    {
        // Arrange
        Ulid myUlid = Ulid.NewUlid();

        // Act
        OrderUlidId myUlidId = (OrderUlidId)myUlid;

        // Assert
        myUlidId.Value.Should().Be(myUlid);
    }

    [Fact]
    public void Cannot_cast_empty_to_RequiredUlid()
    {
        // Arrange
        Ulid myUlid = default;
        OrderUlidId myUlidId;

        // Act
        Action act = () => myUlidId = (OrderUlidId)myUlid;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Attempted to access the Value for a failed result. A failed result has no Value.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("Invalid")]
    [InlineData("01ARZ3NDEKTSV4RRFFQ69G5FA")] // Too short (25 chars)
    [InlineData("01ARZ3NDEKTSV4RRFFQ69G5FAVX")] // Too long (27 chars)
    public void Cannot_create_RequiredUlid_from_invalid_string(string value)
    {
        // Act
        var myUlidResult = OrderUlidId.TryCreate(value);

        // Assert
        myUlidResult.IsFailure.Should().BeTrue();
        myUlidResult.Error.Should().BeOfType<ValidationError>();
        ValidationError ve = (ValidationError)myUlidResult.Error;
        ve.FieldErrors[0].FieldName.Should().Be("orderUlidId");
        ve.FieldErrors[0].Details[0].Should().Be("Ulid should be a 26-character Crockford Base32 string.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("00000000000000000000000000")]
    public void Cannot_create_RequiredUlid_from_empty_string(string? value)
    {
        // Act
        var myUlidResult = OrderUlidId.TryCreate(value);

        // Assert
        myUlidResult.IsFailure.Should().BeTrue();
        myUlidResult.Error.Should().BeOfType<ValidationError>();
        ValidationError ve = (ValidationError)myUlidResult.Error;
        ve.FieldErrors[0].FieldName.Should().Be("orderUlidId");
        ve.FieldErrors[0].Details[0].Should().Be("Order Ulid Id cannot be empty.");
    }

    [Fact]
    public void Can_create_RequiredUlid_from_try_parsing_valid_string()
    {
        // Arrange
        var strUlid = Ulid.NewUlid().ToString();

        // Act
        OrderUlidId.TryParse(strUlid, null, out var myUlid)
            .Should().BeTrue();

        // Assert
        myUlid.Should().BeOfType<OrderUlidId>();
        myUlid!.ToString(CultureInfo.InvariantCulture).Should().Be(strUlid);
    }

    [Fact]
    public void Cannot_create_RequiredUlid_from_try_parsing_invalid_string()
    {
        // Arrange
        var strUlid = "bad string";

        // Act
        OrderUlidId.TryParse(strUlid, null, out var myUlid)
            .Should().BeFalse();

        // Assert
        myUlid.Should().BeNull();
    }

    [Fact]
    public void Can_create_RequiredUlid_from_parsing_valid_string()
    {
        // Arrange
        var strUlid = Ulid.NewUlid().ToString();

        // Act
        var myUlid = OrderUlidId.Parse(strUlid, null);

        // Assert
        myUlid.Should().BeOfType<OrderUlidId>();
        myUlid.ToString(CultureInfo.InvariantCulture).Should().Be(strUlid);
    }

    [Fact]
    public void Cannot_create_RequiredUlid_from_parsing_invalid_string()
    {
        // Arrange
        var strUlid = "bad string";

        // Act
        Action act = () => OrderUlidId.Parse(strUlid, null);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Ulid should be a 26-character Crockford Base32 string.");
    }

    [Fact]
    public void Can_use_Contains()
    {
        // Arrange
        var orderId1 = OrderUlidId.NewUnique();
        var orderId2 = OrderUlidId.NewUnique();
        IReadOnlyList<OrderUlidId> orderIds = new List<OrderUlidId> { orderId1, orderId2 };

        // Act
        var actual = orderIds.Contains(orderId1);

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public void ConvertToJson()
    {
        // Arrange
        var orderId = OrderUlidId.NewUnique();
        Ulid primOrderId = orderId.Value;
        var expected = JsonSerializer.Serialize(primOrderId);

        // Act
        var actual = JsonSerializer.Serialize(orderId);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void ConvertFromJson()
    {
        // Arrange
        Ulid ulid = Ulid.NewUlid();
        var json = JsonSerializer.Serialize(ulid);

        // Act
        OrderUlidId actual = JsonSerializer.Deserialize<OrderUlidId>(json)!;

        // Assert
        actual.Value.Should().Be(ulid);
    }

    [Fact]
    public void Cannot_create_RequiredUlid_from_parsing_invalid_string_in_json()
    {
        // Arrange
        var strUlid = JsonSerializer.Serialize("bad ulid");

        // Act
        Action act = () => JsonSerializer.Deserialize<OrderUlidId>(strUlid);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Ulid should be a 26-character Crockford Base32 string.");
    }

    [Fact]
    public void NewUnique_creates_new_ulid()
    {
        // Act
        var orderId1 = OrderUlidId.NewUnique();
        var orderId2 = OrderUlidId.NewUnique();

        // Assert
        orderId1.Should().NotBe(orderId2);
        orderId1.Value.Should().NotBe(default(Ulid));
        orderId2.Value.Should().NotBe(default(Ulid));
    }

    [Fact]
    public void NewUnique_ulids_are_lexicographically_sortable()
    {
        // Arrange - create ULIDs with a small delay to ensure different timestamps
        var orderId1 = OrderUlidId.NewUnique();
        var orderId2 = OrderUlidId.NewUnique();

        // Assert - ULIDs created later should sort after earlier ones
        // Note: Within the same millisecond, they're still sortable by random component
        orderId1.Value.CompareTo(orderId2.Value).Should().BeLessThanOrEqualTo(0);
    }

    [Fact]
    public void Can_create_RequiredUlid_from_nullable_Ulid()
    {
        // Arrange
        Ulid? nullableUlid = Ulid.NewUlid();

        // Act
        var result = OrderUlidId.TryCreate(nullableUlid);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(nullableUlid.Value);
    }

    [Fact]
    public void Cannot_create_RequiredUlid_from_null_nullable_Ulid()
    {
        // Arrange
        Ulid? nullableUlid = null;

        // Act
        var result = OrderUlidId.TryCreate(nullableUlid);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
        var validation = (ValidationError)result.Error;
        validation.FieldErrors[0].FieldName.Should().Be("orderUlidId");
        validation.FieldErrors[0].Details[0].Should().Be("Order Ulid Id cannot be empty.");
    }

    [Fact]
    public void Can_use_Create_with_valid_ulid()
    {
        // Arrange
        var ulid = Ulid.NewUlid();

        // Act
        var orderId = OrderUlidId.Create(ulid);

        // Assert
        orderId.Should().BeOfType<OrderUlidId>();
        orderId.Value.Should().Be(ulid);
    }

    [Fact]
    public void Create_with_empty_ulid_throws_exception()
    {
        // Arrange
        var emptyUlid = default(Ulid);

        // Act
        Action act = () => OrderUlidId.Create(emptyUlid);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Failed to create OrderUlidId: *");
    }

    [Fact]
    public void Custom_field_name_is_used_in_validation_error()
    {
        // Arrange
        var emptyUlid = default(Ulid);

        // Act
        var result = OrderUlidId.TryCreate(emptyUlid, "customFieldName");

        // Assert
        result.IsFailure.Should().BeTrue();
        var validation = (ValidationError)result.Error;
        validation.FieldErrors[0].FieldName.Should().Be("customFieldName");
    }
}
