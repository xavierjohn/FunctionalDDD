﻿
namespace Example.Tests;
using FunctionalDdd;
using Xunit;

public class AsyncUsageExamples : IClassFixture<TraceFixture>
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public static async Task Promote_customer(int id)
    {
        using var activity = TraceFixture.ActivitySource.StartActivity();
        var result = await GetCustomerByIdAsync(id)
            .ToResultAsync(Error.NotFound("Customer with such Id is not found: " + id))
            .EnsureAsync(customer => customer.CanBePromoted, Error.Validation("The customer has the highest status possible"))
            .TapAsync(customer => customer.Promote())
            .BindAsync(customer => EmailGateway.SendPromotionNotification(customer.Email))
            .FinallyAsync(ok => "Okay", error => "Failed");

        if (id == 1)
            result.Should().Be("Okay");
        else
            result.Should().Be("Failed");
    }

    [Fact]
    public static async Task PromoteAsync_customer()
    {
        var id = 1;

        using var activity = TraceFixture.ActivitySource.StartActivity();

        var result = await GetCustomerByIdAsync(id)
            .ToResultAsync(Error.NotFound("Customer with such Id is not found: " + id))
            .EnsureAsync(static customer => customer.CanBePromoted, Error.Validation("The customer has the highest status possible"))
            .TapAsync(static customer => customer.PromoteAsync())
            .BindAsync(static customer => EmailGateway.SendPromotionNotificationAsync(customer.Email))
            .FinallyAsync(static ok => "Okay", static error => error.Message);

        result.Should().Be("Okay");
    }

    [Fact]
    public static async Task Ask_manager_for_promotion()
    {
        var id = 1;

        using var activity = TraceFixture.ActivitySource.StartActivity();

        var result = await GetCustomerByIdAsync(id)
            .ToResultAsync(Error.NotFound("Customer with such Id is not found: " + id))
            .EnsureAsync(customer => customer.CanBePromoted, Error.Validation("Need to ask manager"))
            .TapErrorAsync(Log)
            .CompensateAsync(() => AskManagerAsync(id))
            .TapAsync(static customer => Log("Manager approved promotion"))
            .TapAsync(static customer => customer.PromoteAsync())
            .BindAsync(static customer => EmailGateway.SendPromotionNotificationAsync(customer.Email))
            .FinallyAsync(static ok => "Okay", static error => error.Message);

        result.Should().Be("Okay");
    }

    static void Log(Error error)
    {
    }

    static void Log(string message)
    {
    }

    static Task<Result<Customer>> AskManagerAsync(long id) => Task.FromResult(Result.Success(new Customer(true)));

    public static Task<Customer?> GetCustomerByIdAsync(long id)
    {
        var customer = id switch
        {
            1 => new Customer(true),
            2 => new Customer(false),
            _ => null
        };

        return Task.FromResult(customer);
    }

    public class Customer
    {
        public Customer(bool canBePromoted) => CanBePromoted = canBePromoted;

        public string Email { get; } = "random@universe.com";

        public bool CanBePromoted { get; }

        public bool Promoted { get; set; }

        public void Promote() => Promoted = true;

        public Task PromoteAsync()
        {
            Promoted = true;
            return Task.CompletedTask;
        }
    }

    public class EmailGateway
    {
        public static Result<Unit> SendPromotionNotification(string email) => Result.Success<Unit>(new Unit());

        public static Task<Result<Unit>> SendPromotionNotificationAsync(string email) => Task.FromResult(SendPromotionNotification(email));
    }
}
