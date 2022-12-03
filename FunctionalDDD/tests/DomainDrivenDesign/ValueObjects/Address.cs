namespace FunctionalDDD.Tests.DomainDrivenDesign.ValueObjects;
using FunctionalDDD;

internal class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }

    public Address(string street, string city)
    {
        Street = street;
        City = city;
    }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
    }
}
