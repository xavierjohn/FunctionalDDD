namespace PrimitiveValueObjects.Tests;

using FunctionalDdd;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System.Diagnostics;
using Xunit;

/// <summary>
/// Tests for PvoTracingExtensions to verify OpenTelemetry integration.
/// </summary>
public class PvoTracingExtensionsTests : IDisposable
{
    private readonly List<Activity> _capturedActivities = new();
    private readonly ActivityListener _listener;

    public PvoTracingExtensionsTests()
    {
        // Configure listener to capture activities
        _listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == PrimitiveValueObjectTrace.ActivitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity =>
            {
                lock (_capturedActivities)
                {
                    _capturedActivities.Add(activity);
                }
            }
        };

        ActivitySource.AddActivityListener(_listener);
    }

    [Fact]
    public void AddFunctionalDddPvoInstrumentation_RegistersActivitySource()
    {
        // Arrange
        var builder = Sdk.CreateTracerProviderBuilder();

        // Act
        var result = builder.AddPrimitiveValueObjectInstrumentation();

        // Assert - Method should return builder for chaining
        result.Should().BeSameAs(builder);
        result.Should().NotBeNull();
    }

    [Fact]
    public void AddFunctionalDddPvoInstrumentation_EnablesActivityCapture()
    {
        // Arrange
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddPrimitiveValueObjectInstrumentation()
            .Build();

        // Act
        var emailResult = EmailAddress.TryCreate("test@example.com");

        // Give activities time to be recorded
        SpinWait.SpinUntil(() => _capturedActivities.Count > 0, TimeSpan.FromSeconds(1));

        // Assert
        emailResult.IsSuccess.Should().BeTrue();
        _capturedActivities.Should().ContainSingle();
        var activity = _capturedActivities.First();
        activity.DisplayName.Should().Be("EmailAddress.TryCreate");
        activity.Status.Should().Be(ActivityStatusCode.Ok);
    }

    [Fact]
    public void AddFunctionalDddPvoInstrumentation_SupportsMethodChaining()
    {
        // Arrange & Act
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddPrimitiveValueObjectInstrumentation()
            .AddSource("TestSource")  // Chain another call
            .Build();

        // Assert
        tracerProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddFunctionalDddPvoInstrumentation_RegistersCorrectActivitySourceName()
    {
        // Arrange
        var expectedSourceName = "Functional DDD PVO";

        // Act
        var actualSourceName = PrimitiveValueObjectTrace.ActivitySourceName;

        // Assert
        actualSourceName.Should().Be(expectedSourceName);
    }

    [Fact]
    public void EmailAddress_WithTracing_CreatesActivityWithCorrectName()
    {
        // Act
        var _ = EmailAddress.TryCreate("user@domain.com");

        // Give activities time to be recorded
        SpinWait.SpinUntil(() => _capturedActivities.Count > 0, TimeSpan.FromSeconds(1));

        // Assert
        var activity = _capturedActivities.Should().ContainSingle().Subject;
        activity.OperationName.Should().Be("EmailAddress.TryCreate");
        activity.Source.Name.Should().Be(PrimitiveValueObjectTrace.ActivitySourceName);
    }

    [Fact]
    public void EmailAddress_SuccessfulCreation_SetsOkStatus()
    {
        // Act
        var emailResult = EmailAddress.TryCreate("test@example.com");

        // Give activities time to be recorded
        SpinWait.SpinUntil(() => _capturedActivities.Count > 0, TimeSpan.FromSeconds(1));

        // Assert
        emailResult.IsSuccess.Should().BeTrue();
        _capturedActivities.Should().ContainSingle();
        var activity = _capturedActivities.First();
        activity.DisplayName.Should().Be("EmailAddress.TryCreate");
        activity.Status.Should().Be(ActivityStatusCode.Ok);
    }

    [Fact]
    public void EmailAddress_ValidationFailure_SetsErrorStatus()
    {
        // Act
        var emailResult = EmailAddress.TryCreate("invalid-email");

        // Give activities time to be recorded
        SpinWait.SpinUntil(() => _capturedActivities.Count > 0, TimeSpan.FromSeconds(1));

        // Assert
        emailResult.IsFailure.Should().BeTrue();
        _capturedActivities.Should().ContainSingle();
        var activity = _capturedActivities.First();
        activity.DisplayName.Should().Be("EmailAddress.TryCreate");
        activity.Status.Should().Be(ActivityStatusCode.Error);
    }

    [Fact]
    public void EmailAddress_MultipleOperations_CapturesAllActivities()
    {
        // Act
        var email1 = EmailAddress.TryCreate("valid@example.com");
        var email2 = EmailAddress.TryCreate("invalid");
        var email3 = EmailAddress.TryCreate("another@test.com");

        // Give activities time to be recorded
        SpinWait.SpinUntil(() => _capturedActivities.Count >= 3, TimeSpan.FromSeconds(2));

        // Assert
        _capturedActivities.Should().HaveCount(3);
        _capturedActivities.Should().AllSatisfy(a => a.DisplayName.Should().Be("EmailAddress.TryCreate"));
        
        // Verify statuses
        _capturedActivities[0].Status.Should().Be(ActivityStatusCode.Ok);  // valid
        _capturedActivities[1].Status.Should().Be(ActivityStatusCode.Error); // invalid
        _capturedActivities[2].Status.Should().Be(ActivityStatusCode.Ok);  // valid
    }

    [Fact]
    public void PrimitiveValueObjectTrace_HasCorrectVersion()
    {
        // Act
        var version = PrimitiveValueObjectTrace.Version;

        // Assert
        version.Should().NotBeNull();
        version.Should().Be(PrimitiveValueObjectTrace.AssemblyName.Version);
    }

    [Fact]
    public void ActivitySource_IsNotNull()
    {
        // Act
        var activitySource = PrimitiveValueObjectTrace.ActivitySource;

        // Assert
        activitySource.Should().NotBeNull();
        activitySource.Name.Should().Be(PrimitiveValueObjectTrace.ActivitySourceName);
        activitySource.Version.Should().Be(PrimitiveValueObjectTrace.Version.ToString());
    }

    [Fact]
    public void PrimitiveValueObjectTrace_ActivitySourceName_MatchesConstant()
    {
        // Arrange
        var expectedName = "Functional DDD PVO";

        // Act & Assert
        PrimitiveValueObjectTrace.ActivitySourceName.Should().Be(expectedName);
        PrimitiveValueObjectTrace.ActivitySource.Name.Should().Be(expectedName);
    }

    public void Dispose()
    {
        _listener.Dispose();
        GC.SuppressFinalize(this);
    }
}
