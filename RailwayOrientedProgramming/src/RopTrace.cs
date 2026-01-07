namespace FunctionalDdd;

using System.Diagnostics;
using System.Reflection;

internal static class RopTrace
{
    internal static readonly AssemblyName AssemblyName = typeof(RopTrace).Assembly.GetName();
    internal static readonly string ActivitySourceName = "Functional DDD ROP";
    internal static readonly Version Version = AssemblyName.Version!;
    
    private static readonly ActivitySource DefaultActivitySource = new(ActivitySourceName, Version.ToString());
    
    // Use AsyncLocal for test isolation - works across async boundaries and is thread-safe
    private static readonly AsyncLocal<ActivitySource?> _testActivitySource = new();
    
    internal static ActivitySource ActivitySource =>
        _testActivitySource.Value ?? DefaultActivitySource;

    /// <summary>
    /// Sets a custom ActivitySource for testing purposes.
    /// This allows tests to have complete isolation from other tests.
    /// Uses AsyncLocal to ensure proper isolation even with async tests and parallel execution.
    /// </summary>
    /// <param name="source">The test-specific ActivitySource to use.</param>
    internal static void SetTestActivitySource(ActivitySource source) => _testActivitySource.Value = source;
    
    /// <summary>
    /// Resets the ActivitySource to the default production source.
    /// Should be called in test cleanup/dispose.
    /// </summary>
    internal static void ResetTestActivitySource() => _testActivitySource.Value = null;
}
