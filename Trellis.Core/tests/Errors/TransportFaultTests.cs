namespace Trellis.Core.Tests.Errors;

using Trellis.Testing;

public sealed class TransportFaultTests
{
    private sealed record SampleTransportFault(string Name) : ITransportFault;

    [Fact]
    public void Construct_with_fault_carries_fault_instance()
    {
        ITransportFault fault = new SampleTransportFault("http-timeout");

        var error = new Error.TransportFault(fault);

        error.Fault.Should().BeSameAs(fault);
    }

    [Fact]
    public void Kind_is_transport_fault()
    {
        var error = new Error.TransportFault(new SampleTransportFault("method-not-allowed"));

        error.Kind.Should().Be("transport-fault");
    }

    [Fact]
    public void Equality_compares_wrapped_fault_by_value()
    {
        var left = new Error.TransportFault(new SampleTransportFault("http-timeout"));
        var right = new Error.TransportFault(new SampleTransportFault("http-timeout"));

        left.Should().Be(right);
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Different_wrapped_faults_are_not_equal()
    {
        var left = new Error.TransportFault(new SampleTransportFault("http-timeout"));
        var right = new Error.TransportFault(new SampleTransportFault("unsupported-media-type"));

        left.Equals(right).Should().BeFalse();
    }

    [Fact]
    public void Cause_is_preserved_but_excluded_from_equality()
    {
        var cause = new Error.Unexpected("inner")
        {
            Detail = "inner detail",
        };
        var left = new Error.TransportFault(new SampleTransportFault("http-timeout"));
        var right = new Error.TransportFault(new SampleTransportFault("http-timeout"))
        {
            Cause = cause,
        };

        right.Cause.Should().BeSameAs(cause);
        left.Should().Be(right);
    }
}