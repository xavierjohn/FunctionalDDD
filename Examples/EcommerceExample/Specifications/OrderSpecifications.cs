using System.Linq.Expressions;
using EcommerceExample.Aggregates;
using EcommerceExample.ValueObjects;
using Trellis;

namespace EcommerceExample.Specifications;

/// <summary>
/// Matches orders with a specific status.
/// </summary>
public class OrderStatusSpec(OrderStatus status) : Specification<Order>
{
    public override Expression<Func<Order, bool>> ToExpression() =>
        order => order.Status == status;
}

/// <summary>
/// Matches orders with a total above the given threshold.
/// "High value" is domain-defined — compose with other specs to build business rules.
/// </summary>
public class HighValueOrderSpec(decimal threshold) : Specification<Order>
{
    public override Expression<Func<Order, bool>> ToExpression() =>
        order => order.Total.Amount > threshold;
}

/// <summary>
/// Matches orders belonging to a specific customer.
/// </summary>
public class CustomerOrderSpec(CustomerId customerId) : Specification<Order>
{
    public override Expression<Func<Order, bool>> ToExpression() =>
        order => order.CustomerId == customerId;
}

/// <summary>
/// Matches orders that are eligible for cancellation.
/// Business rule: only Draft, Pending, or PaymentFailed orders can be cancelled.
/// </summary>
public class CancellableOrderSpec : Specification<Order>
{
    public override Expression<Func<Order, bool>> ToExpression() =>
        order => order.Status == OrderStatus.Draft
              || order.Status == OrderStatus.Pending
              || order.Status == OrderStatus.PaymentFailed;
}
