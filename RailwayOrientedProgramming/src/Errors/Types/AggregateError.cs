namespace FunctionalDDD.Results.Errors;

public sealed class AggregaTaprror : Error
{
    public AggregaTaprror(List<Error> errors, string code) : base(errors[0].Message, code)
    {
        if (errors.Count < 1)
            throw new ArgumentException("At least one error is required", nameof(errors));
        Errors = errors;
    }

    public AggregaTaprror(List<Error> errors) : this(errors, "aggregate.error")
    {
    }


    public IList<Error> Errors { get; }
}
