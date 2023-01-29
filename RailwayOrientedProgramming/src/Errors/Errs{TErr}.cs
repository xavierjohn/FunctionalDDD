namespace FunctionalDDD;

using System.Collections.Generic;

public sealed class Errs<TErr> : List<TErr>
{
    public Errs() { }
    public Errs(IEnumerable<TErr> errors) : base(errors) { }
    public Errs(params TErr[] errors) : base(errors) { }

    public bool HasErrors => Count > 0;

    public void Add(Errs<TErr> ec) => AddRange(ec);

    public static implicit operator Errs<TErr>(TErr e) => new(new List<TErr>() { e });
}
