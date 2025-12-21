namespace Example.Tests;

using FunctionalDdd;

/// <summary>
/// Demonstrates parallel execution and retry patterns using railway-oriented programming.
/// </summary>
public class ParallelAndRetryExamples
{
    [Fact]
    public async Task Execute_three_operations_in_parallel_all_succeed()
    {
        // Arrange
        var orderId = "order-123";

        // Act - Use ParallelAsync to execute operations concurrently
        var result = await ValidateInventoryAsync(orderId)
            .ParallelAsync(ValidatePaymentAsync(orderId))
            .ParallelAsync(ValidateShippingAddressAsync(orderId))
            .AwaitAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        var (inventory, payment, shipping) = result.Value;
        inventory.Should().Be("Inventory validated");
        payment.Should().Be("Payment validated");
        shipping.Should().Be("Shipping validated");
    }

    [Fact]
    public async Task Execute_three_operations_in_parallel_one_fails()
    {
        // Arrange
        var orderId = "invalid-payment";

        // Act - ParallelAsync with failure will aggregate errors
        var result = await ValidateInventoryAsync(orderId)
            .ParallelAsync(ValidatePaymentAsync(orderId)) // This will fail
            .ParallelAsync(ValidateShippingAddressAsync(orderId))
            .AwaitAsync();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("validation.error");
        result.Error.Detail.Should().Contain("Payment validation failed");
    }

    [Fact]
    public async Task Retry_transient_failures_succeeds_on_second_attempt()
    {
        // Arrange
        var attempt = 0;

        // Simulate transient failure that succeeds on retry
        async Task<Result<string>> FlakeyOperation()
        {
            attempt++;
            if (attempt == 1)
                return Result.Failure<string>(Error.ServiceUnavailable("Service temporarily unavailable"));

            await Task.Delay(10);
            return Result.Success("Operation succeeded");
        }

        // Act - Retry up to 3 times
        var result = await RetryExtensions.RetryAsync(
            FlakeyOperation,
            maxRetries: 3,
            initialDelay: TimeSpan.FromMilliseconds(10));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Operation succeeded");
        attempt.Should().Be(2); // Failed once, succeeded on second attempt
    }

    [Fact]
    public async Task Retry_exhausts_all_attempts_and_fails()
    {
        // Arrange
        var attempt = 0;

        // Simulate persistent failure
        async Task<Result<string>> AlwaysFailsOperation()
        {
            attempt++;
            await Task.Delay(10);
            return Result.Failure<string>(Error.ServiceUnavailable($"Attempt {attempt} failed"));
        }

        // Act - Retry up to 3 times, all fail
        var result = await RetryExtensions.RetryAsync(
            AlwaysFailsOperation,
            maxRetries: 3,
            initialDelay: TimeSpan.FromMilliseconds(10));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Detail.Should().Contain("Attempt 4 failed"); // Initial + 3 retries = 4 attempts
        attempt.Should().Be(4); // Initial attempt + 3 retries
    }

    [Fact]
    public async Task Parallel_execution_followed_by_sequential_processing()
    {
        // Arrange
        var userId = "user-123";

        // Act - Fetch user data in parallel, then process sequentially
        var result = await FetchUserProfileAsync(userId)
            .ParallelAsync(FetchUserPreferencesAsync(userId))
            .ParallelAsync(FetchUserOrdersAsync(userId))
            .AwaitAsync()
            .BindAsync(BuildUserDashboardAsync);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Dashboard for user-123");
    }

    [Fact]
    public async Task Traverse_collection_processes_all_items_successfully()
    {
        // Arrange
        var orderIds = new[] { "order-1", "order-2", "order-3" };

        // Act - Process each order, collecting results
        var result = await orderIds
            .TraverseAsync((string orderId) => Task.FromResult(Result.Success($"Processed {orderId}")));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value.Should().Contain("Processed order-1");
        result.Value.Should().Contain("Processed order-2");
        result.Value.Should().Contain("Processed order-3");
    }

    [Fact]
    public async Task Traverse_collection_short_circuits_on_first_failure()
    {
        // Arrange
        var orderIds = new[] { "order-1", "invalid-order", "order-3" };

        // Act - Process until first failure
        var result = await orderIds
            .TraverseAsync((string orderId) =>
            {
                if (orderId == "invalid-order")
                    return Task.FromResult(Result.Failure<string>(Error.NotFound($"Order {orderId} not found")));

                return Task.FromResult(Result.Success($"Processed {orderId}"));
            });

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Detail.Should().Contain("invalid-order");
    }

