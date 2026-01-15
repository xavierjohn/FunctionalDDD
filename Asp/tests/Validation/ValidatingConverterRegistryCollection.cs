namespace Asp.Tests.Validation;

using Xunit;

/// <summary>
/// Collection definition to ensure tests that access ValidatingConverterRegistry
/// run sequentially to avoid race conditions with the shared static registry.
/// </summary>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
[CollectionDefinition("ValidatingConverterRegistry")]
public class ValidatingConverterRegistryTestCollection : ICollectionFixture<ValidatingConverterRegistryFixture>
{
}
#pragma warning restore CA1711

/// <summary>
/// Fixture for ValidatingConverterRegistry tests.
/// </summary>
public class ValidatingConverterRegistryFixture
{
    // No setup needed - the collection just ensures sequential execution
}
