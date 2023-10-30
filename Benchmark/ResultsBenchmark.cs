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
                ok => "Success",
                error => error.Message
            );

    [Benchmark]
    public string IfStyle()
    {
        Error? error = null;
        var firstName = FirstName.New("Xavier");
        if (firstName.IsFailure)
            error = firstName.Error;

        if (error is null)
        {
            var emailAddress = EmailAddress.New("xavier@somewhere.com");
            if (emailAddress.IsFailure)
            {
                if (error is null)
                    error = emailAddress.Error;
                else
                {
                    error.Combine(emailAddress.Error);
                }
            }
        }

        return error is null ? "Success" : error.Message;

    }
}
