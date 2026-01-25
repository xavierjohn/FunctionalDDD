namespace BankingExample.Events;

using BankingExample.Aggregates;
using BankingExample.ValueObjects;
using FunctionalDdd;
using FunctionalDdd.PrimitiveValueObjects;

/// <summary>
/// Raised when a new bank account is opened.
/// </summary>
public record AccountOpened(
    AccountId AccountId,
    CustomerId CustomerId,
    AccountType AccountType,
    Money InitialBalance,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when money is deposited into an account.
/// </summary>
public record MoneyDeposited(
    AccountId AccountId,
    Money Amount,
    Money NewBalance,
    string Description,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when money is withdrawn from an account.
/// </summary>
public record MoneyWithdrawn(
    AccountId AccountId,
    Money Amount,
    Money NewBalance,
    string Description,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when a transfer is completed between accounts.
/// </summary>
public record TransferCompleted(
    AccountId FromAccountId,
    AccountId ToAccountId,
    Money Amount,
    string Description,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when an account is frozen due to suspicious activity.
/// </summary>
public record AccountFrozen(
    AccountId AccountId,
    string Reason,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when an account is unfrozen.
/// </summary>
public record AccountUnfrozen(
    AccountId AccountId,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when an account is closed.
/// </summary>
public record AccountClosed(
    AccountId AccountId,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when interest is paid to a savings account.
/// </summary>
public record InterestPaid(
    AccountId AccountId,
    Money InterestAmount,
    Money NewBalance,
    decimal AnnualRate,
    DateTime OccurredAt) : IDomainEvent;
