namespace RailwayOrientedProgramming.Tests.Results.Extensions;

using FunctionalDdd.Testing;
using System.Diagnostics;
using System.Net;

/// <summary>
/// Real-world example demonstrating ParallelAsync with HTTP calls.
/// Shows handling of transient errors (retry) vs permanent errors (fail fast).
/// 
/// Scenario: E-commerce checkout calling multiple external services in parallel:
/// - User service (get user details)
/// - Inventory service (check stock)  
/// - Payment service (validate payment method)
/// - Shipping service (calculate rates)
/// </summary>
public class ParallelAsyncHttpRealWorldTests : TestBase
{
    #region Test Data & Helpers

    // Simulated HTTP responses
    private record UserResponse(string UserId, string Email, bool IsActive);
    private record InventoryResponse(string ProductId, int StockLevel, bool Available);
    private record PaymentResponse(string PaymentId, bool IsValid);
    private record ShippingResponse(string CarrierId, decimal Rate);

    // Checkout DTO
    private record CheckoutRequest(string UserId, string ProductId, string PaymentMethodId, string ShippingAddress);
    private record CheckoutResult(UserResponse User, InventoryResponse Inventory, PaymentResponse Payment, ShippingResponse Shipping);

    #endregion

    #region Success Scenarios

    [Fact]
    public async Task ParallelAsync_HttpCalls_AllServicesSucceed_ReturnsCheckoutData()
    {
        // Arrange - all services return success
        var request = new CheckoutRequest("user-123", "prod-456", "pm-789", "123 Main St");

        // Act - call 4 services in parallel using ParallelAsync
        var result = await Result.ParallelAsync(
            () => FetchUserAsync(request.UserId),
            () => CheckInventoryAsync(request.ProductId),
            () => ValidatePaymentAsync(request.PaymentMethodId),
            () => CalculateShippingAsync(request.ShippingAddress)
        )
        .AwaitAsync()
        .BindAsync((user, inventory, payment, shipping) =>
            Result.Success(new CheckoutResult(user, inventory, payment, shipping))
        );

        // Assert
        result.Should().BeSuccess();
        result.Value.User.UserId.Should().Be("user-123");
        result.Value.Inventory.Available.Should().BeTrue();
        result.Value.Payment.IsValid.Should().BeTrue();
        result.Value.Shipping.Rate.Should().BeGreaterThan(0);
    }

    #endregion

    #region Permanent Error Scenarios - Fail Fast (No Retry)

    [Fact]
    public async Task ParallelAsync_HttpCalls_UserNotFound_FailsFast()
    {
        // Arrange - user doesn't exist (404 - permanent error)
        var invalidUserId = "nonexistent-user";
        var request = new CheckoutRequest(invalidUserId, "prod-456", "pm-789", "123 Main St");

        // Act - ParallelAsync with 404 should fail immediately (no retry)
        var result = await Result.ParallelAsync(
            () => FetchUserAsync(invalidUserId),  // This will return 404 → permanent error
            () => CheckInventoryAsync(request.ProductId),
            () => ValidatePaymentAsync(request.PaymentMethodId),
            () => CalculateShippingAsync(request.ShippingAddress)
        ).AwaitAsync();

        // Assert - should fail with NotFoundError (permanent failure)
        result.Should().BeFailure();
        result.Error.Should().BeOfType<NotFoundError>();
        result.Error.Detail.Should().Contain("User not found");
    }

    [Fact]
    public async Task ParallelAsync_HttpCalls_OutOfStock_FailsFast()
    {
        // Arrange - product out of stock (conflict/domain error - permanent)
        var outOfStockProductId = "prod-out-of-stock";
        var request = new CheckoutRequest("user-123", outOfStockProductId, "pm-789", "123 Main St");

        // Act
        var result = await Result.ParallelAsync(
            () => FetchUserAsync(request.UserId),
            () => CheckInventoryAsync(outOfStockProductId),  // This returns conflict error
            () => ValidatePaymentAsync(request.PaymentMethodId),
            () => CalculateShippingAsync(request.ShippingAddress)
        ).AwaitAsync();

        // Assert - should fail with ConflictError (permanent business rule violation)
        result.Should().BeFailure();
        result.Error.Should().BeOfType<ConflictError>();
        result.Error.Detail.Should().Contain("Out of stock");
    }

