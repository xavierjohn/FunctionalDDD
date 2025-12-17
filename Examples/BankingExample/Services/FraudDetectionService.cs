namespace BankingExample.Services;

using BankingExample.Aggregates;
using BankingExample.ValueObjects;
using FunctionalDdd;

/// <summary>
/// Detects fraudulent transactions based on patterns.
/// </summary>
public class FraudDetectionService
{
    private const decimal SuspiciousAmountThreshold = 5000m;
    private const int MaxTransactionsPerHour = 10;

    /// <summary>
    /// Analyzes a transaction for fraud indicators.
    /// Returns Success if transaction appears legitimate, Failure if suspicious.
    /// </summary>
    public async Task<Result<Unit>> AnalyzeTransactionAsync(
        BankAccount account,
        Money amount,
        string transactionType,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken); // Simulate API call

        return CheckSuspiciousAmount(amount)
            .Ensure(_ => !IsHighFrequencyTrading(account), 
                Error.Validation("Suspicious activity: Too many transactions in short period", "fraud"))
            .Ensure(_ => !IsUnusualPattern(account, amount), 
                Error.Validation("Suspicious activity: Unusual transaction pattern", "fraud"))
            .Tap(_ => Console.WriteLine($"? Fraud check passed for {transactionType} of {amount}"));
    }

    /// <summary>
    /// Checks if amount exceeds suspicious threshold.
    /// </summary>
    private static Result<Unit> CheckSuspiciousAmount(Money amount)
    {
        if (amount.Value > SuspiciousAmountThreshold)
        {
            Console.WriteLine($"? Large transaction detected: {amount}");
            return Error.Validation(
                $"Transaction amount {amount} exceeds threshold of ${SuspiciousAmountThreshold}. Manual review required.",
                "fraud"
            );
        }

        return Result.Success();
    }

    /// <summary>
    /// Checks if account has too many recent transactions.
    /// </summary>
    private static bool IsHighFrequencyTrading(BankAccount account)
    {
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentCount = account.Transactions.Count(t => t.Timestamp >= oneHourAgo);

        return recentCount >= MaxTransactionsPerHour;
    }

    /// <summary>
    /// Detects unusual patterns (e.g., multiple round-number withdrawals).
    /// </summary>
    private static bool IsUnusualPattern(BankAccount account, Money amount)
    {
        // Check for multiple round-number transactions
        var recentTransactions = account.Transactions
            .Where(t => t.Timestamp >= DateTime.UtcNow.AddHours(-24))
            .ToList();

        var roundNumberCount = recentTransactions
            .Count(t => t.Amount.Value % 1000 == 0);

        return roundNumberCount >= 3 && amount.Value % 1000 == 0;
    }

    /// <summary>
    /// Verifies customer identity for high-value transactions.
    /// </summary>
    public async Task<Result<Unit>> VerifyCustomerIdentityAsync(
        CustomerId customerId,
        string verificationCode,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(200, cancellationToken); // Simulate MFA verification

        if (string.IsNullOrWhiteSpace(verificationCode))
            return Error.Unauthorized("Verification code required for this transaction");

        if (verificationCode.Length != 6 || !verificationCode.All(char.IsDigit))
            return Error.Unauthorized("Invalid verification code format");

        // Simulate verification check
        if (verificationCode == "000000")
            return Error.Unauthorized("Invalid verification code");

        Console.WriteLine($"? Customer {customerId} identity verified");
        return Result.Success();
    }
}
