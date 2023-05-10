﻿namespace RailwayOrientedProgramming.Tests.Maybes;

using FunctionalDDD;

public class EqualityTests
{
    [Fact]
    public void Two_maybes_of_the_same_content_are_equal()
    {
        var instance = new MyClass();
        Maybe<MyClass> maybe1 = instance;
        Maybe<MyClass> maybe2 = instance;

        var equals1 = maybe1.Equals(maybe2);
        var equals2 = ((object)maybe1).Equals(maybe2);
        var equals3 = maybe1 == maybe2;
        var equals4 = maybe1 != maybe2;
        var equals5 = maybe1.GetHashCode() == maybe2.GetHashCode();

        equals1.Should().BeTrue();
        equals2.Should().BeTrue();
        equals3.Should().BeTrue();
        equals4.Should().BeFalse();
        equals5.Should().BeTrue();
    }

    [Fact]
    public void Two_maybes_are_not_equal_if_differ()
    {
        Maybe<MyClass> maybe1 = new MyClass();
        Maybe<MyClass> maybe2 = new MyClass();

        var equals1 = maybe1.Equals(maybe2);
        var equals2 = ((object)maybe1).Equals(maybe2);
        var equals3 = maybe1 == maybe2;
        var equals4 = maybe1 != maybe2;
        var equals5 = maybe1.GetHashCode() == maybe2.GetHashCode();

        equals1.Should().BeFalse();
        equals2.Should().BeFalse();
        equals3.Should().BeFalse();
        equals4.Should().BeTrue();
        equals5.Should().BeFalse();
    }

    [Fact]
    public void Two_empty_maybes_are_equal()
    {
        Maybe<MyClass> maybe1 = null;
        Maybe<MyClass> maybe2 = null;

        var equals1 = maybe1.Equals(maybe2);
        var equals2 = ((object)maybe1).Equals(maybe2);
        var equals3 = maybe1 == maybe2;
        var equals4 = maybe1 != maybe2;
        var equals5 = maybe1.GetHashCode() == maybe2.GetHashCode();

        equals1.Should().BeTrue();
        equals2.Should().BeTrue();
        equals3.Should().BeTrue();
        equals4.Should().BeFalse();
        equals5.Should().BeTrue();
    }

    [Fact]
    public void Two_maybes_are_not_equal_if_one_of_them_empty()
    {
        Maybe<MyClass> maybe1 = new MyClass();
        Maybe<MyClass> maybe2 = null;

        var equals1 = maybe1.Equals(maybe2);
        var equals2 = ((object)maybe1).Equals(maybe2);
        var equals3 = maybe1 == maybe2;
        var equals4 = maybe1 != maybe2;
        var equals5 = maybe1.GetHashCode() == maybe2.GetHashCode();

        equals1.Should().BeFalse();
        equals2.Should().BeFalse();
        equals3.Should().BeFalse();
        equals4.Should().BeTrue();
        equals5.Should().BeFalse();
    }

    [Fact]
    public void Can_compare_maybe_to_underlying_type()
    {
        var instance = new MyClass();
        Maybe<MyClass> maybe = instance;

        var equals1 = maybe.Equals(instance);
        var equals2 = ((object)maybe).Equals(instance);
        var equals3 = maybe == instance;
        var equals4 = maybe != instance;
        var equals5 = maybe.GetHashCode() == instance.GetHashCode();

        equals1.Should().BeTrue();
        equals2.Should().BeTrue();
        equals3.Should().BeTrue();
        equals4.Should().BeFalse();
        equals5.Should().BeTrue();
    }

    [Fact]
    public void Can_compare_underlying_type_to_maybe()
    {
        var instance = new MyClass();
        Maybe<MyClass> maybe = instance;

        var equals1 = instance == maybe;
        var equals2 = instance != maybe;

        equals1.Should().BeTrue();
        equals2.Should().BeFalse();
    }

    [Fact]
    public void Can_compare_maybe_of_object()
    {
        var instance = new object();
        Maybe<object> maybe = instance;

        var equals1 = maybe.Equals(instance);
        var equals2 = ((object)maybe).Equals(instance);
        var equals3 = maybe == instance;
        var equals4 = maybe != instance;
        var equals5 = maybe.GetHashCode() == instance.GetHashCode();

        equals1.Should().BeTrue();
        equals2.Should().BeTrue();
        equals3.Should().BeTrue();
        equals4.Should().BeFalse();
        equals5.Should().BeTrue();
    }

    [Fact]
    public void Compare_with_null_value()
    {
        Maybe<MyClass> maybe = null;

        var result = maybe == null!;

        result.Should().BeTrue();
    }

    private class MyClass
    {
    }
}
