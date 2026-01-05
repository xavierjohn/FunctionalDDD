namespace Example.Tests;

using FunctionalDdd;

/// <summary>
/// Demonstrates parallel execution patterns using railway-oriented programming.
/// </summary>
public class ParallelExamples
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
