namespace RailwayOrientedProgramming.Tests.Helpers;

using FunctionalDdd;
using System.Diagnostics;

/// <summary>
/// Helper class for testing Activity tracing with complete isolation between tests.
/// Creates a unique ActivitySource per test instance to prevent cross-test contamination.
/// </summary>
public sealed class ActivityTestHelper : IDisposable
{
    private readonly ActivitySource _testActivitySource;
    private readonly ActivityListener _listener;
    private readonly List<Activity> _capturedActivities = new();
    private readonly object _lock = new();
    private bool _disposed;

    public ActivityTestHelper()
    {
        // Create a unique ActivitySource for this test
        _testActivitySource = new ActivitySource($"Test-ROP-{Guid.NewGuid():N}");
        
        // Configure the listener to capture activities from our test source
        _listener = new ActivityListener
        {
            ShouldListenTo = source => source == _testActivitySource,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity =>
            {
                lock (_lock)
                {
                    _capturedActivities.Add(activity);
                }
            }
        };
        
        ActivitySource.AddActivityListener(_listener);
        
        // Inject our test source into RopTrace
        RopTrace.SetTestActivitySource(_testActivitySource);
    }

    /// <summary>
    /// Gets the first activity with the specified display name.
    /// </summary>
    /// <param name="displayName">The activity display name to search for.</param>
    /// <returns>The activity if found; otherwise null.</returns>
    public Activity? GetActivity(string displayName)
    {
        lock (_lock)
        {
            return _capturedActivities.FirstOrDefault(a => a.DisplayName == displayName);
        }
    }

    /// <summary>
    /// Gets all captured activities from this test.
    /// </summary>
    /// <returns>A read-only list of all captured activities.</returns>
    public IReadOnlyList<Activity> GetAllActivities()
    {
        lock (_lock)
        {
            return _capturedActivities.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Gets the number of activities captured so far.
    /// </summary>
    public int ActivityCount
    {
        get
        {
            lock (_lock)
            {
                return _capturedActivities.Count;
            }
        }
    }

    /// <summary>
    /// Clears all captured activities.
    /// </summary>
    public void ClearActivities()
    {
        lock (_lock)
        {
            _capturedActivities.Clear();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        // Delay to ensure all pending ActivityStopped callbacks complete
        // This is especially important for async operations which may complete
        // after the test code finishes executing but before disposal
        // 100ms provides a reliable buffer for async operation completion
        Thread.Sleep(100);
        
        // Reset RopTrace to use the default ActivitySource
        RopTrace.ResetTestActivitySource();
        
        // Dispose resources
        _listener.Dispose();
        _testActivitySource.Dispose();
        
        _disposed = true;
    }
}
