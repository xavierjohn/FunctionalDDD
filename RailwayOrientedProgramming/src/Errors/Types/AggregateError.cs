namespace FunctionalDdd;

public sealed class AggregateError : Error
{
    public AggregateError(List<Error> errors, string code) : base("Aggregated error", code)
    {
        if (errors.Count < 1)
            throw new ArgumentException("At least one error is required", nameof(errors));
        Errors = errors;
    }

    public AggregateError(List<Error> errors) : this(errors, "aggregate.error")
    {
    }


    public IList<Error> Errors { get; }
}
