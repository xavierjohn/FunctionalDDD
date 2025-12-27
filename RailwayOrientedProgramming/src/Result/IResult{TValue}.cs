namespace FunctionalDdd;

/// <summary>
/// Generic interface for result types, providing typed access to the success value.
/// </summary>
/// <typeparam name="TValue">The type of the success value.</typeparam>
/// <remarks>
/// This interface extends <see cref="IResult"/> to add strongly-typed access to the success value.
/// </remarks>
public interface IResult<TValue> : IResult
{
    /// <summary>
    /// Gets the success value if this result represents success.
    /// </summary>
    /// <value>The value of type <typeparamref name="TValue"/>.</value>
    /// <exception cref="InvalidOperationException">Thrown when accessed on a failed result.</exception>
    TValue Value { get; }
}
