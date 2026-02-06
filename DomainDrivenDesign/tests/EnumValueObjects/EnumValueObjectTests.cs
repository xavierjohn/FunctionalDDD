namespace DomainDrivenDesign.Tests.EnumValueObjects;

using System.Text.Json.Serialization;

/// <summary>
/// Tests for <see cref="EnumValueObject{TSelf}"/> base class functionality.
/// </summary>
public class EnumValueObjectTests
{
    #region Test Enum Value Objects

    [JsonConverter(typeof(EnumValueObjectJsonConverter<OrderStatus>))]
    internal class OrderStatus : EnumValueObject<OrderStatus>
    {
        // Name is auto-derived from field name
        public static readonly OrderStatus Pending = new();
        public static readonly OrderStatus Processing = new();
        public static readonly OrderStatus Shipped = new();
        public static readonly OrderStatus Delivered = new();
        public static readonly OrderStatus Cancelled = new();

        private OrderStatus() { }
    }

    internal class PaymentStatus : EnumValueObject<PaymentStatus>
    {
        public static readonly PaymentStatus Pending = new(isTerminal: false);
        public static readonly PaymentStatus Completed = new(isTerminal: true);
        public static readonly PaymentStatus Failed = new(isTerminal: true);
        public static readonly PaymentStatus Refunded = new(isTerminal: true);

        public bool IsTerminal { get; }

        private PaymentStatus(bool isTerminal) => IsTerminal = isTerminal;
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public void GetAll_ReturnsAllDefinedMembers()
    {
        var allStatuses = OrderStatus.GetAll();

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
        var first = OrderStatus.GetAll();
        var second = OrderStatus.GetAll();

        first.Should().BeSameAs(second);
    }

    [Fact]
    public void GetAll_AssignsAutoIncrementingValues()
    {
        var all = OrderStatus.GetAll().ToList();

        all[0].Value.Should().Be(0);
        all[1].Value.Should().Be(1);
        all[2].Value.Should().Be(2);
        all[3].Value.Should().Be(3);
        all[4].Value.Should().Be(4);
    }

    #endregion

    #region TryFromName Tests

