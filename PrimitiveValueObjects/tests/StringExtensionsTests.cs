namespace PrimitiveValueObjects.Tests;

using FluentAssertions;
using FunctionalDdd;
using Xunit;

public class StringExtensionsTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("A", "a")]
    [InlineData("Email", "email")]
    [InlineData("alreadyCamel", "alreadyCamel")]
    public void ToCamelCase_handles_various_inputs(string? input, string expected)
    {
        // Act
        var actual = input.ToCamelCase();

        // Assert
        actual.Should().Be(expected);
    }
}
