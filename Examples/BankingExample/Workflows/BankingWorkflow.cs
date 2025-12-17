namespace BankingExample.Workflows;

using BankingExample.Aggregates;
using BankingExample.Services;
using BankingExample.ValueObjects;
using FunctionalDdd;

/// <summary>
/// Orchestrates banking operations with fraud detection and validation.
/// </summary>
public class BankingWorkflow
{
    private readonly FraudDetectionService _fraudDetection;

    public BankingWorkflow(FraudDetectionService fraudDetection)
    {
        _fraudDetection = fraudDetection;
    }

    /// <summary>
    /// Processes a secure withdrawal with fraud detection.
    /// Demonstrates: Ensure, Bind, EnsureAsync, TapAsync, Compensate
    /// </summary>
    public async Task<Result<BankAccount>> ProcessSecureWithdrawalAsync(
        BankAccount account,
        Money amount,
        string verificationCode,
        CancellationToken cancellationToken = default)
    {
        async Task<Result<BankAccount>> PerformChecks(BankAccount acc)
        {
            // Fraud detection
            var fraudResult = await _fraudDetection.AnalyzeTransactionAsync(acc, amount, "withdrawal", cancellationToken);
            if (fraudResult.IsFailure)
                return fraudResult.Error;

            // Require MFA for large withdrawals
            if (amount.Value > 1000)
            {
                var mfaResult = await _fraudDetection.VerifyCustomerIdentityAsync(
                    acc.CustomerId,
                    verificationCode,
                    cancellationToken
                );

                if (mfaResult.IsFailure)
                    return mfaResult.Error;
            }

            return acc.ToResult();
        }

        return await account.ToResult()
            .BindAsync((Func<BankAccount, Task<Result<BankAccount>>>)PerformChecks)
            .BindAsync(acc => Task.FromResult(acc.Withdraw(amount, "ATM Withdrawal")))
            .TapAsync(acc => LogTransactionAsync(acc.Id, "withdrawal", amount, cancellationToken))
            .CompensateAsync(
                predicate: error => error.Code == "fraud",
                funcAsync: async error =>
                {
                    await Task.FromResult(account.Freeze("Suspicious activity detected"));
                    await NotifySecurityTeamAsync(account.CustomerId, error, cancellationToken);
                    return error; // Still return error after compensation
                }
            );
    }

    /// <summary>
    /// Processes a transfer between accounts with full validation.
    /// Demonstrates: Complex workflow with multiple validations and parallel operations.
    /// </summary>
    public async Task<Result<(BankAccount From, BankAccount To)>> ProcessTransferAsync(
        BankAccount fromAccount,
        BankAccount toAccount,
        Money amount,
        string description,
        CancellationToken cancellationToken = default)
    {
        // Validate both accounts in parallel
        var fromValidation = _fraudDetection.AnalyzeTransactionAsync(fromAccount, amount, "transfer-out", cancellationToken);
        var toValidation = _fraudDetection.AnalyzeTransactionAsync(toAccount, amount, "transfer-in", cancellationToken);

        var validationResults = await Task.WhenAll(fromValidation, toValidation);

        if (validationResults[0].IsFailure)
            return validationResults[0].Error;

        if (validationResults[1].IsFailure)
            return validationResults[1].Error;

        // Perform transfer
        return await Task.FromResult(fromAccount.TransferTo(toAccount, amount, description))
            .TapAsync(accounts => LogTransactionAsync(
                fromAccount.Id,
                "transfer",
                amount,
                cancellationToken
            ))
            .TapAsync(accounts => NotifyTransferCompleteAsync(
                fromAccount.CustomerId,
                toAccount.CustomerId,
                amount,
                cancellationToken
            ));
    }

    /// <summary>
    /// Processes daily interest payment for savings accounts.
    /// </summary>
    public async Task<Result<BankAccount>> ProcessInterestPaymentAsync(
        BankAccount account,
        decimal interestRate,
        CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(account.ToResult()
            .Ensure(acc => acc.AccountType == AccountType.Savings,
                Error.Validation("Interest is only paid on savings accounts"))
            .Ensure(acc => acc.Status == AccountStatus.Active,
                Error.Validation($"Cannot process interest for {account.Status} account"))
            .Ensure(acc => acc.Balance.Value > 0,
                Error.Validation("No interest on accounts with zero or negative balance"))
            .Bind(acc =>
            {
                var interestAmount = acc.Balance.Value * (interestRate / 365m); // Daily interest
                return Money.TryCreate(interestAmount);
            }))
            .BindAsync(async interest =>
            {
                await Task.Delay(50, cancellationToken);
                return account.Deposit(interest, $"Daily interest at {interestRate:P2} APR");
            })
            .TapAsync(acc => LogTransactionAsync(acc.Id, "interest", acc.Balance, cancellationToken));
    }

    /// <summary>
    /// Processes multiple transactions in batch with rollback on any failure.
    /// Demonstrates: Transaction-like behavior with compensation.
    /// </summary>
    public async Task<Result<BankAccount>> ProcessBatchTransactionsAsync(
        BankAccount account,
        List<(Money Amount, string Description)> transactions,
        CancellationToken cancellationToken = default)
    {
        var originalBalance = account.Balance;
        var processedCount = 0;

        foreach (var (amount, description) in transactions)
        {
            var result = account.Deposit(amount, description);

            if (result.IsFailure)
            {
                // Rollback all previous transactions
                Console.WriteLine($"? Transaction failed, rolling back {processedCount} transactions");
                return await RollbackTransactionsAsync(account, originalBalance, cancellationToken);
            }

            processedCount++;
        }

        Console.WriteLine($"? Successfully processed {processedCount} transactions");
        return account;
    }

    private static async Task<Result<BankAccount>> RollbackTransactionsAsync(
        BankAccount account,
        Money originalBalance,
        CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken);
        Console.WriteLine($"Rolling back to original balance: {originalBalance}");

        // In a real system, this would involve more complex rollback logic
        return Error.Unexpected("Batch transaction failed and was rolled back");
    }

    private static async Task LogTransactionAsync(
        AccountId accountId,
        string type,
        Money amount,
        CancellationToken cancellationToken)
    {
        await Task.Delay(20, cancellationToken);
        Console.WriteLine($"?? Logged {type} of {amount} for account {accountId}");
    }

    private static async Task NotifyTransferCompleteAsync(
        CustomerId fromCustomerId,
        CustomerId toCustomerId,
        Money amount,
        CancellationToken cancellationToken)
    {
        await Task.Delay(50, cancellationToken);
        Console.WriteLine($"?? Transfer notification sent: {amount} from {fromCustomerId} to {toCustomerId}");
    }

    private static async Task NotifySecurityTeamAsync(
        CustomerId customerId,
        Error error,
        CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken);
        Console.WriteLine($"?? Security alert for customer {customerId}: {error.Detail}");
    }
}
