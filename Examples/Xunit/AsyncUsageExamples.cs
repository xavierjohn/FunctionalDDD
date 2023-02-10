
namespace Example.Tests;
using FunctionalDDD;
using Xunit;

public class AsyncUsageExamples
{
    [Fact]
    public static async Task Promote_with_async_methods_in_the_beginning_of_the_chain()
    {
        var id = 1;

        var result = await GetCustomerByIdAsync(id)
            .ToResultAsync(Error.NotFound("Customer with such Id is not found: " + id))
            .EnsureAsync(customer => customer.CanBePromoted, Error.Validation("The customer has the highest status possible"))
            .TeeAsync(customer => customer.Promote())
            .BindAsync(customer => EmailGateway.SendPromotionNotification(customer.Email))
            .FinallyAsync(ok => "Okay", error => error.Message);

        result.Should().Be("Okay");
    }

    [Fact]
    public static async Task Promote_with_async_methods_in_the_beginning_and_in_the_middle_of_the_chain()
    {
        var id = 1;

        var result = await GetCustomerByIdAsync(id)
            .ToResultAsync(Error.NotFound("Customer with such Id is not found: " + id))
            .EnsureAsync(customer => customer.CanBePromoted, Error.Validation("The customer has the highest status possible"))
            .TeeAsync(customer => customer.PromoteAsync())
            .BindAsync(customer => EmailGateway.SendPromotionNotificationAsync(customer.Email))
            .FinallyAsync(ok => "Okay", error => error.Message);

        result.Should().Be("Okay");
    }

    [Fact]
    public static async Task Promote_with_async_methods_in_the_beginning_and_in_the_middle_of_the_chain_using_compensate()
    {
        var id = 1;

        var result = await GetCustomerByIdAsync(id)
            .ToResultAsync(Error.NotFound("Customer with such Id is not found: " + id))
            .EnsureAsync(customer => customer.CanBePromoted, Error.Validation("Need to ask manager"))
            .OnErrorTapAsync(error => Log(error))
            .OnErrorAsync(() => AskManagerAsync(id))
            .TeeAsync(customer => Log("Manager approved promotion"))
            .TeeAsync(customer => customer.PromoteAsync())
            .BindAsync(customer => EmailGateway.SendPromotionNotificationAsync(customer.Email))
            .FinallyAsync(ok => "Okay", error => error.Message);

        result.Should().Be("Okay");
    }

    static void Log(Error error)
    {
    }

    static void Log(string message)
    {
    }

    static Task<Result<Customer, Error>> AskManagerAsync(long id) => Task.FromResult(Result.Success<Customer, Error>(new Customer()));

    public static Task<Maybe<Customer>> GetCustomerByIdAsync(long id) => Task.FromResult((Maybe<Customer>)new Customer());

    public class Customer
    {
        public string Email { get; } = "random@universe.com";

        public bool Promoted { get; set; }
        public bool CanBePromoted { get; } = true;

        public void Promote()
        {
            Promoted = true;
        }
        public Task PromoteAsync()
        {
            Promoted = true;
            return Task.CompletedTask;
        }
    }

    public class EmailGateway
    {
        public static Result<Unit, Error> SendPromotionNotification(string email) => Result.Success<Unit, Error>(new Unit());

        public static Task<Result<Unit, Error>> SendPromotionNotificationAsync(string email) => Task.FromResult(SendPromotionNotification(email));
    }
}
