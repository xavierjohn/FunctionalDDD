namespace BankingExample.ValueObjects;

using FunctionalDdd;

/// <summary>
/// Represents a monetary amount in the banking system.
/// </summary>
public class Money : ScalarValueObject<Money, decimal>, IScalarValueObject<Money, decimal>
{
    private Money(decimal value) : base(value) { }

    public static Result<Money> TryCreate(decimal amount)
    {
        if (amount < 0)
            return Error.Validation("Amount cannot be negative", nameof(amount));

        return new Money(Math.Round(amount, 2));
    }

    public Result<Money> Add(Money other)
    {
        return TryCreate(Value + other.Value);
    }

    public Result<Money> Subtract(Money other)
    {
        return TryCreate(Value - other.Value);
    }

    public bool IsGreaterThan(Money other) => Value > other.Value;

    public bool IsGreaterThanOrEqual(Money other) => Value >= other.Value;

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Math.Round(Value, 2);
    }

    public override string ToString() => $"${Value:F2}";

    public static Money Zero => new(0);
}
