namespace Benchmark;

using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using Trellis;

/// <summary>
/// Benchmarks for the Specification pattern — composable business rules
/// with expression tree generation and in-memory evaluation.
/// </summary>
[MemoryDiagnoser]
public class SpecificationBenchmarks
{
    private Order _validOrder = default!;
    private Order _invalidOrder = default!;
    private HighValueOrderSpec _highValueSpec = default!;
    private RecentOrderSpec _recentSpec = default!;
    private Specification<Order> _andSpec = default!;
    private Specification<Order> _orSpec = default!;
    private Specification<Order> _notSpec = default!;
    private Specification<Order> _complexSpec = default!;
    private List<Order> _orders = default!;

    [GlobalSetup]
    public void Setup()
    {
        _validOrder = new Order(1000m, DateTimeOffset.UtcNow.AddDays(-5), "Shipped");
        _invalidOrder = new Order(50m, DateTimeOffset.UtcNow.AddDays(-60), "Pending");

        _highValueSpec = new HighValueOrderSpec(500m);
        _recentSpec = new RecentOrderSpec(DateTimeOffset.UtcNow, 30);

        _andSpec = _highValueSpec.And(_recentSpec);
        _orSpec = _highValueSpec.Or(_recentSpec);
        _notSpec = _highValueSpec.Not();
        _complexSpec = _highValueSpec.And(_recentSpec).Or(_highValueSpec.Not().And(_recentSpec));

        _orders = [];
        for (var i = 0; i < 100; i++)
            _orders.Add(new Order(i * 25m, DateTimeOffset.UtcNow.AddDays(-i), i % 3 == 0 ? "Shipped" : "Pending"));
    }

    #region Simple Evaluation

    [Benchmark(Baseline = true)]
    public bool IsSatisfiedBy_Simple_Pass()
    {
        return _highValueSpec.IsSatisfiedBy(_validOrder);
    }

    [Benchmark]
    public bool IsSatisfiedBy_Simple_Fail()
    {
        return _highValueSpec.IsSatisfiedBy(_invalidOrder);
    }

    #endregion

    #region Composed Evaluation

    [Benchmark]
    public bool IsSatisfiedBy_And_Pass()
    {
        return _andSpec.IsSatisfiedBy(_validOrder);
    }

    [Benchmark]
    public bool IsSatisfiedBy_And_Fail()
    {
        return _andSpec.IsSatisfiedBy(_invalidOrder);
    }

    [Benchmark]
    public bool IsSatisfiedBy_Or_Pass()
    {
        return _orSpec.IsSatisfiedBy(_validOrder);
    }

    [Benchmark]
    public bool IsSatisfiedBy_Or_Fail()
    {
        return _orSpec.IsSatisfiedBy(_invalidOrder);
    }

    [Benchmark]
    public bool IsSatisfiedBy_Not()
    {
        return _notSpec.IsSatisfiedBy(_invalidOrder);
    }

    [Benchmark]
    public bool IsSatisfiedBy_Complex()
    {
        return _complexSpec.IsSatisfiedBy(_validOrder);
    }

    #endregion

    #region Expression Tree Generation

    [Benchmark]
    public Expression<Func<Order, bool>> ToExpression_Simple()
    {
        return _highValueSpec.ToExpression();
    }

    [Benchmark]
    public Expression<Func<Order, bool>> ToExpression_And()
    {
        return _andSpec.ToExpression();
    }

    [Benchmark]
    public Expression<Func<Order, bool>> ToExpression_Complex()
    {
        return _complexSpec.ToExpression();
    }

    #endregion

    #region Filtering

    [Benchmark]
    public List<Order> Filter_100_Orders()
    {
        return _orders.Where(_highValueSpec.ToExpression().Compile()).ToList();
    }

    [Benchmark]
    public List<Order> Filter_100_Orders_Composed()
    {
        return _orders.Where(_andSpec.ToExpression().Compile()).ToList();
    }

    #endregion

    #region Test Types

    public record Order(decimal Amount, DateTimeOffset CreatedAt, string Status);

    private sealed class HighValueOrderSpec(decimal threshold) : Specification<Order>
    {
        public override Expression<Func<Order, bool>> ToExpression() =>
            order => order.Amount >= threshold;
    }

    private sealed class RecentOrderSpec(DateTimeOffset now, int days) : Specification<Order>
    {
        public override Expression<Func<Order, bool>> ToExpression() =>
            order => order.CreatedAt >= now.AddDays(-days);
    }

    #endregion
}
