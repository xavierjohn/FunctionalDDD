namespace Asp.Tests.Validation;

using FunctionalDdd;

/// <summary>
/// Test struct value object for testing struct converter functionality.
/// Defined at namespace level to work with JSON serialization.
/// </summary>
public readonly struct TestStructValueObject : ITryCreatable<TestStructValueObject>
{
    public string Value { get; }

    private TestStructValueObject(string value) => Value = value;

    public static Result<TestStructValueObject> TryCreate(string? value, string? fieldName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<TestStructValueObject>(Error.Validation("Value is required", fieldName ?? "testStructValueObject"));

        return new TestStructValueObject(value);
    }

    public override string ToString() => Value;
}
