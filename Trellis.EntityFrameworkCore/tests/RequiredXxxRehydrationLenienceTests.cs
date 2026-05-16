namespace Trellis.EntityFrameworkCore.Tests;

using System;
using Trellis;
using Xunit;

// Lenient and strict variants for the POLA realignment rehydration test.
public partial class LenientEfGuid : RequiredGuid<LenientEfGuid> { }

[NotDefault]
public partial class StrictEfGuid : RequiredGuid<StrictEfGuid> { }

public partial class LenientEfDateTime : RequiredDateTime<LenientEfDateTime> { }

[NotDefault]
public partial class StrictEfDateTime : RequiredDateTime<StrictEfDateTime> { }

public partial class LenientEfString : RequiredString<LenientEfString> { }

[Trim, NotDefault]
public partial class StrictEfString : RequiredString<StrictEfString> { }

/// <summary>
/// Regression coverage for the POLA realignment EF read-path impact: <see cref="TrellisScalarConverter{TModel, TProvider}"/>
/// calls <c>TryCreate</c> to materialize every row, so lenient-by-default Required types no longer
/// throw <c>TrellisPersistenceMappingException</c> when a column legitimately contains the
/// sentinel value (<c>Guid.Empty</c>, <c>DateTime.MinValue</c>, <c>""</c>). Strict types decorated
/// with <c>[NotDefault]</c> retain the database-invariant guarantee.
/// </summary>
/// <remarks>
/// Exercises the converter directly via <c>ValueConverter&lt;,&gt;.ConvertFromProvider</c> rather
/// than spinning up a DbContext. The materialization path is the same in both cases — EF Core's
/// query pipeline invokes the same delegate on every row read — so the converter-level assertion
/// is equivalent to the round-trip check but does not require a backing database. This keeps the
/// test compatible with the SQL-Server-less CI environment.
/// </remarks>
public class RequiredXxxRehydrationLenienceTests
{
    [Fact]
    public void LenientGuidConverter_materializes_Guid_Empty_without_throwing()
    {
        var converter = new TrellisScalarConverter<LenientEfGuid, Guid>();
        var materialized = (LenientEfGuid)converter.ConvertFromProvider(Guid.Empty)!;
        materialized.Value.Should().Be(Guid.Empty);
    }

    [Fact]
    public void StrictGuidConverter_throws_TrellisPersistenceMappingException_on_Guid_Empty()
    {
        var converter = new TrellisScalarConverter<StrictEfGuid, Guid>();
        Action act = () => _ = (StrictEfGuid)converter.ConvertFromProvider(Guid.Empty)!;
        act.Should().Throw<TrellisPersistenceMappingException>()
            .WithMessage("*Strict Ef Guid cannot be Guid.Empty*");
    }

    [Fact]
    public void LenientDateTimeConverter_materializes_MinValue_without_throwing()
    {
        var converter = new TrellisScalarConverter<LenientEfDateTime, DateTime>();
        var materialized = (LenientEfDateTime)converter.ConvertFromProvider(DateTime.MinValue)!;
        materialized.Value.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void StrictDateTimeConverter_throws_on_MinValue()
    {
        var converter = new TrellisScalarConverter<StrictEfDateTime, DateTime>();
        Action act = () => _ = (StrictEfDateTime)converter.ConvertFromProvider(DateTime.MinValue)!;
        act.Should().Throw<TrellisPersistenceMappingException>()
            .WithMessage("*Strict Ef Date Time cannot be DateTime.MinValue*");
    }

    [Fact]
    public void LenientStringConverter_materializes_empty_string_without_throwing()
    {
        var converter = new TrellisScalarConverter<LenientEfString, string>();
        var materialized = (LenientEfString)converter.ConvertFromProvider("")!;
        materialized.Value.Should().Be("");
    }

    [Fact]
    public void StrictStringConverter_throws_on_empty_string()
    {
        var converter = new TrellisScalarConverter<StrictEfString, string>();
        Action act = () => _ = (StrictEfString)converter.ConvertFromProvider("")!;
        act.Should().Throw<TrellisPersistenceMappingException>()
            .WithMessage("*Strict Ef String cannot be empty*");
    }
}
