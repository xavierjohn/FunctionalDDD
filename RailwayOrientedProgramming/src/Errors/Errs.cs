namespace FunctionalDDD;

using System.Collections.Generic;

public sealed class Errs : List<Err>
{
    public Errs() { }
    public Errs(IEnumerable<Err> errors) : base(errors) { }
    public Errs(params Err[] errors) : base(errors) { }

    public bool HasErrors => Count > 0;

    public void Add(Errs ec) => AddRange(ec);

    public static implicit operator Errs(Err e) => new(new List<Err>() { e });
}