    [Fact]
    public async Task Retry_with_compensate_provides_fallback_on_final_failure()
    {
        // Arrange
        var attempt = 0;

        async Task<Result<string>> UnreliableService()
        {
            attempt++;
            await Task.Delay(10);
            return Result.Failure<string>(Error.ServiceUnavailable("Primary service down"));
        }

        async Task<Result<string>> FallbackService()
        {
            await Task.Delay(10);
            return Result.Success("Fallback service response");
        }

        // Act - Retry primary service, then fall back to secondary
        var result = await RetryExtensions.RetryAsync(
                UnreliableService,
                maxRetries: 2,
                initialDelay: TimeSpan.FromMilliseconds(10))
            .CompensateAsync(FallbackService);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Fallback service response");
        attempt.Should().Be(3); // Initial attempt + 2 retries
    }

    [Fact]
    public async Task Retry_only_specific_error_types()
    {
        // Arrange
        var attempt = 0;

        async Task<Result<string>> ServiceWithValidationError()
        {
            attempt++;
            await Task.Delay(10);
            
            if (attempt == 1)
                return Result.Failure<string>(Error.ServiceUnavailable("Temporary issue")); // Should retry
            
            return Result.Failure<string>(Error.Validation("Invalid input")); // Should not retry
        }

        // Act - Only retry service unavailable errors, not validation errors
        var result = await RetryExtensions.RetryAsync(
            ServiceWithValidationError,
            maxRetries: 3,
            initialDelay: TimeSpan.FromMilliseconds(10),
            shouldRetry: error => error is ServiceUnavailableError);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
        attempt.Should().Be(2); // Initial + 1 retry, then stopped on validation error
    }

    [Fact]
    public async Task Parallel_with_five_operations()
    {
        // Arrange
        var studentId = "student-456";

        // Act - Execute 5 independent operations in parallel
        var result = await FetchStudentInfoAsync(studentId)
            .ParallelAsync(FetchStudentGradesAsync(studentId))
            .ParallelAsync(FetchStudentAttendanceAsync(studentId))
            .ParallelAsync(FetchLibraryBooksAsync(studentId))
            .ParallelAsync(FetchExtracurricularsAsync(studentId))
            .AwaitAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        var (info, grades, attendance, books, activities) = result.Value;
        info.Should().Contain("student-456");
        grades.Should().Contain("Grades");
        attendance.Should().Contain("Attendance");
        books.Should().Contain("Books");
        activities.Should().Contain("Activities");
    }

    // ----- Helper Methods -----

    private static Task<Result<string>> ValidateInventoryAsync(string orderId) =>
        Task.FromResult(Result.Success("Inventory validated"));

    private static Task<Result<string>> ValidatePaymentAsync(string orderId)
    {
        if (orderId == "invalid-payment")
            return Task.FromResult(Result.Failure<string>(Error.Validation("Payment validation failed")));

        return Task.FromResult(Result.Success("Payment validated"));
    }

    private static Task<Result<string>> ValidateShippingAddressAsync(string orderId) =>
        Task.FromResult(Result.Success("Shipping validated"));

    private static Task<Result<string>> FetchUserProfileAsync(string userId) =>
        Task.FromResult(Result.Success($"Profile for {userId}"));

    private static Task<Result<string>> FetchUserPreferencesAsync(string userId) =>
        Task.FromResult(Result.Success($"Preferences for {userId}"));

    private static Task<Result<string>> FetchUserOrdersAsync(string userId) =>
        Task.FromResult(Result.Success($"Orders for {userId}"));

    private static Task<Result<string>> FetchStudentInfoAsync(string studentId) =>
        Task.FromResult(Result.Success($"Student info for {studentId}"));

    private static Task<Result<string>> FetchStudentGradesAsync(string studentId) =>
        Task.FromResult(Result.Success($"Grades for {studentId}"));

    private static Task<Result<string>> FetchStudentAttendanceAsync(string studentId) =>
        Task.FromResult(Result.Success($"Attendance for {studentId}"));

    private static Task<Result<string>> FetchLibraryBooksAsync(string studentId) =>
        Task.FromResult(Result.Success($"Books for {studentId}"));

    private static Task<Result<string>> FetchExtracurricularsAsync(string studentId) =>
        Task.FromResult(Result.Success($"Activities for {studentId}"));

    private static async Task<Result<string>> BuildUserDashboardAsync((string profile, string preferences, string orders) data)
    {
        await Task.Delay(10);
        var userId = data.profile.Split(' ')[2];
        return Result.Success($"Dashboard for {userId} with {data.preferences} and {data.orders}");
    }
}
