
namespace Example.Tests;

using FunctionalDdd;
using System.Diagnostics;
using Xunit;

public class AsyncUsageExamples : IClassFixture<TraceFixture>, IDisposable
{
    private readonly List<Activity> _completedActivities = [];
    private readonly ActivityListener _listener;

    public AsyncUsageExamples()
    {
        // Set up ActivityListener to capture all activities from both test and ROP sources
        _listener = new ActivityListener
        {
            ShouldListenTo = source =>
                source.Name == TraceFixture.ActivitySourceName ||
                source.Name == "Functional DDD ROP",  // ROP ActivitySource name
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = _completedActivities.Add
        };
        ActivitySource.AddActivityListener(_listener);
    }
    public void Dispose()
    {
        _listener?.Dispose();
        GC.SuppressFinalize(this);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Promote_customer(int id)
    {
        using var activity = TraceFixture.ActivitySource.StartActivity();
        var result = await GetCustomerByIdAsync(id)
            .ToResultAsync(Error.NotFound("Customer with such Id is not found: " + id))
            .EnsureAsync(customer => customer.CanBePromoted, Error.Validation("The customer has the highest status possible"))
            .TapAsync(customer => customer.Promote())
            .BindAsync(customer => EmailGateway.SendPromotionNotification(customer.Email))
            .MatchAsync(ok => "Okay", error => "Failed");

        if (id == 1)
            result.Should().Be("Okay");
        else
            result.Should().Be("Failed");

        // Analyze the complete trace tree
        _completedActivities.Should().HaveCount(5);
        
        var activities = _completedActivities;
        activities[0].OperationName.Should().Be("ToResult");
        activities[0].Status.Should().Be(ActivityStatusCode.Ok);

        var expectedStatus = id == 1 ? ActivityStatusCode.Ok : ActivityStatusCode.Error;
        var expectedOperations = new[] { "Ensure", "Tap", "Bind", "Match" };
        
        for (int i = 0; i < expectedOperations.Length; i++)
        {
            activities[i + 1].OperationName.Should().Be(expectedOperations[i]);
            activities[i + 1].Status.Should().Be(expectedStatus);
        }
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
            .MatchAsync(static ok => "Okay", static error => error.Detail);

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
            .MatchAsync(static ok => "Okay", static error => error.Detail);

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

        public bool Promoted { get; set; }

        public bool CanBePromoted { get; }

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
