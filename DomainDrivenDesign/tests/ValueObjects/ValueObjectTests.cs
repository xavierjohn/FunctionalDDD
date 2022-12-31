namespace DomainDrivenDesign.Tests.ValueObjects;

using FluentAssertions;

public class ValueObjectTests
{
    [Fact]
    public void Two_VO_of_the_same_content_are_equal()
    {
        var address1 = new Address("Street", "City");
        var address2 = new Address("Street", "City");

        address1.Equals(address2).Should().BeTrue();
        address1.GetHashCode().Equals(address2.GetHashCode()).Should().BeTrue();
    }

    [Fact]
    public void Derived_value_objects_are_not_equal()
    {
        var address = new Address("Street", "City");
        var derivedAddress = new DerivedAddress("Country", "Street", "City");

        address.Equals(derivedAddress).Should().BeFalse();
        derivedAddress.Equals(address).Should().BeFalse();
    }

    [Fact]
    public void VO_is_sorted()
    {
        // Arrange
        var one = new Money(1);
        var two = new Money(2);
        var three = new Money(3);
        var moneys = new List<Money> { two, one, three };

        // Act
        moneys.Sort();

        // Assert
        moneys.Should().Equal(new List<Money> { one, two, three });

    }

    [Fact]
    public void It_is_possible_to_override_default_equality_comparison_behavior()
    {
        var money1 = new Money(2.2222m);
        var money2 = new Money(2.22m);

        money1.Equals(money2).Should().BeTrue();
        money1.GetHashCode().Equals(money2.GetHashCode()).Should().BeTrue();
    }

    [Fact]
    public void Comparing_simple_value_objects_of_different_values_returns_false()
    {
        var money1 = new Money(2.1m);
        var money2 = new Money(2.2m);

        money1.Equals(money2).Should().BeFalse();
    }

    [Fact]
    public void Comparing_less_than()
    {
        var money1 = new Money(2.1m);
        var money2 = new Money(2.2m);

        (money1 < money2).Should().BeTrue();
    }

    [Fact]
    public void Comparing_greater_than()
    {
        var money1 = new Money(2.1m);
        var money2 = new Money(2.2m);

        (money2 > money1).Should().BeTrue();
    }
}
