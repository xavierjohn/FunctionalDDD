namespace DomainDrivenDesign.Tests.Aggregates;

using FunctionalDdd;

public class AggregateTests
{
    [Fact]
    public void Aggregate_is_abstract() => typeof(Aggregate<>).IsAbstract.Should().BeTrue();

    [Fact]
    public void Aggregate_with_same_ids_is_equal()
    {
        User user1 = User.TryCreate("1", "John", "Doe", "john@doe.com");
        User user2 = User.TryCreate("1", "Jane", "Doe", "jane@doecom");

        user1.Should().Be(user2);
    }

    [Fact]
    public void Aggregate_with_different_ids_are_not_equal()
    {
        User user1 = User.TryCreate("1", "John", "Doe", "john@doe.com");
        User user2 = User.TryCreate("2", "John", "Doe", "john@doe.com");

        user1.Should().NotBe(user2);
    }
}

internal class User : Aggregate<string>
{
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }

    public static User TryCreate(string id, string firstName, string lastName, string email)
    {
        // Validate parameters.
        var user = new User(id, firstName, lastName, email);
        return user;
    }

    private User(string id, string firstName, string lastName, string email)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }
}
