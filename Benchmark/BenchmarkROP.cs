namespace Benchmark;
using BenchmarkDotNet.Attributes;
using FunctionalDDD.Domain;
using FunctionalDDD.Results;

/// <summary>
/// Benchmark ROP vs If.
/// Run 1
/// | Method        | Mean      | Error    | StdDev   | Gen0   | Allocated |
/// |-------------- |----------:|---------:|---------:|-------:|----------:|
/// | RopStyleHappy | 147.62 ns | 1.529 ns | 1.430 ns | 0.0229 |     144 B |
/// | IfStyleHappy  | 126.27 ns | 0.916 ns | 0.812 ns | 0.0229 |     144 B |
/// | RopStyleSad   |  77.76 ns | 1.169 ns | 1.093 ns | 0.0331 |     208 B |
/// | IfStyleSad    |  72.32 ns | 1.400 ns | 1.309 ns | 0.0331 |     208 B |
/// 
/// Run 2
/// | Method        | Mean      | Error    | StdDev   | Gen0   | Allocated |
/// |-------------- |----------:|---------:|---------:|-------:|----------:|
/// | RopStyleHappy | 145.82 ns | 2.058 ns | 1.825 ns | 0.0229 |     144 B |
/// | IfStyleHappy  | 123.99 ns | 2.309 ns | 2.047 ns | 0.0229 |     144 B |
/// | RopStyleSad   |  79.50 ns | 1.395 ns | 1.305 ns | 0.0331 |     208 B |
/// | IfStyleSad    |  73.71 ns | 1.151 ns | 0.961 ns | 0.0331 |     208 B |
/// </summary>

public partial class FirstName : RequiredString
{
}

#pragma warning disable CA1822 // Mark members as static

[MemoryDiagnoser]
public class BenchmarkROP
{
    [Benchmark]
    public string RopStyleHappy() =>
        FirstName.TryCreate("Xavier")
            .Combine(EmailAddress.TryCreate("xavier@somewhere.com"))
            .Finally(
                ok => ok.Item1.ToString() + " " + ok.Item2.ToString(),
                error => error.Message
            );

    [Benchmark]
    public string IfStyleHappy()
    {
        var firstName = FirstName.TryCreate("Xavier");
        var emailAddress = EmailAddress.TryCreate("xavier@somewhere.com");
        if (firstName.IsSuccess && emailAddress.IsSuccess)
            return firstName.Value.ToString() + " " + emailAddress.Value.ToString();

        var error = firstName.IsFailure ? firstName.Error : emailAddress.Error;
        if (emailAddress.IsFailure)
            error = error.Combine(emailAddress.Error);

        return error.Message;
    }

    [Benchmark]
    public string RopStyleSad() =>
    FirstName.TryCreate("Xavier")
        .Combine(EmailAddress.TryCreate("bad email"))
        .Finally(
            ok => ok.Item1.ToString() + " " + ok.Item2.ToString(),
            error => error.Message
        );

    [Benchmark]
    public string IfStyleSad()
    {
        var firstName = FirstName.TryCreate("Xavier");
        var emailAddress = EmailAddress.TryCreate("bad email");
        if (firstName.IsSuccess && emailAddress.IsSuccess)
            return firstName.ToString() + " " + emailAddress.ToString();

        var error = firstName.IsFailure ? firstName.Error : emailAddress.Error;
        if (firstName.IsFailure && emailAddress.IsFailure)
            error = error.Combine(emailAddress.Error);

        return error.Message;
    }
}
