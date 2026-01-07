namespace FunctionalDdd;

using System.Diagnostics;
using System.Reflection;

internal static class RopTrace
{
    internal static readonly AssemblyName AssemblyName = typeof(RopTrace).Assembly.GetName();
    internal static readonly string ActivitySourceName = "Functional DDD ROP";
    internal static readonly Version Version = AssemblyName.Version!;
    
    private static readonly ActivitySource DefaultActivitySource = new(ActivitySourceName, Version.ToString());
    
#if DEBUG
    private static ActivitySource? _testActivitySource;
#endif
    
    internal static ActivitySource ActivitySource =>
#if DEBUG
        _testActivitySource ??
#endif
        DefaultActivitySource;

#if DEBUG
    /// <summary>
    /// Sets a custom ActivitySource for testing purposes only.
    /// This allows tests to have complete isolation from other tests.
    /// </summary>
    /// <param name="source">The test-specific ActivitySource to use.</param>
    internal static void SetTestActivitySource(ActivitySource source) => _testActivitySource = source;
    
    /// <summary>
    /// Resets the ActivitySource to the default production source.
    /// Should be called in test cleanup/dispose.
    /// </summary>
    internal static void ResetTestActivitySource() => _testActivitySource = null;
#endif
}
