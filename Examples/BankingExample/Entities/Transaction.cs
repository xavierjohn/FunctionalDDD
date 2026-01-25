namespace BankingExample.Entities;

using BankingExample.ValueObjects;
using FunctionalDdd;
using FunctionalDdd.PrimitiveValueObjects;

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Transfer,
    Fee,
    Interest
}

/// <summary>
/// Represents a single transaction on an account.
/// </summary>
public class Transaction : Entity<TransactionId>
{
    public TransactionType Type { get; }
    public Money Amount { get; }
    public Money BalanceAfter { get; }
    public string Description { get; }
    public DateTime Timestamp { get; }

    private Transaction(
        TransactionId id,
        TransactionType type,
        Money amount,
        Money balanceAfter,
        string description) : base(id)
    {
        Type = type;
        Amount = amount;
        BalanceAfter = balanceAfter;
        Description = description;
        Timestamp = DateTime.UtcNow;
    }

    public static Transaction CreateDeposit(TransactionId id, Money amount, Money balanceAfter, string description)
        => new(id, TransactionType.Deposit, amount, balanceAfter, description);

    public static Transaction CreateWithdrawal(TransactionId id, Money amount, Money balanceAfter, string description)
        => new(id, TransactionType.Withdrawal, amount, balanceAfter, description);

    public static Transaction CreateTransfer(TransactionId id, Money amount, Money balanceAfter, string description)
        => new(id, TransactionType.Transfer, amount, balanceAfter, description);

    public static Transaction CreateFee(TransactionId id, Money amount, Money balanceAfter, string description)
        => new(id, TransactionType.Fee, amount, balanceAfter, description);

    public override string ToString()
        => $"{Timestamp:yyyy-MM-dd HH:mm:ss} | {Type,-12} | {Amount,10} | Balance: {BalanceAfter} | {Description}";
}
