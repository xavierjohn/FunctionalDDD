namespace Benchmark;
using BenchmarkDotNet.Attributes;
using FunctionalDDD.Domain;
using FunctionalDDD.Results;
using FunctionalDDD.Results.Errors;

public partial class FirstName : RequiredString
{
}

#pragma warning disable CA1822 // Mark members as static

public class ResultsBenchmark
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
        Error? error = null;
        var firstName = FirstName.New("Xavier");
        if (firstName.IsFailure)
            error = firstName.Error;

        Result<EmailAddress>? emailAddress = null;
        if (error is null)
        {
            emailAddress = EmailAddress.New("xavier@somewhere.com");
            if (emailAddress.Value.IsFailure)
            {
                if (error is null)
                    error = emailAddress.Value.Error;
                else
                    error.Combine(emailAddress.Value.Error);
            }
        }

        return error is null ? firstName.ToString() + " " + emailAddress?.ToString() : error.Message;
    }
}
