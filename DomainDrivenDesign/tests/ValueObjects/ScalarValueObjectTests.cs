namespace DomainDrivenDesign.Tests.ValueObjects;

using System.Collections.Generic;

public class ScalarValueObjectTests
{
    internal class PasswordSimple : ScalarValueObject<string>
    {
        public PasswordSimple(string value) : base(value)
        {
        }
    }

    internal class DerivedPasswordSimple : PasswordSimple
    {
        public DerivedPasswordSimple(string value) : base(value)
        {
        }
    }

    internal class MoneySimple : ScalarValueObject<decimal>
    {
        public MoneySimple(decimal value) : base(value)
        {
        }
        protected override IEnumerable<IComparable> GetEqualityComponents()
        {
            yield return Math.Round(Value, 2);
        }
    }

    internal class CustomerId : ScalarValueObject<Guid>
    {
        public CustomerId(Guid value) : base(value) { }
    }

    internal class Quantity : ScalarValueObject<int>
    {
        public Quantity(int value) : base(value) { }
    }

    [Fact]
    public void Two_ScalarValueObject_of_the_same_content_are_equal()
    {
        var password1 = new PasswordSimple("Password");
        var password2 = new PasswordSimple("Password");

        password1.Equals(password2).Should().BeTrue();
        (password1 == password2).Should().BeTrue();
        (password1 != password2).Should().BeFalse();
        password1.GetHashCode().Equals(password2.GetHashCode()).Should().BeTrue();
    }

    [Fact]
    public void Two_ScalarValueObject_of_the_different_content_are_not_equal()
    {
        var password1 = new PasswordSimple("Password1");
        var password2 = new PasswordSimple("Password2");

        password1.Equals(password2).Should().BeFalse();
        (password1 == password2).Should().BeFalse();
        (password1 != password2).Should().BeTrue();
        password1.GetHashCode().Equals(password2.GetHashCode()).Should().BeFalse();
    }

    [Fact]
    public void Derived_ScalarValueObject_are_not_equal()
    {
        var password = new PasswordSimple("Password");
        var derivedPassword = new DerivedPasswordSimple("Password");

        password.Equals(derivedPassword).Should().BeFalse();
        derivedPassword.Equals(password).Should().BeFalse();
        (password == derivedPassword).Should().BeFalse();
    }

    [Fact]
    public void NullAble_ScalarValueObject_can_be_compared_to_null()
    {
        // Arrange
        PasswordSimple? password = default;

        // Act & Assert
        (password == null).Should().BeTrue();
        (password == default(PasswordSimple)).Should().BeTrue();
        (password != null).Should().BeFalse();
        (null != password).Should().BeFalse();
        (password < null).Should().BeFalse();
        (password > null).Should().BeFalse();
    }

    [Fact]
    public void ScalarValueObject_is_sorted()
    {
        // Arrange
        var one = new MoneySimple(1);
        var two = new MoneySimple(2);
        var three = new MoneySimple(3);
        var moneys = new List<MoneySimple> { two, one, three };

        // Act
        moneys.Sort();

        // Assert
        moneys.Should().Equal(new List<MoneySimple> { one, two, three });

    }

    [Fact]
    public void It_is_possible_to_override_default_equality_comparison_behavior()
    {
        var money1 = new MoneySimple(2.2222m);
        var money2 = new MoneySimple(2.22m);

        money1.Equals(money2).Should().BeTrue();
        money1.GetHashCode().Equals(money2.GetHashCode()).Should().BeTrue();
    }

    [Fact]
    public void Comparing_simple_value_objects_of_different_values_returns_false()
    {
        var money1 = new MoneySimple(2.1m);
        var money2 = new MoneySimple(2.2m);

        money1.Equals(money2).Should().BeFalse();
    }

    #region Value Access and Implicit Conversion

    [Fact]
    public void Value_ReturnsWrappedValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var customerId = new CustomerId(guid);

