namespace BankingExample.ValueObjects;

using FunctionalDdd;

/// <summary>
/// Represents a monetary amount in the banking system with a currency.
/// </summary>
public class Money : ValueObject
{
    public decimal Amount { get; }
    public CurrencyCode Currency { get; }

    private Money(decimal amount, CurrencyCode currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Result<Money> TryCreate(decimal amount, string? fieldName = null) => 
        TryCreate(amount, "USD", fieldName);

    public static Result<Money> TryCreate(decimal amount, string currencyCode, string? fieldName = null)
    {
        var field = fieldName ?? "amount";
        if (amount < 0)
            return Error.Validation("Amount cannot be negative", field);

        return CurrencyCode.TryCreate(currencyCode)
            .Map(currency => new Money(Math.Round(amount, 2), currency));
    }

    public Result<Money> Add(Money other)
    {
        if (!Currency.Equals(other.Currency))
            return Error.Validation($"Cannot add amounts with different currencies: {Currency} and {other.Currency}");

        return TryCreate(Amount + other.Amount, Currency.Value);
    }

    public Result<Money> Subtract(Money other)
    {
        if (!Currency.Equals(other.Currency))
            return Error.Validation($"Cannot subtract amounts with different currencies: {Currency} and {other.Currency}");

        return TryCreate(Amount - other.Amount, Currency.Value);
    }

    public bool IsGreaterThan(Money other) => 
        Currency.Equals(other.Currency) && Amount > other.Amount;

    public bool IsGreaterThanOrEqual(Money other) => 
        Currency.Equals(other.Currency) && Amount >= other.Amount;

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Math.Round(Amount, 2);
        yield return Currency.Value;
    }

    public override string ToString() => $"{Amount:F2} {Currency}";

    public static Result<Money> Zero(string currencyCode = "USD") => TryCreate(0, currencyCode);
}
