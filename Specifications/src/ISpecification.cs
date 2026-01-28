namespace FunctionalDdd;

using System.Linq.Expressions;

/// <summary>
/// Encapsulates query logic for filtering, ordering, and including related data.
/// </summary>
/// <typeparam name="T">The entity type to query.</typeparam>
public interface ISpecification<T> where T : class
{
    /// <summary>
    /// The criteria expression for filtering entities.
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Navigation properties to eagerly load.
    /// </summary>
    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// String-based navigation properties for nested includes.
    /// </summary>
    IReadOnlyList<string> IncludeStrings { get; }

    /// <summary>
    /// Ordering expressions (ascending).
    /// </summary>
    IReadOnlyList<Expression<Func<T, object>>> OrderBy { get; }

    /// <summary>
    /// Ordering expressions (descending).
    /// </summary>
    IReadOnlyList<Expression<Func<T, object>>> OrderByDescending { get; }

    /// <summary>
    /// Number of records to skip for pagination.
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Maximum number of records to return.
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// Whether to disable change tracking (AsNoTracking).
    /// </summary>
    bool AsNoTracking { get; }

    /// <summary>
    /// Whether to split queries for includes (AsSplitQuery).
    /// </summary>
    bool AsSplitQuery { get; }
}
