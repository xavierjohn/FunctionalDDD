namespace FunctionalDdd.Specifications.Tests;

/// <summary>
/// Example specification for testing - filters active orders.
/// </summary>
public sealed class ActiveOrdersSpec : Specification<Order>
{
    public ActiveOrdersSpec()
        : base(o => o.Status == OrderStatus.Active)
    {
        AddInclude(o => o.Lines);
        AddOrderByDescending(o => o.CreatedAt);
        ApplyAsNoTracking();
    }
}

/// <summary>
/// Example specification for testing - filters orders by customer.
/// </summary>
public sealed class OrdersByCustomerSpec : Specification<Order>
{
    public OrdersByCustomerSpec(string customerName)
        : base(o => o.CustomerName == customerName) =>
        AddInclude(o => o.Lines);
}

/// <summary>
/// Example specification for testing - filters high value orders.
/// </summary>
public sealed class HighValueOrdersSpec : Specification<Order>
{
    public HighValueOrdersSpec(decimal minTotal = 500m)
        : base(o => o.Total >= minTotal) =>
        AddOrderByDescending(o => o.Total);
}

/// <summary>
/// Example specification for testing - complex business rule.
/// </summary>
public sealed class OrdersReadyForShippingSpec : Specification<Order>
{
    public OrdersReadyForShippingSpec()
        : base(o => o.Status == OrderStatus.Confirmed)
    {
        AddInclude(o => o.Lines);
        AddOrderBy(o => o.CreatedAt);
        ApplyAsSplitQuery();
    }
}
