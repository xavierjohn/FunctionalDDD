namespace FunctionalDDD.Tests.DomainDrivenDesign.ValueObjects;
using FunctionalDDD;

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
