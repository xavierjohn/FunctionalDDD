using BankingExample.Aggregates;
using BankingExample.Services;
using BankingExample.ValueObjects;
using BankingExample.Workflows;
using FunctionalDdd;

namespace BankingExample;

/// <summary>
/// Demonstrates banking operations with Railway Oriented Programming and Domain-Driven Design.
/// 
/// Features demonstrated:
/// - Result&lt;T&gt; and ROP patterns (Bind, Map, Ensure, Tap, Match)
/// - Aggregates with domain events
/// - Change tracking with UncommittedEvents and AcceptChanges
/// - Various error types (Validation, Domain, Conflict, Unauthorized)
/// - Async workflows with ParallelAsync and CompensateAsync
/// - Value objects with validation
/// </summary>
public class BankingExamples
{
    public static async Task RunExamplesAsync()
    {
        Console.WriteLine("=== Banking Transaction Examples ===");
        Console.WriteLine("=== Demonstrating FunctionalDDD Library ===\n");

        await Example1_BasicAccountOperationsWithEvents();
        await Example2_TransferBetweenAccountsWithEvents();
        await Example3_FraudDetection();
        await Example4_DailyWithdrawalLimit();
        await Example5_InterestPayment();
        await Example6_DomainEventsAndChangeTracking();
    }

