
namespace Example.Tests;
using FunctionalDDD;
using Xunit;

public class AsyncUsageExamples
{
    [Fact]
    public static async Task Promote_with_async_methods_in_the_beginning_of_the_chain()
    {
        var id = 1;

        var result = await GetByIdAsync(id)
            .ToResultAsync(Error.NotFound("Customer with such Id is not found: " + id))
            .EnsureAsync(customer => customer.CanBePromoted, Error.Validation("The customer has the highest status possible"))
            .TapAsync(customer => customer.Promote())
            .BindAsync(customer => EmailGateway.SendPromotionNotification(customer.Email))
            .FinallyAsync(result => result.IsSuccess ? "Okay" : result.Error.Message);

        result.Should().Be("Okay");
    }

    [Fact]
    public static async Task Promote_with_async_methods_in_the_beginning_and_in_the_middle_of_the_chain()
    {
        var id = 1;

        var result = await GetByIdAsync(id)
            .ToResultAsync(Error.NotFound("Customer with such Id is not found: " + id))
            .EnsureAsync(customer => customer.CanBePromoted, Error.Validation("The customer has the highest status possible"))
            .TapAsync(customer => customer.PromoteAsync())
            .BindAsync(customer => EmailGateway.SendPromotionNotificationAsync(customer.Email))
            .FinallyAsync(result => result.IsSuccess ? "Okay" : result.Error.Message);

        result.Should().Be("Okay");
    }

    [Fact]
    public static async Task Promote_with_async_methods_in_the_beginning_and_in_the_middle_of_the_chain_using_compensate()
    {
        var id = 1;

        var result = await GetByIdAsync(id)
            .ToResultAsync(Error.NotFound("Customer with such Id is not found: " + id))
            .EnsureAsync(customer => customer.CanBePromoted, Error.Validation("Need to ask manager"))
            .TapErrorAsync(error => Log(error))
            .OnFailureCompensateAsync(() => AskManagerAsync(id))
            .TapAsync(customer => Log("Manager approved promotion"))
            .TapAsync(customer => customer.PromoteAsync())
            .BindAsync(customer => EmailGateway.SendPromotionNotificationAsync(customer.Email))
            .FinallyAsync(result => result.IsSuccess ? "Okay" : result.Error.Message);

        result.Should().Be("Okay");
    }

    static void Log(ErrorList message)
    {
    }

    static void Log(string message)
    {
    }

    static Task<Result<Customer>> AskManagerAsync(long id) => Task.FromResult(Result.Success(new Customer()));

    public static Task<Maybe<Customer>> GetByIdAsync(long id) => Task.FromResult((Maybe<Customer>)new Customer());

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
        public static Result<Unit> SendPromotionNotification(string email) => Result.Success(new Unit());

        public static Task<Result<Unit>> SendPromotionNotificationAsync(string email) => Task.FromResult(SendPromotionNotification(email));
    }
}
