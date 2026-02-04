namespace DomainDrivenDesign.Tests.EnumValueObjects;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Tests for <see cref="EnumValueObject{TSelf}"/> base class functionality.
/// </summary>
public class EnumValueObjectTests
{
    #region Test Enum value objects

    /// <summary>
    /// Basic Enum value object for testing core functionality.
    /// </summary>
    [JsonConverter(typeof(EnumValueObjectJsonConverter<OrderStatus>))]
    internal class OrderStatus : EnumValueObject<OrderStatus>
    {
        public static readonly OrderStatus Pending = new(1, "Pending");
        public static readonly OrderStatus Processing = new(2, "Processing");
        public static readonly OrderStatus Shipped = new(3, "Shipped");
        public static readonly OrderStatus Delivered = new(4, "Delivered");
        public static readonly OrderStatus Cancelled = new(5, "Cancelled");

        private OrderStatus(int value, string name) : base(value, name) { }
    }

    /// <summary>
    /// Enum value object with additional properties for testing behavior.
    /// </summary>
    internal class PaymentStatus : EnumValueObject<PaymentStatus>
    {
        public static readonly PaymentStatus Pending = new(1, "Pending", isTerminal: false);
        public static readonly PaymentStatus Completed = new(2, "Completed", isTerminal: true);
        public static readonly PaymentStatus Failed = new(3, "Failed", isTerminal: true);
        public static readonly PaymentStatus Refunded = new(4, "Refunded", isTerminal: true);

        public bool IsTerminal { get; }

        private PaymentStatus(int value, string name, bool isTerminal) : base(value, name) =>
            IsTerminal = isTerminal;
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public void GetAll_ReturnsAllDefinedMembers()
    {
        // Act
        var allStatuses = OrderStatus.GetAll();

        // Assert
        allStatuses.Should().HaveCount(5);
        allStatuses.Should().Contain(OrderStatus.Pending);
        allStatuses.Should().Contain(OrderStatus.Processing);
        allStatuses.Should().Contain(OrderStatus.Shipped);
        allStatuses.Should().Contain(OrderStatus.Delivered);
        allStatuses.Should().Contain(OrderStatus.Cancelled);
    }

    [Fact]
    public void GetAll_ReturnsSameInstanceOnMultipleCalls()
    {
        // Act
        var first = OrderStatus.GetAll();
        var second = OrderStatus.GetAll();

        // Assert - should be cached
        first.Should().BeSameAs(second);
    }

    #endregion

    #region TryFromValue Tests

