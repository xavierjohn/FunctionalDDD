namespace SpecificationExample.Specifications;

using FunctionalDdd;
using SpecificationExample.Entities;
using SpecificationExample.ValueObjects;

// =============================================================================
// REUSABLE BUSINESS SPECIFICATIONS
// =============================================================================
// These specifications encapsulate common query patterns for orders.
// They can be composed together for complex queries.
// =============================================================================

/// <summary>
/// Orders with a specific status.
/// </summary>
public sealed class OrdersByStatusSpec : Specification<Order>
{
    public OrdersByStatusSpec(OrderStatus status)
        : base(o => o.Status == status) { }
}

/// <summary>
/// Orders that are confirmed and paid - ready for processing.
/// </summary>
public sealed class OrdersReadyForProcessingSpec : Specification<Order>
{
    public OrdersReadyForProcessingSpec()
        : base(o => o.Status == OrderStatus.Confirmed && o.PaymentStatus == PaymentStatus.Paid)
    {
        AddInclude(o => o.Lines);
        AddOrderBy(o => o.CreatedAt);
        ApplyAsNoTracking();
    }
}

/// <summary>
/// High-value orders above a threshold.
/// </summary>
public sealed class HighValueOrdersSpec : Specification<Order>
{
    public HighValueOrdersSpec(decimal minTotal = 500m)
        : base(o => o.Total >= minTotal) =>
        AddOrderByDescending(o => o.Total);
}

/// <summary>
/// Priority orders that need immediate attention.
/// </summary>
public sealed class PriorityOrdersSpec : Specification<Order>
{
    public PriorityOrdersSpec()
        : base(o => o.IsPriority) =>
        AddOrderBy(o => o.CreatedAt);
}

/// <summary>
/// Orders for a specific customer.
/// </summary>
public sealed class OrdersByCustomerSpec : Specification<Order>
{
    public OrdersByCustomerSpec(CustomerId customerId)
        : base(o => o.CustomerId == customerId)
    {
        AddInclude(o => o.Lines);
        AddOrderByDescending(o => o.CreatedAt);
    }
}

/// <summary>
/// Orders that need shipping (processing status, paid, not yet shipped).
/// </summary>
public sealed class OrdersReadyForShippingSpec : Specification<Order>
{
    public OrdersReadyForShippingSpec()
        : base(o => o.Status == OrderStatus.Processing
                 && o.PaymentStatus == PaymentStatus.Paid
                 && o.ShippedAt == null)
    {
        AddInclude(o => o.Lines);
        AddOrderBy(o => o.IsPriority); // Priority orders first (false < true, so we need descending)
        AddOrderBy(o => o.CreatedAt);  // Then by creation date
        ApplyAsNoTracking();
    }
}

/// <summary>
/// Recent orders within a time window.
/// </summary>
public sealed class RecentOrdersSpec : Specification<Order>
{
    public RecentOrdersSpec(DateTime since)
        : base(o => o.CreatedAt >= since) =>
        AddOrderByDescending(o => o.CreatedAt);
}

/// <summary>
/// Cancelled orders for reporting.
/// </summary>
public sealed class CancelledOrdersSpec : Specification<Order>
{
    public CancelledOrdersSpec()
        : base(o => o.Status == OrderStatus.Cancelled)
    {
        AddOrderByDescending(o => o.CreatedAt);
        ApplyAsNoTracking();
    }
}
