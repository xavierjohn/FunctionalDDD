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
    /// Waits until the specified number of activities have been captured.
    /// </summary>
    public bool WaitForActivityCount(int expectedCount, TimeSpan? timeout = null)
    {
        var maxWait = timeout ?? TimeSpan.FromSeconds(1);
        return SpinWait.SpinUntil(() => ActivityCount >= expectedCount, maxWait);
    }

    /// <summary>
    /// Waits for an activity with the specified display name to be captured.
    /// </summary>
    public Activity? WaitForActivity(string displayName, TimeSpan? timeout = null)
    {
        var maxWait = timeout ?? TimeSpan.FromSeconds(1);
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < maxWait)
        {
            lock (_lock)
            {
                var activity = _capturedActivities.FirstOrDefault(a => a.DisplayName == displayName);
                if (activity != null)
                    return activity;
            }

            Thread.Sleep(10);
        }

        return null;
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
