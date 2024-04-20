﻿namespace RailwayOrientedProgramming.Tests.Maybes;
using FunctionalDdd;

public class OptionalTests
{
    [Fact]
    public void Will_return_Maybe_Value()
    {
        // Arrange
        string? zipCode = "92874";

        // Act
        var result = Maybe.Optional(zipCode, ZipCode.TryCreate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Maybe<ZipCode>>();
        result.Value.Value.Zip.Should().Be(zipCode);
    }

    [Fact]
    public void Will_return_Maybe_None()
    {
        // Arrange
        string? zipCode = null;

        // Act
        var result = Maybe.Optional(zipCode, ZipCode.TryCreate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Maybe<ZipCode>>();
        result.Value.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void Will_return_Failure()
    {
        // Arrange
        string? zipCode = "Hi";

        // Act
        var result = Maybe.Optional(zipCode, ZipCode.TryCreate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<BadRequestError>();
        result.Error.Detail.Should().Be("Invalid ZipCode.");
    }

    class ZipCode
    {
        public string Zip { get; }

        private ZipCode(string zipCode) => Zip = zipCode;

        public static Result<ZipCode> TryCreate(string zipCode)
        {
            if (string.IsNullOrEmpty(zipCode)) return Result.Failure<ZipCode>(Error.BadRequest("ZipCode is required."));
            if (zipCode.Length != 5) return Result.Failure<ZipCode>(Error.BadRequest("Invalid ZipCode."));

            return new ZipCode(zipCode);
        }
    }
}
