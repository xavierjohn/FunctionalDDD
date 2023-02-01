namespace FunctionalDDD;

public sealed class Aggregate : Err
{
    public Aggregate(List<Err> errors, string code) : base(errors[0].Description, code)
    {
        if (errors.Count < 1)
            throw new ArgumentException("At least one error is required", nameof(errors));
        Errors = errors;
    }

    public Aggregate(List<Err> errors) : this(errors, "aggregate.error")
    {
    }


    public IList<Err> Errors { get; }
}
