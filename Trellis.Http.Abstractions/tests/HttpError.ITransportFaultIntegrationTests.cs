namespace Trellis.Http.Abstractions.Tests;

using Trellis.Testing;

public sealed class HttpErrorITransportFaultIntegrationTests
{
    public static TheoryData<HttpError> FaultCases => new()
    {
        new HttpError.MethodNotAllowed(EquatableArray.Create("GET")),
        new HttpError.NotAcceptable(EquatableArray.Create("application/json")),
        new HttpError.UnsupportedMediaType(EquatableArray.Create("application/json")),
        new HttpError.RangeNotSatisfiable(64),
        new HttpError.ContentTooLarge(1024),
        new HttpError.PreconditionFailed(ResourceRef.For("Widget", 42), PreconditionKind.IfMatch),
        new HttpError.PreconditionRequired(PreconditionKind.IfNoneMatch),
    };

    [Theory]
    [MemberData(nameof(FaultCases))]
    public void Every_http_error_case_is_assignable_to_transport_fault(HttpError error) =>
        error.Should().BeAssignableTo<ITransportFault>();

    [Fact]
    public void Error_transport_fault_accepts_http_error()
    {
        var httpError = new HttpError.NotAcceptable(EquatableArray.Create("application/json"));

        var error = new Error.TransportFault(httpError);

        error.Fault.Should().BeSameAs(httpError);
    }

    [Fact]
    public void Pattern_matching_can_match_inner_http_error_case()
    {
        Error error = new Error.TransportFault(new HttpError.NotAcceptable(EquatableArray.Create("application/json")));

        var matched = error is Error.TransportFault { Fault: HttpError.NotAcceptable };

        matched.Should().BeTrue();
    }
}