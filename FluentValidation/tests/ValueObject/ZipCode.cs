﻿namespace FluentValidationExt.Tests;

using FluentValidation;

public class ZipCode : ScalarValueObject<string>
{
    private ZipCode(string value) : base(value)
    {
    }

    public static Result<ZipCode> TryCreate(string? zipCode)
    {
        return s_validationRules.ValidateToResult(zipCode)
            .Map(v => new ZipCode(v!));
    }

    static readonly InlineValidator<string?> s_validationRules = new()
    {
        v => v.RuleFor(zipCode => zipCode).Matches(@"^\d{5}(?:[-\s]\d{4})?$").OverridePropertyName("zipCode") //US Zip codes.
    };
}
