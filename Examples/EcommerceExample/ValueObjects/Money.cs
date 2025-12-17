namespace EcommerceExample.ValueObjects;

using FunctionalDdd;

/// <summary>
/// Represents a monetary amount with currency and validation.
/// </summary>
public class Money : ScalarValueObject<decimal>
{
    public string Currency { get; }

    private Money(decimal amount, string currency) : base(amount)
    {
        Currency = currency;
    }

    public static Result<Money> TryCreate(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            return Error.Validation("Amount cannot be negative", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            return Error.Validation("Currency is required", nameof(currency));

        if (currency.Length != 3)
            return Error.Validation("Currency must be a 3-letter ISO code", nameof(currency));

        return new Money(Math.Round(amount, 2), currency.ToUpperInvariant());
    }

    public Result<Money> Add(Money other)
    {
        if (Currency != other.Currency)
            return Error.Validation($"Cannot add amounts with different currencies: {Currency} and {other.Currency}");

        return TryCreate(Value + other.Value, Currency);
    }

    public Result<Money> Multiply(int quantity)
    {
        if (quantity < 0)
            return Error.Validation("Quantity cannot be negative", nameof(quantity));

        return TryCreate(Value * quantity, Currency);
    }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Math.Round(Value, 2);
        yield return Currency;
    }

    public override string ToString() => $"{Value:F2} {Currency}";
}