    [Fact]
    public async Task ParallelAsync_HttpCalls_InvalidPayment_FailsFast()
    {
        // Arrange - payment method invalid (validation error - permanent)
        var invalidPaymentId = "pm-expired";
        var request = new CheckoutRequest("user-123", "prod-456", invalidPaymentId, "123 Main St");

        // Act
        var result = await Result.ParallelAsync(
            () => FetchUserAsync(request.UserId),
            () => CheckInventoryAsync(request.ProductId),
            () => ValidatePaymentAsync(invalidPaymentId),  // This returns validation error
            () => CalculateShippingAsync(request.ShippingAddress)
        ).AwaitAsync();

        // Assert - should fail with ValidationError (permanent business rule)
        result.Should().BeFailure();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error.Detail.Should().Contain("expired");
    }

    [Fact]
    public async Task ParallelAsync_HttpCalls_MultipleServiceFailures_CombinesErrors()
    {
        // Arrange - multiple services fail with different error types
        var request = new CheckoutRequest("nonexistent-user", "prod-out-of-stock", "pm-expired", "123 Main St");

        // Act - 3 out of 4 services fail
        var result = await Result.ParallelAsync(
            () => FetchUserAsync("nonexistent-user"),      // NotFoundError
            () => CheckInventoryAsync("prod-out-of-stock"), // ConflictError
            () => ValidatePaymentAsync("pm-expired"),       // ValidationError
            () => CalculateShippingAsync(request.ShippingAddress)  // Success
        ).AwaitAsync();

        // Assert - should aggregate different error types
        result.Should().BeFailure();
        result.Error.Should().BeOfType<AggregateError>();
        var aggregateError = (AggregateError)result.Error;
        aggregateError.Errors.Should().HaveCount(3);
        aggregateError.Errors.Should().ContainSingle(e => e is NotFoundError);
        aggregateError.Errors.Should().ContainSingle(e => e is ConflictError);
        aggregateError.Errors.Should().ContainSingle(e => e is ValidationError);
    }

    #endregion

    #region Transient Error Scenarios - With Retry (Using RecoverOnFailureAsync)

    [Fact]
    public async Task ParallelAsync_HttpCalls_TransientError_RetriesAndSucceeds()
    {
        // Arrange - simulate service that fails once then succeeds (transient 503 error)
        var attemptCount = 0;
        
        async Task<Result<InventoryResponse>> CheckInventoryWithRetry(string productId)
        {
            attemptCount++;
            if (attemptCount == 1)
            {
                // First attempt: transient error (503 Service Unavailable)
                await Task.Delay(10); // Simulate network delay
                return Error.ServiceUnavailable("Inventory service temporarily unavailable");
            }
            
            // Retry succeeds
            await Task.Delay(10);
            return Result.Success(new InventoryResponse(productId, 100, true));
        }

        var request = new CheckoutRequest("user-123", "prod-456", "pm-789", "123 Main St");

        // Act - use RecoverOnFailureAsync to retry transient errors
        var result = await Result.ParallelAsync(
            () => FetchUserAsync(request.UserId),
            () => CheckInventoryWithRetry(request.ProductId)
                .RecoverOnFailureAsync(
                    predicate: error => error is ServiceUnavailableError,
                    funcAsync: async () =>
                    {
                        await Task.Delay(50); // Backoff delay
                        return await CheckInventoryWithRetry(request.ProductId); // Retry
                    }),
            () => ValidatePaymentAsync(request.PaymentMethodId),
            () => CalculateShippingAsync(request.ShippingAddress)
        ).AwaitAsync();

        // Assert - should succeed after retry
        result.Should().BeSuccess();
        attemptCount.Should().Be(2); // Verify retry happened (first call + one retry)
    }

    [Fact]
    public async Task ParallelAsync_HttpCalls_TransientError_RetriesMultipleTimes()
    {
        // Arrange - service fails twice then succeeds
        var attemptCount = 0;
        
        async Task<Result<PaymentResponse>> ValidatePaymentWithRetries(string paymentId)
        {
            attemptCount++;
            if (attemptCount <= 2)
            {
                // Fail first 2 attempts (transient)
                await Task.Delay(10);
                return Error.ServiceUnavailable("Payment service timeout");
            }
            
            // Third attempt succeeds
            await Task.Delay(10);
            return Result.Success(new PaymentResponse(paymentId, true));
        }

        // Act - chain RecoverOnFailureAsync for multiple retries
        var result = await ValidatePaymentWithRetries("pm-789")
            .RecoverOnFailureAsync(
                predicate: error => error is ServiceUnavailableError,
                funcAsync: async () =>
                {
                    await Task.Delay(50); // First retry delay
                    return await ValidatePaymentWithRetries("pm-789")
                        .RecoverOnFailureAsync(
                            predicate: error => error is ServiceUnavailableError,
                            funcAsync: async () =>
                            {
                                await Task.Delay(100); // Second retry delay (exponential backoff)
                                return await ValidatePaymentWithRetries("pm-789");
                            });
                });

        // Assert - should succeed on 3rd attempt
        result.Should().BeSuccess();
        attemptCount.Should().Be(3);
    }

