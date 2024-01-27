namespace FunctionalDdd;
public interface IResult<TValue> : IResult
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    TValue Value { get; }
}
