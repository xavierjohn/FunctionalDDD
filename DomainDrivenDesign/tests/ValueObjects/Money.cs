namespace DomainDrivenDesign.Tests.ValueObjects;
using FunctionalDDD.Domain;

internal class Money : SimpleValueObject<decimal>
{
    public Money(decimal value) : base(value)
    {
    }
    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Math.Round(Value, 2);
    }
}