        // Act & Assert
        customerId.Value.Should().Be(guid);
    }

    [Fact]
    public void ImplicitConversion_ToUnderlyingType_ReturnsValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var customerId = new CustomerId(guid);

        // Act
        Guid convertedGuid = customerId;

        // Assert
        convertedGuid.Should().Be(guid);
    }

    [Fact]
    public void ToString_ReturnsValueAsString()
    {
        // Arrange
        var password = new PasswordSimple("secret123");

        // Act - ToString in tests is culture-invariant context
        var result = password.ToString(null);

        // Assert
        result.Should().Be("secret123");
    }

    #endregion

    #region IConvertible Implementation

    [Fact]
    public void GetTypeCode_ReturnsCorrectTypeCode()
    {
        // Arrange
        var quantity = new Quantity(42);

        // Act
        var typeCode = ((IConvertible)quantity).GetTypeCode();

        // Assert
        typeCode.Should().Be(TypeCode.Int32);
    }

    [Fact]
    public void ToInt32_ConvertsCorrectly()
    {
        // Arrange
        var quantity = new Quantity(42);

        // Act
        var result = ((IConvertible)quantity).ToInt32(null);

        // Assert
        result.Should().Be(42);
    }

    [Fact]
    public void ToDouble_ConvertsIntToDouble()
    {
        // Arrange
        var quantity = new Quantity(42);

        // Act
        var result = ((IConvertible)quantity).ToDouble(null);

        // Assert
        result.Should().Be(42.0);
    }

    [Fact]
    public void ToDecimal_ConvertsDecimalValue()
    {
        // Arrange
        var money = new MoneySimple(98.6m);

        // Act
        var result = ((IConvertible)money).ToDecimal(null);

        // Assert
        result.Should().Be(98.6m);
    }

    [Fact]
    public void ToString_IConvertible_WithProvider_ReturnsFormattedString()
    {
        // Arrange
        var quantity = new Quantity(1234);

        // Act
        var result = ((IConvertible)quantity).ToString(System.Globalization.CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("1234");
    }

    [Fact]
    public void ToBoolean_NumericValue_ConvertsCorrectly()
    {
        // Arrange
        var zero = new Quantity(0);
        var one = new Quantity(1);

        // Act & Assert
        ((IConvertible)zero).ToBoolean(null).Should().BeFalse();
        ((IConvertible)one).ToBoolean(null).Should().BeTrue();
    }

    [Fact]
    public void ToInt64_ConvertsFromInt32()
    {
        // Arrange
        var quantity = new Quantity(42);

        // Act
        var result = ((IConvertible)quantity).ToInt64(null);

        // Assert
        result.Should().Be(42L);
    }

    [Fact]
    public void ToByte_SmallNumber_ConvertsCorrectly()
    {
        // Arrange
        var quantity = new Quantity(255);

        // Act
        var result = ((IConvertible)quantity).ToByte(null);

        // Assert
        result.Should().Be((byte)255);
    }

    [Fact]
    public void ToType_ConvertsToSpecifiedType()
    {
        // Arrange
        var quantity = new Quantity(42);

        // Act
        var result = ((IConvertible)quantity).ToType(typeof(long), null);

        // Assert
        result.Should().BeOfType<long>().Which.Should().Be(42L);
    }

    [Fact]
    public void ToInt16_ConvertsFromInt32()
    {
        // Arrange
        var quantity = new Quantity(100);

        // Act
        var result = ((IConvertible)quantity).ToInt16(null);

        // Assert
        result.Should().Be((short)100);
    }

    [Fact]
    public void ToSingle_ConvertsToFloat()
    {
        // Arrange
        var quantity = new Quantity(42);

        // Act
        var result = ((IConvertible)quantity).ToSingle(null);

        // Assert
        result.Should().Be(42f);
    }

    [Fact]
    public void ToSByte_ConvertsSmallNumber()
    {
        // Arrange
        var quantity = new Quantity(127);

        // Act
        var result = ((IConvertible)quantity).ToSByte(null);

        // Assert
        result.Should().Be((sbyte)127);
    }

    [Fact]
    public void ToUInt16_ConvertsToUnsignedShort()
    {
        // Arrange
        var quantity = new Quantity(1000);

        // Act
        var result = ((IConvertible)quantity).ToUInt16(null);

        // Assert
        result.Should().Be((ushort)1000);
    }

    [Fact]
    public void ToUInt32_ConvertsToUnsignedInt()
    {
        // Arrange
        var quantity = new Quantity(42);

        // Act
        var result = ((IConvertible)quantity).ToUInt32(null);

        // Assert
        result.Should().Be(42u);
    }

    [Fact]
    public void ToUInt64_ConvertsToUnsignedLong()
    {
        // Arrange
        var quantity = new Quantity(42);

        // Act
        var result = ((IConvertible)quantity).ToUInt64(null);

        // Assert
        result.Should().Be(42ul);
    }

    #endregion

    #region HashSet and Dictionary Usage

    [Fact]
    public void HashSet_UsesByValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var set = new HashSet<CustomerId>
        {
            new CustomerId(guid)
        };

        // Act
        var contains = set.Contains(new CustomerId(guid));

        // Assert
        contains.Should().BeTrue();
        set.Should().HaveCount(1);
    }

    [Fact]
    public void Dictionary_UsesAsKey()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var dict = new Dictionary<CustomerId, string>
        {
            [new CustomerId(guid)] = "Test Value"
        };

        // Act
        var value = dict[new CustomerId(guid)];

        // Assert
        value.Should().Be("Test Value");
    }

    #endregion
}
