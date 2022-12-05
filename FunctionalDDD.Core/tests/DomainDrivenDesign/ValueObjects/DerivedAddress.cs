namespace FunctionalDDD.Core.Tests.DomainDrivenDesign.ValueObjects;
internal class DerivedAddress : Address
{
    public string Country { get; }

    public DerivedAddress(string street, string city, string country) : base(street, city)
    {
        Country = country;
    }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        foreach (string s in base.GetEqualityComponents())
        {
            yield return s;
        }

        yield return City;
    }
}
