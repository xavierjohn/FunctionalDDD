namespace DomainDrivenDesign.Tests.ValueObjects;

internal class Money : ScalarValueObject<decimal>
{
    public Money(decimal value) : base(value)
    {
    }
    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Math.Round(Value, 2);
    }
}
