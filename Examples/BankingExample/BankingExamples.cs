using BankingExample.Aggregates;
using BankingExample.Services;
using BankingExample.ValueObjects;
using BankingExample.Workflows;
using FunctionalDdd;

namespace BankingExample;

/// <summary>
/// Demonstrates banking operations with Railway Oriented Programming.
/// 
/// Features:
/// - Account management with validation
/// - Fraud detection and security
/// - Transfer operations with rollback
/// - Daily withdrawal limits and overdraft protection
/// - Interest calculations
/// </summary>
public class BankingExamples
{
    public static async Task RunExamplesAsync()
    {
        Console.WriteLine("=== Banking Transaction Examples ===\n");

        await Example1_BasicAccountOperations();
        await Example2_TransferBetweenAccounts();
        await Example3_FraudDetection();
        await Example4_DailyWithdrawalLimit();
        await Example5_InterestPayment();
    }

    /// <summary>
    /// Example 1: Basic account operations - deposit and withdrawal.
    /// </summary>
    private static async Task Example1_BasicAccountOperations()
    {
        Console.WriteLine("Example 1: Basic Account Operations");
        Console.WriteLine("------------------------------------");

        var customerId = CustomerId.NewUnique();

        var result = BankAccount.TryCreate(
                customerId,
                AccountType.Checking,
                Money.TryCreate(1000m).Value,
                Money.TryCreate(500m).Value,
                Money.TryCreate(100m).Value)
            .Bind(account => account.Deposit(Money.TryCreate(500m).Value, "Salary deposit"))
            .Bind(account => account.Withdraw(Money.TryCreate(200m).Value, "Grocery shopping"))
            .Match(
                onSuccess: ok => $"? Account balance: {ok.Balance}. Transactions: {ok.Transactions.Count}",
                onFailure: err => $"? Operation failed: {err.Detail}"
            );

        Console.WriteLine(result);
        Console.WriteLine();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Example 2: Transfer money between accounts with validation.
    /// </summary>
    private static async Task Example2_TransferBetweenAccounts()
    {
        Console.WriteLine("Example 2: Transfer Between Accounts");
        Console.WriteLine("-------------------------------------");

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

        var fraudDetection = new FraudDetectionService();
        var workflow = new BankingWorkflow(fraudDetection);

        var result = await workflow.ProcessTransferAsync(
            account1,
            account2,
            Money.TryCreate(300m).Value,
            "Rent payment"
        );

        var message = result.Match(
            onSuccess: ok => $"? Transfer successful!\n   From account balance: {ok.From.Balance}\n   To account balance: {ok.To.Balance}",
            onFailure: err => $"? Transfer failed: {err.Detail}"
        );

        Console.WriteLine(message);
        Console.WriteLine();
    }

    /// <summary>
    /// Example 3: Fraud detection preventing suspicious transactions.
    /// </summary>
    private static async Task Example3_FraudDetection()
    {
        Console.WriteLine("Example 3: Fraud Detection");
        Console.WriteLine("--------------------------");

        var customerId = CustomerId.NewUnique();
        var account = BankAccount.TryCreate(
            customerId,
            AccountType.Checking,
            Money.TryCreate(10000m).Value,
            Money.TryCreate(10000m).Value,
            Money.TryCreate(0m).Value
        ).Value;

        var fraudDetection = new FraudDetectionService();
        var workflow = new BankingWorkflow(fraudDetection);

        // Try to withdraw a suspicious amount (>$5000)
        var result = await workflow.ProcessSecureWithdrawalAsync(
            account,
            Money.TryCreate(6000m).Value,
            "123456" // MFA code
        );

        var message = result.Match(
            onSuccess: ok => $"? Withdrawal successful. New balance: {ok.Balance}",
            onFailure: err => $"? Expected fraud detection: {err.Detail}"
        );

        Console.WriteLine(message);
        Console.WriteLine();
    }

    /// <summary>
    /// Example 4: Daily withdrawal limit enforcement.
    /// </summary>
    private static async Task Example4_DailyWithdrawalLimit()
    {
        Console.WriteLine("Example 4: Daily Withdrawal Limit");
        Console.WriteLine("----------------------------------");

        var customerId = CustomerId.NewUnique();
        var dailyLimit = Money.TryCreate(500m).Value;

        var account = BankAccount.TryCreate(
            customerId,
            AccountType.Checking,
            Money.TryCreate(2000m).Value,
            dailyLimit,
            Money.TryCreate(0m).Value
        ).Value;

        // Make multiple withdrawals
        var result1 = account.Withdraw(Money.TryCreate(200m).Value, "ATM withdrawal");
        Console.WriteLine(result1.IsSuccess ? "? First withdrawal: $200" : result1.Error.Detail);

        var result2 = account.Withdraw(Money.TryCreate(200m).Value, "ATM withdrawal");
        Console.WriteLine(result2.IsSuccess ? "? Second withdrawal: $200" : result2.Error.Detail);

        // This should exceed daily limit
        var result3 = account.Withdraw(Money.TryCreate(200m).Value, "ATM withdrawal");
        Console.WriteLine(result3.IsSuccess ? "? Third withdrawal: $200" : $"? {result3.Error.Detail}");

        Console.WriteLine($"Final balance: {account.Balance}");
        Console.WriteLine($"Today's withdrawals: {account.Transactions.Where(t => t.Type == Entities.TransactionType.Withdrawal && t.Timestamp.Date == DateTime.UtcNow.Date).Sum(t => t.Amount.Value):C}");
        Console.WriteLine();
    }

    /// <summary>
    /// Example 5: Interest payment calculation for savings account.
    /// </summary>
    private static async Task Example5_InterestPayment()
    {
        Console.WriteLine("Example 5: Interest Payment");
        Console.WriteLine("---------------------------");

        var customerId = CustomerId.NewUnique();
        var account = BankAccount.TryCreate(
            customerId,
            AccountType.Savings,
            Money.TryCreate(10000m).Value,
            Money.TryCreate(100m).Value,
            Money.TryCreate(0m).Value
        ).Value;

        var fraudDetection = new FraudDetectionService();
        var workflow = new BankingWorkflow(fraudDetection);

        var annualRate = 0.025m; // 2.5% APR

        var result = await workflow.ProcessInterestPaymentAsync(account, annualRate);

        var message = result.Match(
            onSuccess: ok =>
            {
                var interestTransaction = ok.Transactions.Last();
                return $"? Interest payment processed\n" +
                       $"   Amount: {interestTransaction.Amount}\n" +
                       $"   New balance: {ok.Balance}\n" +
                       $"   Annual rate: {annualRate:P2}";
            },
            onFailure: err => $"? Interest payment failed: {err.Detail}"
        );

        Console.WriteLine(message);
        Console.WriteLine();
    }
}
