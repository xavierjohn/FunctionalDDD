namespace FunctionalDdd.ArdalisSpecification.Tests;

/// <summary>
/// Test entity used for mocking Ardalis.Specification interfaces.
/// Must be public for Castle.DynamicProxy to work with Moq.
/// </summary>
public class TestEntity
{
    public int Id { get; init; }
    public required string Name { get; init; }
}