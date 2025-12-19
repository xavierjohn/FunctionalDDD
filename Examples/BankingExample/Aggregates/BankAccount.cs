namespace BankingExample.Aggregates;

using BankingExample.Entities;
using BankingExample.ValueObjects;
using FunctionalDdd;

public enum AccountStatus
{
    Active,
    Frozen,
    Closed
}

public enum AccountType
{
    Checking,
    Savings,
    MoneyMarket
}

/// <summary>
/// Bank account aggregate demonstrating transaction processing with validation.
/// </summary>
public class BankAccount : Aggregate<AccountId>
{
    private readonly List<Transaction> _transactions = [];

    public CustomerId CustomerId { get; }
    public AccountType AccountType { get; }
    public Money Balance { get; private set; }
    public Money DailyWithdrawalLimit { get; }
    public Money OverdraftLimit { get; }
    public AccountStatus Status { get; private set; }
    public DateTime CreatedAt { get; }
    public IReadOnlyList<Transaction> Transactions => _transactions.AsReadOnly();

    private BankAccount(
        CustomerId customerId,
        AccountType accountType,
        Money initialBalance,
        Money dailyWithdrawalLimit,
        Money overdraftLimit) : base(AccountId.NewUnique())
    {
        CustomerId = customerId;
        AccountType = accountType;
        Balance = initialBalance;
        DailyWithdrawalLimit = dailyWithdrawalLimit;
        OverdraftLimit = overdraftLimit;
        Status = AccountStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<BankAccount> TryCreate(
        CustomerId customerId,
        AccountType accountType,
        Money initialDeposit,
        Money dailyWithdrawalLimit,
        Money overdraftLimit)
    {
        return customerId.ToResult()
            .Ensure(_ => customerId != null, Error.Validation("Customer ID is required"))
            .Ensure(_ => initialDeposit.Value >= 0, Error.Validation("Initial deposit must be non-negative"))
            .Ensure(_ => dailyWithdrawalLimit.Value > 0, Error.Validation("Daily withdrawal limit must be positive"))
            .Ensure(_ => overdraftLimit.Value >= 0, Error.Validation("Overdraft limit must be non-negative"))
            .Map(_ => new BankAccount(customerId, accountType, initialDeposit, dailyWithdrawalLimit, overdraftLimit));
    }

    /// <summary>
    /// Deposits money into the account with validation.
    /// </summary>
    public Result<BankAccount> Deposit(Money amount, string description = "Deposit")
    {
        return this.ToResult()
            .Ensure(_ => Status == AccountStatus.Active, Error.Validation($"Cannot deposit to {Status} account"))
            .Ensure(_ => amount.Value > 0, Error.Validation("Deposit amount must be positive"))
            .Ensure(_ => amount.Value <= 10000, Error.Validation("Single deposit cannot exceed $10,000"))
            .Bind(_ => Balance.Add(amount))
            .Tap(newBalance =>
            {
                Balance = newBalance;
                _transactions.Add(Transaction.CreateDeposit(TransactionId.NewUnique(), amount, Balance, description));
            })
            .Map(_ => this);
    }

    /// <summary>
    /// Withdraws money with daily limit and overdraft protection.
    /// </summary>
    public Result<BankAccount> Withdraw(Money amount, string description = "Withdrawal")
    {
        var todayTotal = GetTodayWithdrawals();

        return this.ToResult()
            .Ensure(_ => Status == AccountStatus.Active, Error.Validation($"Cannot withdraw from {Status} account"))
            .Ensure(_ => amount.Value > 0, Error.Validation("Withdrawal amount must be positive"))
            .Bind(_ => todayTotal.Add(amount))
            .Ensure(totalWithToday => totalWithToday.IsGreaterThanOrEqual(DailyWithdrawalLimit) == false,
                Error.Validation($"Daily withdrawal limit of {DailyWithdrawalLimit} would be exceeded"))
            .Bind(_ => Balance.Subtract(amount))
            .Ensure(newBalance => newBalance.Value >= -OverdraftLimit.Value,
                Error.Validation($"Withdrawal would exceed overdraft limit of {OverdraftLimit}"))
            .Tap(newBalance =>
            {
                Balance = newBalance;
                _transactions.Add(Transaction.CreateWithdrawal(TransactionId.NewUnique(), amount, Balance, description));
            })
            .Map(_ => this);
    }

    /// <summary>
    /// Transfers money to another account with validation.
    /// </summary>
    public Result<(BankAccount From, BankAccount To)> TransferTo(BankAccount toAccount, Money amount, string description = "Transfer")
    {
        if (toAccount == null)
            return Error.Validation("Destination account is required");

        if (Id.Equals(toAccount.Id))
            return Error.Validation("Cannot transfer to the same account");

        return Withdraw(amount, $"{description} to {toAccount.Id}")
            .Bind(_ => toAccount.Deposit(amount, $"{description} from {Id}"))
            .Map(_ => (this, toAccount));
    }

    /// <summary>
    /// Freezes the account to prevent transactions.
    /// </summary>
    public Result<BankAccount> Freeze(string reason)
    {
        return this.ToResult()
            .Ensure(_ => Status == AccountStatus.Active, Error.Validation($"Cannot freeze {Status} account"))
            .Ensure(_ => !string.IsNullOrWhiteSpace(reason), Error.Validation("Freeze reason is required"))
            .Tap(_ =>
            {
                Status = AccountStatus.Frozen;
                Console.WriteLine($"Account {Id} frozen: {reason}");
            })
            .Map(_ => this);
    }

    /// <summary>
    /// Unfreezes a previously frozen account.
    /// </summary>
    public Result<BankAccount> Unfreeze()
    {
        return this.ToResult()
            .Ensure(_ => Status == AccountStatus.Frozen, Error.Validation("Only frozen accounts can be unfrozen"))
            .Tap(_ =>
            {
                Status = AccountStatus.Active;
                Console.WriteLine($"Account {Id} unfrozen");
            })
            .Map(_ => this);
    }

    /// <summary>
    /// Closes the account if balance is zero.
    /// </summary>
    public Result<BankAccount> Close()
    {
        return this.ToResult()
            .Ensure(_ => Status != AccountStatus.Closed, Error.Validation("Account is already closed"))
            .Ensure(_ => Balance.Value == 0, Error.Validation("Account balance must be zero to close"))
            .Tap(_ =>
            {
                Status = AccountStatus.Closed;
                Console.WriteLine($"Account {Id} closed");
            })
            .Map(_ => this);
    }

    private Money GetTodayWithdrawals()
    {
        var today = DateTime.UtcNow.Date;
        var todayWithdrawals = _transactions
            .Where(t => t.Type == TransactionType.Withdrawal && t.Timestamp.Date == today)
            .Sum(t => t.Amount.Value);

        return Money.TryCreate(todayWithdrawals).Value;
    }

    public Money GetAvailableBalance()
    {
        return Money.TryCreate(Balance.Value + OverdraftLimit.Value).Value;
    }
}
