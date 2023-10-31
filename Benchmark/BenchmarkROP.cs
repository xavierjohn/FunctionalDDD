namespace Benchmark;
using BenchmarkDotNet.Attributes;
using FunctionalDDD.Domain;
using FunctionalDDD.Results;

/// <summary>
/// Benchmark ROP vs If.
/// | Method   | Mean     | Error   | StdDev  | Gen0   | Allocated |
/// |--------- |---------:|--------:|--------:|-------:|----------:|
/// | RopStyle | 144.9 ns | 1.90 ns | 1.68 ns | 0.0229 |     144 B |
/// | IfStyle  | 147.3 ns | 2.90 ns | 3.22 ns | 0.0637 |     400 B |
/// </summary>

public partial class FirstName : RequiredString
{
}

#pragma warning disable CA1822 // Mark members as static

[MemoryDiagnoser]
public class BenchmarkROP
{
    [Benchmark]
    public string RopStyle() =>
        FirstName.New("Xavier")
            .Combine(EmailAddress.New("xavier@somewhere.com"))
            .Finally(
                ok => ok.Item1.ToString() + " " + ok.Item2.ToString(),
                error => error.Message
            );

    [Benchmark]
    public string IfStyle()
    {
        var firstName = FirstName.New("Xavier");
        var emailAddress = EmailAddress.New("xavier@somewhere.com");
        if (firstName.IsSuccess && emailAddress.IsSuccess)
            return firstName.ToString() + " " + emailAddress.ToString();

        var error = firstName.IsFailure ? firstName.Error : emailAddress.Error;
        if (emailAddress.IsFailure)
            error = error.Combine(emailAddress.Error);

        return error.Message;
    }
}