    /// <summary>
    /// Example 1: Basic account operations with domain events.
    /// Demonstrates: Aggregate creation, domain events, ROP chaining
    /// </summary>
    private static async Task Example1_BasicAccountOperationsWithEvents()
    {
        Console.WriteLine("Example 1: Basic Account Operations with Domain Events");
        Console.WriteLine("-------------------------------------------------------");

        var customerId = CustomerId.NewUnique();

        var result = BankAccount.TryCreate(
                customerId,
                AccountType.Checking,
                Money.TryCreate(1000m).Value,
                Money.TryCreate(500m).Value,
                Money.TryCreate(100m).Value)
            .Tap(account =>
            {
                // Show that account creation raised an event
                Console.WriteLine($"Account created with {account.UncommittedEvents().Count} uncommitted event(s)");
            })
            .Bind(account => account.Deposit(Money.TryCreate(500m).Value, "Salary deposit"))
            .Bind(account => account.Withdraw(Money.TryCreate(200m).Value, "Grocery shopping"))
            .Tap(account =>
            {
                // Show accumulated events before accepting changes
                Console.WriteLine($"After operations: {account.UncommittedEvents().Count} uncommitted event(s)");
                Console.WriteLine($"IsChanged: {account.IsChanged}");

                // Simulate repository save - accept changes
                account.AcceptChanges();
                Console.WriteLine($"After AcceptChanges: {account.UncommittedEvents().Count} uncommitted event(s)");
                Console.WriteLine($"IsChanged: {account.IsChanged}");
            })
            .Match(
                onSuccess: ok => $"✅ Account balance: {ok.Balance}. Transactions: {ok.Transactions.Count}",
                onFailure: err => $"❌ Operation failed: {err.Detail}"
            );

        Console.WriteLine(result);
        Console.WriteLine();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Example 2: Transfer money between accounts with domain event publishing.
    /// Demonstrates: Workflow orchestration, event publishing pattern
    /// </summary>
    private static async Task Example2_TransferBetweenAccountsWithEvents()
    {
        Console.WriteLine("Example 2: Transfer Between Accounts with Event Publishing");
        Console.WriteLine("-----------------------------------------------------------");

        var customer1 = CustomerId.NewUnique();
        var customer2 = CustomerId.NewUnique();

        var account1 = BankAccount.TryCreate(
            customer1,
            AccountType.Checking,
            Money.TryCreate(1000m).Value,
            Money.TryCreate(500m).Value,
            Money.TryCreate(0m).Value
        ).Value;

        var account2 = BankAccount.TryCreate(
            customer2,
            AccountType.Savings,
            Money.TryCreate(500m).Value,
            Money.TryCreate(500m).Value,
            Money.TryCreate(0m).Value
        ).Value;

        // Clear initial creation events for cleaner demo output
        account1.AcceptChanges();
        account2.AcceptChanges();

        var fraudDetection = new FraudDetectionService();
        var workflow = new BankingWorkflow(fraudDetection);

        var result = await workflow.ProcessTransferAsync(
            account1,
            account2,
            Money.TryCreate(300m).Value,
            "Rent payment"
        );

        var message = result.Match(
            onSuccess: ok => $"✅ Transfer successful!\n   From account balance: {ok.From.Balance}\n   To account balance: {ok.To.Balance}",
            onFailure: err => $"❌ Transfer failed: {err.Detail}"
        );

        Console.WriteLine(message);
        Console.WriteLine();
    }

    /// <summary>
    /// Example 3: Fraud detection with various error types.
    /// Demonstrates: Error.Domain with custom codes, CompensateAsync
    /// </summary>
    private static async Task Example3_FraudDetection()
    {
        Console.WriteLine("Example 3: Fraud Detection with Error Types");
        Console.WriteLine("--------------------------------------------");

        var customerId = CustomerId.NewUnique();
        var account = BankAccount.TryCreate(
            customerId,
            AccountType.Checking,
            Money.TryCreate(10000m).Value,
            Money.TryCreate(10000m).Value,
            Money.TryCreate(0m).Value
        ).Value;

        account.AcceptChanges(); // Clear creation event

        var fraudDetection = new FraudDetectionService();
        var workflow = new BankingWorkflow(fraudDetection);

        // Try to withdraw a suspicious amount (>$5000)
        var result = await workflow.ProcessSecureWithdrawalAsync(
            account,
            Money.TryCreate(6000m).Value,
            "123456" // MFA code
        );

        var message = result.Match(
            onSuccess: ok => $"✅ Withdrawal successful. New balance: {ok.Balance}",
            onFailure: err => $"⚠️ Expected fraud detection:\n   Code: {err.Code}\n   Detail: {err.Detail}"
        );

        Console.WriteLine(message);
        Console.WriteLine();
    }

    /// <summary>
    /// Example 4: Daily withdrawal limit enforcement.
    /// Demonstrates: Error.Domain for business rule violations
    /// </summary>
    private static async Task Example4_DailyWithdrawalLimit()
    {
        Console.WriteLine("Example 4: Daily Withdrawal Limit (Domain Errors)");
        Console.WriteLine("--------------------------------------------------");

        var customerId = CustomerId.NewUnique();
        var dailyLimit = Money.TryCreate(500m).Value;

        var account = BankAccount.TryCreate(
            customerId,
            AccountType.Checking,
            Money.TryCreate(2000m).Value,
            dailyLimit,
            Money.TryCreate(0m).Value
        ).Value;

        account.AcceptChanges(); // Clear creation event

        // Make multiple withdrawals
        var result1 = account.Withdraw(Money.TryCreate(200m).Value, "ATM withdrawal");
        Console.WriteLine(result1.IsSuccess ? "✅ First withdrawal: $200" : $"❌ {result1.Error.Detail}");

        var result2 = account.Withdraw(Money.TryCreate(200m).Value, "ATM withdrawal");
        Console.WriteLine(result2.IsSuccess ? "✅ Second withdrawal: $200" : $"❌ {result2.Error.Detail}");

        // This should exceed daily limit - demonstrates Error.Domain
        var result3 = account.Withdraw(Money.TryCreate(200m).Value, "ATM withdrawal");
        if (result3.IsFailure)
        {
            Console.WriteLine($"⚠️ Third withdrawal blocked:");
            Console.WriteLine($"   Error Type: {result3.Error.GetType().Name}");
            Console.WriteLine($"   Code: {result3.Error.Code}");
            Console.WriteLine($"   Detail: {result3.Error.Detail}");
        }

        Console.WriteLine($"Final balance: {account.Balance}");
        Console.WriteLine($"Uncommitted events: {account.UncommittedEvents().Count}");
        Console.WriteLine();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Example 5: Interest payment with domain events.
    /// Demonstrates: Domain-specific events, workflow with event publishing
    /// </summary>
    private static async Task Example5_InterestPayment()
    {
        Console.WriteLine("Example 5: Interest Payment with Domain Events");
        Console.WriteLine("-----------------------------------------------");

        var customerId = CustomerId.NewUnique();
        var account = BankAccount.TryCreate(
            customerId,
            AccountType.Savings,
            Money.TryCreate(10000m).Value,
            Money.TryCreate(100m).Value,
            Money.TryCreate(0m).Value
        ).Value;

        account.AcceptChanges(); // Clear creation event

        var fraudDetection = new FraudDetectionService();
        var workflow = new BankingWorkflow(fraudDetection);

        var annualRate = 0.025m; // 2.5% APR

        var result = await workflow.ProcessInterestPaymentAsync(account, annualRate);

        var message = result.Match(
            onSuccess: ok =>
            {
                var interestTransaction = ok.Transactions.Last();
                return $"✅ Interest payment processed\n" +
                       $"   Amount: {interestTransaction.Amount}\n" +
                       $"   New balance: {ok.Balance}\n" +
                       $"   Annual rate: {annualRate:P2}";
            },
            onFailure: err => $"❌ Interest payment failed: {err.Detail}"
        );

        Console.WriteLine(message);
        Console.WriteLine();
    }

    /// <summary>
    /// Example 6: Deep dive into domain events and change tracking.
    /// Demonstrates: UncommittedEvents, AcceptChanges, IsChanged, event inspection
    /// </summary>
    private static async Task Example6_DomainEventsAndChangeTracking()
    {
        Console.WriteLine("Example 6: Domain Events and Change Tracking");
        Console.WriteLine("---------------------------------------------");

        var customerId = CustomerId.NewUnique();

        // Create account - this raises AccountOpenedEvent
        var accountResult = BankAccount.TryCreate(
            customerId,
            AccountType.Checking,
            Money.TryCreate(1000m).Value,
            Money.TryCreate(1000m).Value,
            Money.TryCreate(100m).Value
        );

        if (accountResult.IsFailure)
        {
            Console.WriteLine($"❌ Account creation failed: {accountResult.Error.Detail}");
            return;
        }

        var account = accountResult.Value;

        Console.WriteLine("After account creation:");
        Console.WriteLine($"  IsChanged: {account.IsChanged}");
        Console.WriteLine($"  Uncommitted events: {account.UncommittedEvents().Count}");
        PrintEvents(account);

        // Perform operations
        account.Deposit(Money.TryCreate(500m).Value, "Deposit 1");
        account.Deposit(Money.TryCreate(300m).Value, "Deposit 2");
        account.Withdraw(Money.TryCreate(100m).Value, "Withdrawal 1");

        Console.WriteLine("\nAfter multiple operations:");
        Console.WriteLine($"  IsChanged: {account.IsChanged}");
        Console.WriteLine($"  Uncommitted events: {account.UncommittedEvents().Count}");
        PrintEvents(account);

        // Simulate saving to repository - accept changes
        Console.WriteLine("\nSimulating repository save (AcceptChanges)...");
        account.AcceptChanges();

        Console.WriteLine("\nAfter AcceptChanges:");
        Console.WriteLine($"  IsChanged: {account.IsChanged}");
        Console.WriteLine($"  Uncommitted events: {account.UncommittedEvents().Count}");

        // New operation after save
        account.Deposit(Money.TryCreate(50m).Value, "New deposit after save");

        Console.WriteLine("\nAfter new operation:");
        Console.WriteLine($"  IsChanged: {account.IsChanged}");
        Console.WriteLine($"  Uncommitted events: {account.UncommittedEvents().Count}");
        PrintEvents(account);

        Console.WriteLine();
        await Task.CompletedTask;
    }

    private static void PrintEvents(BankAccount account)
    {
        var events = account.UncommittedEvents();
        foreach (var evt in events)
        {
            Console.WriteLine($"    - {evt.GetType().Name} at {evt.OccurredAt:HH:mm:ss.fff}");
        }
    }
}
