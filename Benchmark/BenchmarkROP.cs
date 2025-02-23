﻿namespace Benchmark;
using BenchmarkDotNet.Attributes;
using FunctionalDdd;
using SampleUserLibrary;
using static FunctionalDdd.EnsureExtensions;

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
                ok => ok.Item1 + " " + ok.Item2,
                error => error.Detail
            );

    [Benchmark]
    public string IfStyleHappy()
    {
        var rFirstName = FirstName.TryCreate("Xavier");
        var rEmailAddress = EmailAddress.TryCreate("xavier@somewhere.com");
        if (rFirstName.IsSuccess && rEmailAddress.IsSuccess)
            return rFirstName.Value + " " + rEmailAddress.Value;

        var error = rFirstName.IsFailure ? rFirstName.Error : rEmailAddress.Error;
        if (rEmailAddress.IsFailure)
            error = error.Combine(rEmailAddress.Error);

        return error.Detail;
    }

    [Benchmark]
    public string RopStyleSad() =>
    FirstName.TryCreate("Xavier")
        .Combine(EmailAddress.TryCreate("bad email"))
        .Finally(
            ok => ok.Item1 + " " + ok.Item2,
            error => error.Detail
        );

    [Benchmark]
    public string IfStyleSad()
    {
        var rFirstName = FirstName.TryCreate("Xavier");
        var rEmailAddress = EmailAddress.TryCreate("bad email");
        if (rFirstName.IsSuccess && rEmailAddress.IsSuccess)
            return rFirstName.Value + " " + rEmailAddress.Value;

        var error = rFirstName.IsFailure ? rFirstName.Error : rEmailAddress.Error;
        if (rFirstName.IsFailure && rEmailAddress.IsFailure)
            error = error.Combine(rEmailAddress.Error);

        return error.Detail;
    }

    [Benchmark]
    public Result<string> RopSample1()
    {
        var createdAt = DateTime.UtcNow;
        var updatedAt = createdAt.AddMinutes(-10);
        return EmailAddress.TryCreate("xavier@somewhere.com")
            .Combine(FirstName.TryCreate("Xavier"))
            .Combine(LastName.TryCreate(string.Empty))
            .Combine(EmailAddress.TryCreate("xavier @ somewhereelse.com"))
            .Combine(Ensure(createdAt <= updatedAt, Error.Validation("updateAt cannot be less than createdAt", nameof(updatedAt))))
            .Bind((email, firstName, lastName, anotherEmail) => Result.Success(string.Join(" ", firstName, lastName, email, anotherEmail)));
    }

    [Benchmark]
    public Result<string> IfSample1()
    {
        var createdAt = DateTime.UtcNow;
        var updatedAt = createdAt.AddMinutes(-10);
        var hrEmail = EmailAddress.TryCreate("xavier@somewhere.com");
        var hrFname = FirstName.TryCreate("Xavier");
        var hrLname = LastName.TryCreate(string.Empty);
        var hrEmailSec = EmailAddress.TryCreate("xavier @ somewhereelse.com");
        var hrDateCheck = Ensure(createdAt <= updatedAt, Error.Validation("updateAt cannot be less than createdAt", nameof(updatedAt)));

        Error? error = null;
        if (hrEmail.IsFailure)
            error = hrEmail.Error;
        if (hrFname.IsFailure)
            error = error.Combine(hrFname.Error);
        if (hrLname.IsFailure)
            error = error.Combine(hrLname.Error);
        if (hrEmailSec.IsFailure)
            error = error.Combine(hrEmailSec.Error);
        if (hrDateCheck.IsFailure)
            error = error.Combine(hrDateCheck.Error);

        if (error == null)
        {
            var email = hrEmail.Value;
            var firstName = hrFname.Value;
            var lastName = hrLname.Value;
            var anotherEmail = hrEmailSec.Value;
            return Result.Success(string.Join(" ", firstName, lastName, email, anotherEmail));
        }

        return Result.Failure<string>(error);
    }
}
