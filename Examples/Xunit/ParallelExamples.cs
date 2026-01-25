namespace Example.Tests;

using FunctionalDdd;
using System.Diagnostics;
using Xunit;

/// <summary>
/// Examples demonstrating parallel execution of async Result operations using Result.ParallelAsync.
/// These operations start simultaneously and complete efficiently.
/// </summary>
public class ParallelExamples : IClassFixture<TraceFixture>
{
    [Fact]
    public async Task ParallelExecution_Success_Example()
    {
        var orderId = "12345";
        var result = await Result.ParallelAsync(
            () => CheckInventoryAsync(orderId),
            () => ValidatePaymentAsync(orderId),
            () => CalculateShippingAsync(orderId)
        ).AwaitAsync();

        result.IsSuccess.Should().BeTrue();
        var (inventory, payment, shipping) = result.Value;
        inventory.Should().Be("Inventory OK");
        payment.Should().Be("Payment OK");
        shipping.Should().Be("Shipping OK");
    }

    [Fact]
    public async Task ParallelExecution_OneFailure_ReturnsError()
    {
        var orderId = "invalid-payment"; // Invalid payment order ID

        var result = await Result.ParallelAsync(
            () => CheckInventoryAsync(orderId),
            () => ValidatePaymentAsync(orderId), // This will fail
            () => CalculateShippingAsync(orderId)
        ).AwaitAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error.Detail.Should().Contain("Invalid payment");
    }

    [Fact]
    public async Task ParallelExecution_ThreeTasks_CompletesFaster()
    {
        var userId = "user-123";

        var result = await Result.ParallelAsync(
            () => FetchUserProfileAsync(userId),
            () => FetchUserPreferencesAsync(userId),
            () => FetchUserStatsAsync(userId)
        ).AwaitAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ParallelExecution_FiveTasks_Example()
    {
        var studentId = "student-123";

        var result = await Result.ParallelAsync(
            () => FetchStudentInfoAsync(studentId),
            () => FetchStudentGradesAsync(studentId),
            () => FetchStudentAttendanceAsync(studentId),
            () => FetchLibraryBooksAsync(studentId),
            () => FetchExtracurricularActivitiesAsync(studentId)
        ).AwaitAsync();

        result.IsSuccess.Should().BeTrue();
        var (info, grades, attendance, books, activities) = result.Value;
        info.Should().NotBeNullOrEmpty();
        grades.Should().NotBeNullOrEmpty();
        attendance.Should().NotBeNullOrEmpty();
        books.Should().NotBeNullOrEmpty();
        activities.Should().NotBeNullOrEmpty();
    }

    // ----- Helper Methods -----

    private static Task<Result<string>> CheckInventoryAsync(string orderId) =>
        Task.FromResult(Result.Success("Inventory OK"));

    private static Task<Result<string>> ValidatePaymentAsync(string orderId)
    {
        if (orderId == "invalid-payment")
            return Task.FromResult(Result.Failure<string>(Error.Validation("Invalid payment")));

        return Task.FromResult(Result.Success("Payment OK"));
    }

    private static Task<Result<string>> CalculateShippingAsync(string orderId) =>
        Task.FromResult(Result.Success("Shipping OK"));

    private static Task<Result<string>> FetchUserProfileAsync(string userId) =>
        Task.FromResult(Result.Success($"Profile for {userId}"));

    private static Task<Result<string>> FetchUserPreferencesAsync(string userId) =>
        Task.FromResult(Result.Success($"Preferences for {userId}"));

    private static Task<Result<string>> FetchUserStatsAsync(string userId) =>
        Task.FromResult(Result.Success($"Stats for {userId}"));

    private static Task<Result<string>> FetchStudentInfoAsync(string studentId) =>
        Task.FromResult(Result.Success($"Student info for {studentId}"));

    private static Task<Result<string>> FetchStudentGradesAsync(string studentId) =>
        Task.FromResult(Result.Success($"Grades for {studentId}"));

    private static Task<Result<string>> FetchStudentAttendanceAsync(string studentId) =>
        Task.FromResult(Result.Success($"Attendance for {studentId}"));

    private static Task<Result<string>> FetchLibraryBooksAsync(string studentId) =>
        Task.FromResult(Result.Success($"Books for {studentId}"));

    private static Task<Result<string>> FetchExtracurricularActivitiesAsync(string studentId) =>
        Task.FromResult(Result.Success($"Activities for {studentId}"));

    private static async Task<Result<string>> BuildUserDashboardAsync((string profile, string preferences, string orders) data)
    {
        await Task.Delay(10);
        var userId = data.profile.Split(' ')[2];
        return Result.Success($"Dashboard for {userId} with {data.preferences} and {data.orders}");
    }
}