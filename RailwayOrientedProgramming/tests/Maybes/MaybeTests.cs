namespace RailwayOrientedProgramming.Tests.Maybes;

using FunctionalDdd;

public class MaybeTests
{
    [Fact]
    public void Can_create_a_nullable_maybe()
    {
        Maybe<MyClass> maybe = null;

        maybe.HasValue.Should().BeFalse();
        maybe.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void Can_create_a_maybe_none()
    {
        var maybe = Maybe.None<MyClass>();

        maybe.HasValue.Should().BeFalse();
        maybe.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void Nullable_maybe_is_same_as_maybe_none()
    {
        Maybe<MyClass> nullableMaybe = null;
        var maybeNone = Maybe.None<MyClass>();

        nullableMaybe.Should().Be(maybeNone);
    }

    [Fact]
    public void Cannot_access_Value_if_none()
    {
        Maybe<MyClass> maybe = null;

        Action action = () =>
        {
            var myClass = maybe.GetValueOrThrow();
        };

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Can_create_a_non_nullable_maybe()
    {
        var instance = new MyClass();

        Maybe<MyClass> maybe = instance;

        maybe.HasValue.Should().BeTrue();
        maybe.HasNoValue.Should().BeFalse();
        maybe.GetValueOrThrow().Should().Be(instance);
    }

    [Fact]
    public void ToString_returns_Empty_String_for_no_value()
    {
        Maybe<MyClass> maybe = null;

        var str = maybe.ToString();

        str.Should().Be(string.Empty);
    }

    [Fact]
    public void ToString_returns_underlying_objects_string_representation()
    {
        Maybe<MyClass> maybe = new MyClass();

        var str = maybe.ToString();

        str.Should().Be("My custom class");
    }

    [Fact]
    public void Maybe_None_has_no_value()
    {
        Maybe.None<string>().HasValue.Should().BeFalse();
        Maybe.None<int>().HasValue.Should().BeFalse();
    }

    [Fact]
    public void Maybe_None_Tuples_has_no_value_is_true()
    {
        Maybe.None<(Array, Exception)>().HasNoValue.Should().BeTrue();
        Maybe.None<(double, int, byte)>().HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void Maybe_None_Tuples_has_value_is_false()
    {
        Maybe.None<(DateTime, bool, char)>().HasValue.Should().BeFalse();
        Maybe.None<(string, TimeSpan)>().HasValue.Should().BeFalse();
    }

    [Fact]
    public void Maybe_From_without_type_parameter_creates_new_maybe()
    {
        var withoutTypeParam = Maybe.From("test");
        var withTypeParam = Maybe.From<string>("test");
        var differentValueTypeParam = Maybe.From<string>("tests");

        withoutTypeParam.Should().Be(withTypeParam);
        withoutTypeParam.Should().NotBe(differentValueTypeParam);
    }

    [Fact]
    public void Can_cast_non_generic_maybe_none_to_maybe_none()
    {
        Maybe<int> maybe = Maybe.None<int>();

        maybe.HasValue.Should().BeFalse();
        maybe.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void GetValueOrThrww_throws_with_message_if_source_is_empty()
    {
        const string errorMessage = "Maybe is none";

        Action action = () =>
        {
            var maybe = Maybe.None<int>();
            maybe.GetValueOrThrow(errorMessage);
        };

        action.Should().Throw<InvalidOperationException>().WithMessage(errorMessage);
    }

    [Fact]
    public void GetValueOrThrow_returns_value_if_source_has_value()
    {
        const int value = 5;
        var maybe = Maybe.From(value);

        const string errorMessage = "Maybe is none";
        var result = maybe.GetValueOrThrow(errorMessage);

        result.Should().Be(value);
    }

    [Fact]
    public void TryGetValue_returns_false_if_source_is_empty()
    {
        var maybe = Maybe.None<int>();

        var result = maybe.TryGetValue(out var value);

        result.Should().BeFalse();
        value.Should().Be(default);
    }

    [Fact]
    public void TryGetValue_returns_true_if_source_has_value()
    {
        var maybe = Maybe.From(5);

        var result = maybe.TryGetValue(out var value);

        result.Should().BeTrue();
        value.Should().Be(5);
    }

    [Fact]
    public void Maybe_struct_default_is_none()
    {
        Maybe<int> maybe = default;

        maybe.HasValue.Should().BeFalse();
        maybe.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void Maybe_struct_value_is_some()
    {
        Maybe<int> maybe = 5;

        maybe.HasValue.Should().BeTrue();
        maybe.HasNoValue.Should().BeFalse();
    }

    [Fact]
    public void Maybe_class_null_is_none()
    {
        Maybe<MyClass> maybe = null;

        maybe.HasValue.Should().BeFalse();
        maybe.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void Maybe_implicit_operator_converts_false_to_some_false()
    {
        Maybe<bool> m = false;

        m.HasValue.Should().BeTrue();
        m.GetValueOrThrow().Should().BeFalse();
    }

    private class MyClass
    {
        public override string ToString() => "My custom class";
    }
}