    [Fact]
    public void TryFromValue_ValidValue_ReturnsSuccess()
    {
        // Act
        var result = OrderStatus.TryFromValue(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void TryFromValue_InvalidValue_ReturnsFailure()
    {
        // Act
        var result = OrderStatus.TryFromValue(999);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error.Detail.Should().Contain("999");
        result.Error.Detail.Should().Contain("not a valid OrderStatus");
    }

    [Fact]
    public void TryFromValue_WithFieldName_IncludesFieldNameInError()
    {
        // Act
        var result = OrderStatus.TryFromValue(999, "status");

        // Assert
        result.IsFailure.Should().BeTrue();
        var validationError = result.Error as ValidationError;
        validationError.Should().NotBeNull();
        validationError!.FieldErrors.Should().ContainSingle()
            .Which.FieldName.Should().Be("status");
    }

    [Fact]
    public void TryFromValue_OutOverload_ValidValue_ReturnsTrue()
    {
        // Act
        var success = OrderStatus.TryFromValue(2, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public void TryFromValue_OutOverload_InvalidValue_ReturnsFalse()
    {
        // Act
        var success = OrderStatus.TryFromValue(999, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    #endregion

    #region TryFromName Tests

    [Fact]
    public void TryFromName_ValidName_ReturnsSuccess()
    {
        // Act
        var result = OrderStatus.TryFromName("Pending");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void TryFromName_ValidNameCaseInsensitive_ReturnsSuccess()
    {
        // Act
        var result = OrderStatus.TryFromName("PENDING");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void TryFromName_InvalidName_ReturnsFailure()
    {
        // Act
        var result = OrderStatus.TryFromName("Unknown");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error.Detail.Should().Contain("Unknown");
        result.Error.Detail.Should().Contain("not a valid OrderStatus");
    }

    [Fact]
    public void TryFromName_NullName_ReturnsFailure()
    {
        // Act
        var result = OrderStatus.TryFromName(null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Detail.Should().Contain("cannot be empty");
    }

    [Fact]
    public void TryFromName_EmptyName_ReturnsFailure()
    {
        // Act
        var result = OrderStatus.TryFromName("");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Detail.Should().Contain("cannot be empty");
    }

    [Fact]
    public void TryFromName_WhitespaceName_ReturnsFailure()
    {
        // Act
        var result = OrderStatus.TryFromName("   ");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Detail.Should().Contain("cannot be empty");
    }

    [Fact]
    public void TryFromName_OutOverload_ValidName_ReturnsTrue()
    {
        // Act
        var success = OrderStatus.TryFromName("Shipped", out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void TryFromName_OutOverload_InvalidName_ReturnsFalse()
    {
        // Act
        var success = OrderStatus.TryFromName("Unknown", out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    #endregion

    #region FromValue Tests

    [Fact]
    public void FromValue_ValidValue_ReturnsMember()
    {
        // Act
        var status = OrderStatus.FromValue(3);

        // Assert
        status.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void FromValue_InvalidValue_ThrowsInvalidOperationException()
    {
        // Act
        var act = () => OrderStatus.FromValue(999);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*OrderStatus*999*");
    }

    #endregion

    #region FromName Tests

    [Fact]
    public void FromName_ValidName_ReturnsMember()
    {
        // Act
        var status = OrderStatus.FromName("Delivered");

        // Assert
        status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public void FromName_InvalidName_ThrowsInvalidOperationException()
    {
        // Act
        var act = () => OrderStatus.FromName("Unknown");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*OrderStatus*Unknown*");
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_SameInstance_ReturnsTrue()
    {
        // Arrange
        var status = OrderStatus.Pending;

        // Act & Assert
        status.Equals(status).Should().BeTrue();
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        // Arrange
        var status1 = OrderStatus.TryFromValue(1).Value;
        var status2 = OrderStatus.TryFromName("Pending").Value;

        // Act & Assert
        status1.Equals(status2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse() =>
        // Act & Assert
        OrderStatus.Pending.Equals(OrderStatus.Processing).Should().BeFalse();

    [Fact]
    public void Equals_Null_ReturnsFalse() =>
        // Act & Assert
        OrderStatus.Pending.Equals(null).Should().BeFalse();

    [Fact]
    public void EqualsOperator_SameValue_ReturnsTrue()
    {
        // Arrange
        var status1 = OrderStatus.Pending;
        var status2 = OrderStatus.TryFromValue(1).Value;

        // Act & Assert
        (status1 == status2).Should().BeTrue();
    }

    [Fact]
    public void NotEqualsOperator_DifferentValue_ReturnsTrue() =>
        // Act & Assert
        (OrderStatus.Pending != OrderStatus.Processing).Should().BeTrue();

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHashCode()
    {
        // Arrange
        var status1 = OrderStatus.Pending;
        var status2 = OrderStatus.TryFromValue(1).Value;

        // Act & Assert
        status1.GetHashCode().Should().Be(status2.GetHashCode());
    }

    #endregion

    #region Comparison Tests

    [Fact]
    public void CompareTo_LessThan_ReturnsNegative()
    {
        // Act
        var result = OrderStatus.Pending.CompareTo(OrderStatus.Processing);

        // Assert
        result.Should().BeNegative();
    }

    [Fact]
    public void CompareTo_GreaterThan_ReturnsPositive()
    {
        // Act
        var result = OrderStatus.Shipped.CompareTo(OrderStatus.Pending);

        // Assert
        result.Should().BePositive();
    }

    [Fact]
    public void CompareTo_Equal_ReturnsZero()
    {
        // Arrange
        var status1 = OrderStatus.Pending;
        var status2 = OrderStatus.TryFromValue(1).Value;

        // Act
        var result = status1.CompareTo(status2);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        // Act
        var result = OrderStatus.Pending.CompareTo(null);

        // Assert
        result.Should().BePositive();
    }

    [Fact]
    public void LessThanOperator_Works()
    {
        // Assert
        (OrderStatus.Pending < OrderStatus.Processing).Should().BeTrue();
        (OrderStatus.Processing < OrderStatus.Pending).Should().BeFalse();
    }

    [Fact]
    public void GreaterThanOperator_Works()
    {
        // Assert
        (OrderStatus.Shipped > OrderStatus.Pending).Should().BeTrue();
        (OrderStatus.Pending > OrderStatus.Shipped).Should().BeFalse();
    }

    [Fact]
    public void LessThanOrEqualOperator_Works()
    {
        // Arrange
        var pending1 = OrderStatus.Pending;
        var pending2 = OrderStatus.TryFromValue(1).Value;

        // Assert
        (pending1 <= pending2).Should().BeTrue();
        (OrderStatus.Pending <= OrderStatus.Processing).Should().BeTrue();
        (OrderStatus.Processing <= OrderStatus.Pending).Should().BeFalse();
    }

    [Fact]
    public void GreaterThanOrEqualOperator_Works()
    {
        // Arrange
        var pending1 = OrderStatus.Pending;
        var pending2 = OrderStatus.TryFromValue(1).Value;

        // Assert
        (pending1 >= pending2).Should().BeTrue();
        (OrderStatus.Shipped >= OrderStatus.Pending).Should().BeTrue();
        (OrderStatus.Pending >= OrderStatus.Shipped).Should().BeFalse();
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversionToInt_ReturnsValue()
    {
        // Arrange
        int value = OrderStatus.Processing;

        // Assert
        value.Should().Be(2);
    }

    [Fact]
    public void ImplicitConversionToString_ReturnsName()
    {
        // Arrange
        string name = OrderStatus.Processing;

        // Assert
        name.Should().Be("Processing");
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsName()
    {
        // Act
        var result = OrderStatus.Pending.ToString();

        // Assert
        result.Should().Be("Pending");
    }

    #endregion

    #region Value and Name Properties Tests

    [Fact]
    public void Value_ReturnsAssignedValue()
    {
        // Assert
        OrderStatus.Pending.Value.Should().Be(1);
        OrderStatus.Cancelled.Value.Should().Be(5);
    }

    [Fact]
    public void Name_ReturnsAssignedName()
    {
        // Assert
        OrderStatus.Pending.Name.Should().Be("Pending");
        OrderStatus.Cancelled.Name.Should().Be("Cancelled");
    }

    #endregion

    #region Enum value object with Behavior Tests

    [Fact]
    public void EnumValueObjectWithBehavior_PropertiesAreAccessible()
    {
        // Assert
        PaymentStatus.Pending.IsTerminal.Should().BeFalse();
        PaymentStatus.Completed.IsTerminal.Should().BeTrue();
        PaymentStatus.Failed.IsTerminal.Should().BeTrue();
    }

    [Fact]
    public void EnumValueObjectWithBehavior_CanFilterByProperty()
    {
        // Act
        var terminalStatuses = PaymentStatus.GetAll().Where(s => s.IsTerminal).ToList();

        // Assert
        terminalStatuses.Should().HaveCount(3);
        terminalStatuses.Should().Contain(PaymentStatus.Completed);
        terminalStatuses.Should().Contain(PaymentStatus.Failed);
        terminalStatuses.Should().Contain(PaymentStatus.Refunded);
    }

    #endregion
}
