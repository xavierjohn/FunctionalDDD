namespace FunctionalDDD;

using System.Collections.Generic;

public sealed class ErrorList : List<Err>
{
    public ErrorList() { }
    public ErrorList(IEnumerable<Err> errors) : base(errors) { }
    public ErrorList(params Err[] errors) : base(errors) { }

    public bool HasErrors => Count > 0;

    public void Add(ErrorList ec) => AddRange(ec);

    public static implicit operator ErrorList(Err e) => new(new List<Err>() { e });
}