    [Fact]
    public void TryFromName_ValidName_ReturnsSuccess()
    {
        var result = OrderStatus.TryFromName("Pending");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void TryFromName_ValidNameCaseInsensitive_ReturnsSuccess()
    {
        var result = OrderStatus.TryFromName("PENDING");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void TryFromName_InvalidName_ReturnsFailure()
    {
        var result = OrderStatus.TryFromName("Unknown");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error.Detail.Should().Contain("Unknown");
        result.Error.Detail.Should().Contain("not a valid OrderStatus");
    }

    [Fact]
    public void TryFromName_NullName_ReturnsFailure()
    {
        var result = OrderStatus.TryFromName(null);

        result.IsFailure.Should().BeTrue();
        result.Error.Detail.Should().Contain("cannot be empty");
    }

    [Fact]
    public void TryFromName_EmptyName_ReturnsFailure()
    {
        var result = OrderStatus.TryFromName("");

        result.IsFailure.Should().BeTrue();
        result.Error.Detail.Should().Contain("cannot be empty");
    }

    [Fact]
    public void TryFromName_WhitespaceName_ReturnsFailure()
    {
        var result = OrderStatus.TryFromName("   ");

        result.IsFailure.Should().BeTrue();
        result.Error.Detail.Should().Contain("cannot be empty");
    }

    [Fact]
    public void TryFromName_OutOverload_ValidName_ReturnsTrue()
    {
        var success = OrderStatus.TryFromName("Shipped", out var result);

        success.Should().BeTrue();
        result.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void TryFromName_OutOverload_InvalidName_ReturnsFalse()
    {
        var success = OrderStatus.TryFromName("Unknown", out var result);

        success.Should().BeFalse();
        result.Should().BeNull();
    }

    #endregion

    #region FromName Tests

    [Fact]
    public void FromName_ValidName_ReturnsMember()
    {
        var status = OrderStatus.FromName("Delivered");

        status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public void FromName_InvalidName_ThrowsInvalidOperationException()
    {
        var act = () => OrderStatus.FromName("Unknown");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*OrderStatus*Unknown*");
    }

    #endregion

    #region Is and IsNot Tests

    [Fact]
    public void Is_MatchingValue_ReturnsTrue()
    {
        var status = OrderStatus.Pending;

        status.Is(OrderStatus.Pending).Should().BeTrue();
        status.Is(OrderStatus.Pending, OrderStatus.Processing).Should().BeTrue();
    }

    [Fact]
    public void Is_NonMatchingValue_ReturnsFalse()
    {
        var status = OrderStatus.Pending;

        status.Is(OrderStatus.Shipped).Should().BeFalse();
        status.Is(OrderStatus.Shipped, OrderStatus.Delivered).Should().BeFalse();
    }

    [Fact]
    public void IsNot_NonMatchingValue_ReturnsTrue()
    {
        var status = OrderStatus.Pending;

        status.IsNot(OrderStatus.Shipped).Should().BeTrue();
        status.IsNot(OrderStatus.Shipped, OrderStatus.Delivered).Should().BeTrue();
    }

    [Fact]
    public void IsNot_MatchingValue_ReturnsFalse()
    {
        var status = OrderStatus.Pending;

        status.IsNot(OrderStatus.Pending).Should().BeFalse();
        status.IsNot(OrderStatus.Pending, OrderStatus.Processing).Should().BeFalse();
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_SameInstance_ReturnsTrue()
    {
        var status = OrderStatus.Pending;

        status.Equals(status).Should().BeTrue();
    }

    [Fact]
    public void Equals_SameName_ReturnsTrue()
    {
        var status1 = OrderStatus.TryFromName("Pending").Value;
        var status2 = OrderStatus.TryFromName("pending").Value;

        status1.Equals(status2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse() =>
        OrderStatus.Pending.Equals(OrderStatus.Processing).Should().BeFalse();

    [Fact]
    public void Equals_Null_ReturnsFalse() =>
        OrderStatus.Pending.Equals(null).Should().BeFalse();

    [Fact]
    public void EqualsOperator_SameValue_ReturnsTrue()
    {
        var status1 = OrderStatus.Pending;
        var status2 = OrderStatus.TryFromName("Pending").Value;

        (status1 == status2).Should().BeTrue();
    }

    [Fact]
    public void NotEqualsOperator_DifferentValue_ReturnsTrue() =>
        (OrderStatus.Pending != OrderStatus.Processing).Should().BeTrue();

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHashCode()
    {
        var status1 = OrderStatus.Pending;
        var status2 = OrderStatus.TryFromName("Pending").Value;

        status1.GetHashCode().Should().Be(status2.GetHashCode());
    }

    #endregion

    #region Comparison Tests

    [Fact]
    public void CompareTo_LessThan_ReturnsNegative()
    {
        var result = OrderStatus.Pending.CompareTo(OrderStatus.Processing);

        result.Should().BeNegative();
    }

    [Fact]
    public void CompareTo_GreaterThan_ReturnsPositive()
    {
        var result = OrderStatus.Shipped.CompareTo(OrderStatus.Pending);

        result.Should().BePositive();
    }

    [Fact]
    public void CompareTo_Equal_ReturnsZero()
    {
        var status1 = OrderStatus.Pending;
        var status2 = OrderStatus.TryFromName("Pending").Value;

        var result = status1.CompareTo(status2);

        result.Should().Be(0);
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var result = OrderStatus.Pending.CompareTo(null);

        result.Should().BePositive();
    }

    [Fact]
    public void LessThanOperator_Works()
    {
        (OrderStatus.Pending < OrderStatus.Processing).Should().BeTrue();
        (OrderStatus.Processing < OrderStatus.Pending).Should().BeFalse();
    }

    [Fact]
    public void GreaterThanOperator_Works()
    {
        (OrderStatus.Shipped > OrderStatus.Pending).Should().BeTrue();
        (OrderStatus.Pending > OrderStatus.Shipped).Should().BeFalse();
    }

    [Fact]
    public void LessThanOrEqualOperator_Works()
    {
        var pending1 = OrderStatus.Pending;
        var pending2 = OrderStatus.TryFromName("Pending").Value;

        (pending1 <= pending2).Should().BeTrue();
        (OrderStatus.Pending <= OrderStatus.Processing).Should().BeTrue();
        (OrderStatus.Processing <= OrderStatus.Pending).Should().BeFalse();
    }

    [Fact]
    public void GreaterThanOrEqualOperator_Works()
    {
        var pending1 = OrderStatus.Pending;
        var pending2 = OrderStatus.TryFromName("Pending").Value;

        (pending1 >= pending2).Should().BeTrue();
        (OrderStatus.Shipped >= OrderStatus.Pending).Should().BeTrue();
        (OrderStatus.Pending >= OrderStatus.Shipped).Should().BeFalse();
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversionToString_ReturnsName()
    {
        string name = OrderStatus.Processing;

        name.Should().Be("Processing");
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsName()
    {
        var result = OrderStatus.Pending.ToString();

        result.Should().Be("Pending");
    }

    #endregion

    #region Value and Name Properties Tests

    [Fact]
    public void Value_ReturnsAutoGeneratedValue()
    {
        // Values are auto-generated based on declaration order (0, 1, 2, ...)
        OrderStatus.Pending.Value.Should().Be(0);
        OrderStatus.Processing.Value.Should().Be(1);
        OrderStatus.Shipped.Value.Should().Be(2);
    }

    [Fact]
    public void Name_ReturnsDerivedFromFieldName()
    {
        // Name is auto-derived from the field name
        OrderStatus.Pending.Name.Should().Be("Pending");
        OrderStatus.Cancelled.Name.Should().Be("Cancelled");
    }

    #endregion

    #region Behavior Tests

    [Fact]
    public void EnumValueObjectWithBehavior_PropertiesAreAccessible()
    {
        PaymentStatus.Pending.IsTerminal.Should().BeFalse();
        PaymentStatus.Completed.IsTerminal.Should().BeTrue();
        PaymentStatus.Failed.IsTerminal.Should().BeTrue();
        PaymentStatus.Refunded.IsTerminal.Should().BeTrue();
    }

    [Fact]
    public void EnumValueObjectWithBehavior_CanFilterByProperty()
    {
        var terminalStatuses = PaymentStatus.GetAll()
            .Where(s => s.IsTerminal)
            .ToList();

        terminalStatuses.Should().HaveCount(3);
        terminalStatuses.Should().Contain(PaymentStatus.Completed);
        terminalStatuses.Should().Contain(PaymentStatus.Failed);
        terminalStatuses.Should().Contain(PaymentStatus.Refunded);
    }

    #endregion
}