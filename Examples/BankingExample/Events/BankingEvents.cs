namespace BankingExample.Events;

using BankingExample.Aggregates;
using BankingExample.ValueObjects;
using FunctionalDdd;

/// <summary>
/// Raised when a new bank account is opened.
/// </summary>
public record AccountOpenedEvent(
    AccountId AccountId,
    CustomerId CustomerId,
    AccountType AccountType,
    Money InitialBalance,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when money is deposited into an account.
/// </summary>
public record MoneyDepositedEvent(
    AccountId AccountId,
    Money Amount,
    Money NewBalance,
    string Description,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when money is withdrawn from an account.
/// </summary>
public record MoneyWithdrawnEvent(
    AccountId AccountId,
    Money Amount,
    Money NewBalance,
    string Description,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when a transfer is completed between accounts.
/// </summary>
public record TransferCompletedEvent(
    AccountId FromAccountId,
    AccountId ToAccountId,
    Money Amount,
    string Description,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when an account is frozen due to suspicious activity.
/// </summary>
public record AccountFrozenEvent(
    AccountId AccountId,
    string Reason,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when an account is unfrozen.
/// </summary>
public record AccountUnfrozenEvent(
    AccountId AccountId,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when an account is closed.
/// </summary>
public record AccountClosedEvent(
    AccountId AccountId,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when interest is paid to a savings account.
/// </summary>
public record InterestPaidEvent(
    AccountId AccountId,
    Money InterestAmount,
    Money NewBalance,
    decimal AnnualRate,
    DateTime OccurredAt) : IDomainEvent;
