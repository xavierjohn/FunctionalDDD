namespace EcommerceExample.Events;

using EcommerceExample.Aggregates;
using EcommerceExample.ValueObjects;
using FunctionalDdd;
using FunctionalDdd.PrimitiveValueObjects;

/// <summary>
/// Raised when a new order is created.
/// </summary>
public record OrderCreated(
    OrderId OrderId,
    CustomerId CustomerId,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when a product line is added to an order.
/// </summary>
public record OrderLineAdded(
    OrderId OrderId,
    ProductId ProductId,
    string ProductName,
    Money UnitPrice,
    int Quantity,
    Money LineTotal,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when a product line is removed from an order.
/// </summary>
public record OrderLineRemoved(
    OrderId OrderId,
    ProductId ProductId,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when an order is submitted for processing.
/// </summary>
public record OrderSubmitted(
    OrderId OrderId,
    CustomerId CustomerId,
    Money Total,
    int LineCount,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when payment processing starts.
/// </summary>
public record PaymentProcessingStarted(
    OrderId OrderId,
    string TransactionId,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when payment fails.
/// </summary>
public record PaymentFailed(
    OrderId OrderId,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when an order is confirmed after successful payment.
/// </summary>
public record OrderConfirmed(
    OrderId OrderId,
    CustomerId CustomerId,
    Money Total,
    string PaymentTransactionId,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when an order is cancelled.
/// </summary>
public record OrderCancelled(
    OrderId OrderId,
    string Reason,
    OrderStatus PreviousStatus,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when an order is shipped.
/// </summary>
public record OrderShipped(
    OrderId OrderId,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Raised when an order is delivered.
/// </summary>
public record OrderDelivered(
    OrderId OrderId,
    DateTime OccurredAt) : IDomainEvent;
