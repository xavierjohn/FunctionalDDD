namespace Core.Tests.DomainDrivenDesign.ValueObjects;

using System.Collections.Generic;
using FunctionalDDD;

public class SimpleValueObjectTests
{
    internal class PasswordSimple : SimpleValueObject<string>
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

    internal class MoneySimple : SimpleValueObject<decimal>
    {
        public MoneySimple(decimal value) : base(value)
        {
        }
        protected override IEnumerable<IComparable> GetEqualityComponents()
        {
            yield return Math.Round(Value, 2);
        }
    }

    [Fact]
    public void Two_SVO_of_the_same_content_are_equal()
    {
        var password1 = new PasswordSimple("Password");
        var password2 = new PasswordSimple("Password");

        password1.Equals(password2).Should().BeTrue();
        password1.GetHashCode().Equals(password2.GetHashCode()).Should().BeTrue();
    }

    [Fact]
    public void Derived_simple_value_objects_are_not_equal()
    {
        var password = new PasswordSimple("Password");
        var derivedPassword = new DerivedPasswordSimple("Password");

        password.Equals(derivedPassword).Should().BeFalse();
        derivedPassword.Equals(password).Should().BeFalse();
    }

    [Fact]
    public void SVO_is_sorted()
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
}
