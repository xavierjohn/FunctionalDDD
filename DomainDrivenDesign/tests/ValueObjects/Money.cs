namespace DomainDrivenDesign.Tests.ValueObjects;

internal class Money : ScalarValueObject<Money, decimal>
{
    public Money(decimal value) : base(value)
    {
    }

    public static Result<Money> TryCreate(decimal value) =>
        Result.Success(new Money(value));

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Math.Round(Value, 2);
    }
}
