namespace RailwayOrientedProgramming.Tests.Helpers;

using FunctionalDdd;
using System.Collections.Generic;
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
#if DEBUG
            // In DEBUG mode, only listen to our test-specific source
            ShouldListenTo = source => source == _testActivitySource,
#else
            // In RELEASE mode, listen to the default ROP activity source
            ShouldListenTo = source => source.Name == RopTrace.ActivitySourceName,
#endif
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
        
#if DEBUG
        // Inject our test source into RopTrace
        RopTrace.SetTestActivitySource(_testActivitySource);
#endif
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
        var maxWait = timeout ?? TimeSpan.FromSeconds(2);
        return SpinWait.SpinUntil(() => ActivityCount >= expectedCount, maxWait);
    }

    /// <summary>
    /// Waits for an activity with the specified display name to be captured.
    /// </summary>
    public Activity? WaitForActivity(string displayName, TimeSpan? timeout = null)
    {
        var maxWait = timeout ?? TimeSpan.FromSeconds(2);
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

    public IEnumerable<Activity>? WaitForActivities(string displayName, int count, TimeSpan? timeout = null)
    {
        var maxWait = timeout ?? TimeSpan.FromSeconds(2);
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < maxWait)
        {
            lock (_lock)
            {
                IEnumerable<Activity> activity = _capturedActivities.Where(a => a.DisplayName == displayName);
                if (activity != null && activity.Count() >= count)
                    return activity;
            }

            Thread.Sleep(10);
        }

        return null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        // Delay to ensure all pending ActivityStopped callbacks complete.
        // The Activity API is asynchronous - when Activity.Dispose() is called,
        // it doesn't immediately invoke the ActivityStopped callback. Under load
        // or when tests run in parallel, this delay can be significant.
        // We use a 1000ms (1 second) delay to ensure maximum reliability across all test scenarios.
        // This trade-off between test speed and reliability is acceptable for integration tests.
        Thread.Sleep(1000);
        
#if DEBUG
        // Reset RopTrace to use the default ActivitySource
        RopTrace.ResetTestActivitySource();
#endif
        
        // Dispose resources
        _listener.Dispose();
        _testActivitySource.Dispose();
        
        _disposed = true;
    }
}