    [Fact]
    public async Task ParallelAsync_HttpCalls_TransientError_ExceedsMaxRetries_FailsPermanently()
    {
        // Arrange - service always returns transient error
        var attemptCount = 0;
        
        async Task<Result<ShippingResponse>> CalculateShippingAlwaysFails(string address)
        {
            attemptCount++;
            await Task.Delay(10);
            return Error.ServiceUnavailable("Shipping service down");
        }

        // Act - try with 2 retries (3 total attempts)
        var result = await CalculateShippingAlwaysFails("123 Main St")
            .RecoverOnFailureAsync(
                predicate: error => error is ServiceUnavailableError,
                funcAsync: async () =>
                {
                    await Task.Delay(50); // First retry
                    return await CalculateShippingAlwaysFails("123 Main St")
                        .RecoverOnFailureAsync(
                            predicate: error => error is ServiceUnavailableError,
                            funcAsync: async () =>
                            {
                                await Task.Delay(100); // Second retry
                                return await CalculateShippingAlwaysFails("123 Main St");
                                // No more retries after this
                            });
                });

        // Assert - should fail after all retries exhausted
        result.Should().BeFailure();
        result.Error.Should().BeOfType<ServiceUnavailableError>();
        attemptCount.Should().Be(3); // Initial + 2 retries
    }

    #endregion

    #region Performance & Timing

    [Fact]
    public async Task ParallelAsync_HttpCalls_ExecutesInParallel_NotSequentially()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();
        var request = new CheckoutRequest("user-123", "prod-456", "pm-789", "123 Main St");

        // Act - each service takes ~50ms, run in parallel
        var result = await Result.ParallelAsync(
            () => FetchUserAsync(request.UserId),            // ~50ms
            () => CheckInventoryAsync(request.ProductId),     // ~50ms
            () => ValidatePaymentAsync(request.PaymentMethodId), // ~50ms
            () => CalculateShippingAsync(request.ShippingAddress) // ~50ms
        ).AwaitAsync();

        stopwatch.Stop();

        // Assert - parallel execution should take ~50ms (not 200ms sequential)
        result.Should().BeSuccess();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(120); // Allow margin for CI/slow machines
        
        // If sequential, would take 4 * 50ms = 200ms+
        // Parallel should be ~50ms (longest single operation)
    }

    #endregion

    #region Helper Methods - Simulated HTTP Calls

    private static async Task<Result<UserResponse>> FetchUserAsync(string userId)
    {
        await Task.Delay(50); // Simulate network latency

        return userId switch
        {
            "nonexistent-user" => Error.NotFound($"User not found: {userId}"),
            "user-123" => Result.Success(new UserResponse(userId, "user@example.com", true)),
            _ => Error.Unexpected("Unknown user ID")
        };
    }

    private static async Task<Result<InventoryResponse>> CheckInventoryAsync(string productId)
    {
        await Task.Delay(50);

        return productId switch
        {
            "prod-out-of-stock" => Error.Conflict("Out of stock: product is unavailable"),
            "prod-456" => Result.Success(new InventoryResponse(productId, 100, true)),
            _ => Error.NotFound($"Product not found: {productId}")
        };
    }

    private static async Task<Result<PaymentResponse>> ValidatePaymentAsync(string paymentMethodId)
    {
        await Task.Delay(50);

        return paymentMethodId switch
        {
            "pm-expired" => Error.Validation("Payment method expired"),
            "pm-789" => Result.Success(new PaymentResponse(paymentMethodId, true)),
            _ => Error.NotFound($"Payment method not found: {paymentMethodId}")
        };
    }

    private static async Task<Result<ShippingResponse>> CalculateShippingAsync(string address)
    {
        await Task.Delay(50);

        if (string.IsNullOrWhiteSpace(address))
            return Error.Validation("Shipping address required");

        return Result.Success(new ShippingResponse("USPS", 9.99m));
    }

    #endregion
}
