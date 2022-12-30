namespace FunctionalDDD.RailwayOrientedProgramming;

using System.Collections.Generic;

public sealed class ErrorList : List<Error>
{
    public ErrorList() { }
    public ErrorList(IEnumerable<Error> errors) : base(errors) { }
    public ErrorList(params Error[] errors) : base(errors) { }

    public bool HasErrors => Count > 0;

    public void Add(ErrorList ec) => AddRange(ec);

    public static implicit operator ErrorList(Error e) => new(new List<Error>() { e });
}
