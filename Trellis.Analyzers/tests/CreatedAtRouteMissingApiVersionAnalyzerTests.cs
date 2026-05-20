namespace Trellis.Analyzers.Tests;

using System.Threading.Tasks;
using Xunit;

/// <summary>
/// Tests for <see cref="CreatedAtRouteMissingApiVersionAnalyzer"/> (TRLS023).
/// Verifies that <c>HttpResponseOptionsBuilder&lt;T&gt;.CreatedAtRoute(...)</c> calls inside
/// <c>[ApiVersion]</c>-decorated controllers produce a warning when the route-values
/// dictionary literal does not include an <c>"api-version"</c> key.
/// </summary>
public sealed class CreatedAtRouteMissingApiVersionAnalyzerTests
{
    /// <summary>
    /// Stubs the minimal Trellis.Asp + Asp.Versioning + Microsoft.AspNetCore.Routing surface
    /// the analyzer needs to recognise.
    /// </summary>
    private const string StubSource = """
        namespace Microsoft.AspNetCore.Routing
        {
            using System.Collections.Generic;
            public partial class RouteValueDictionary : Dictionary<string, object> { }
        }

        namespace Asp.Versioning
        {
            using System;
            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
            public sealed class ApiVersionAttribute : Attribute
            {
                public ApiVersionAttribute(string version) { }
            }

            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
            public sealed class ApiVersionNeutralAttribute : Attribute { }
        }

        namespace Trellis.Asp
        {
            using System;
            using Microsoft.AspNetCore.Routing;

            public sealed class HttpResponseOptionsBuilder<TDomain>
            {
                public HttpResponseOptionsBuilder<TDomain> CreatedAtRoute(string routeName, Func<TDomain, RouteValueDictionary> routeValues) => this;
            }
        }
        """;

    [Fact]
    public async Task CreatedAtRoute_on_versioned_controller_without_api_version_key_produces_warning()
    {
        const string source = """
            using Asp.Versioning;
            using Microsoft.AspNetCore.Routing;
            using Trellis.Asp;

            public sealed record Customer(int Id);

            [ApiVersion("2026-11-12")]
            public class CustomersController
            {
                public void DoIt(HttpResponseOptionsBuilder<Customer> opts)
                {
                    opts.CreatedAtRoute(
                        "Customers_GetById",
                        c => new RouteValueDictionary { ["id"] = c.Id });
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<CreatedAtRouteMissingApiVersionAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.MissingApiVersionRouteValue)
                .WithLocation(18, 9)
                .WithArguments("CreatedAtRoute"));
        test.TestState.Sources.Add(("Stubs.cs", StubSource));

        await test.RunAsync();
    }

    [Fact]
    public async Task CreatedAtRoute_on_versioned_controller_with_api_version_key_produces_no_warning()
    {
        // The author already remembered to add api-version manually — no migration required.
        const string source = """
            using Asp.Versioning;
            using Microsoft.AspNetCore.Routing;
            using Trellis.Asp;

            public sealed record Customer(int Id);

            [ApiVersion("2026-11-12")]
            public class CustomersController
            {
                public void DoIt(HttpResponseOptionsBuilder<Customer> opts)
                {
                    opts.CreatedAtRoute(
                        "Customers_GetById",
                        c => new RouteValueDictionary { ["id"] = c.Id, ["api-version"] = "2026-11-12" });
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<CreatedAtRouteMissingApiVersionAnalyzer>(source);
        test.TestState.Sources.Add(("Stubs.cs", StubSource));

        await test.RunAsync();
    }

    [Fact]
    public async Task CreatedAtRoute_on_non_versioned_controller_produces_no_warning()
    {
        // No [ApiVersion] on the class — this controller doesn't participate in versioning at all,
        // so the api-version requirement doesn't apply.
        const string source = """
            using Microsoft.AspNetCore.Routing;
            using Trellis.Asp;

            public sealed record Customer(int Id);

            public class CustomersController
            {
                public void DoIt(HttpResponseOptionsBuilder<Customer> opts)
                {
                    opts.CreatedAtRoute(
                        "Customers_GetById",
                        c => new RouteValueDictionary { ["id"] = c.Id });
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<CreatedAtRouteMissingApiVersionAnalyzer>(source);
        test.TestState.Sources.Add(("Stubs.cs", StubSource));

        await test.RunAsync();
    }

    [Fact]
    public async Task CreatedAtRoute_on_ApiVersionNeutral_controller_produces_no_warning()
    {
        // [ApiVersionNeutral] explicitly opts out of versioning — Location must NOT carry api-version.
        const string source = """
            using Asp.Versioning;
            using Microsoft.AspNetCore.Routing;
            using Trellis.Asp;

            public sealed record Customer(int Id);

            [ApiVersionNeutral]
            [ApiVersion("2026-11-12")]
            public class CustomersController
            {
                public void DoIt(HttpResponseOptionsBuilder<Customer> opts)
                {
                    opts.CreatedAtRoute(
                        "Customers_GetById",
                        c => new RouteValueDictionary { ["id"] = c.Id });
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<CreatedAtRouteMissingApiVersionAnalyzer>(source);
        test.TestState.Sources.Add(("Stubs.cs", StubSource));

        await test.RunAsync();
    }

    [Fact]
    public async Task CreatedAtRoute_with_non_literal_routeValues_produces_no_warning()
    {
        // The analyzer can't inspect dynamically-built dictionaries — bail to false-negative
        // rather than false-positive. The runtime helper migration remains the right answer
        // but the warning would be unactionable on this shape.
        const string source = """
            using Asp.Versioning;
            using Microsoft.AspNetCore.Routing;
            using Trellis.Asp;

            public sealed record Customer(int Id);

            [ApiVersion("2026-11-12")]
            public class CustomersController
            {
                public void DoIt(HttpResponseOptionsBuilder<Customer> opts)
                {
                    var rv = new RouteValueDictionary { ["id"] = 1 };
                    opts.CreatedAtRoute("Customers_GetById", _ => rv);
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<CreatedAtRouteMissingApiVersionAnalyzer>(source);
        test.TestState.Sources.Add(("Stubs.cs", StubSource));

        await test.RunAsync();
    }

    [Fact]
    public async Task CreatedAtRoute_with_case_insensitive_api_version_key_produces_no_warning()
    {
        // RouteValueDictionary keys are case-insensitive at runtime — "API-VERSION" already
        // satisfies the requirement, so the analyzer must not fire.
        const string source = """
            using Asp.Versioning;
            using Microsoft.AspNetCore.Routing;
            using Trellis.Asp;

            public sealed record Customer(int Id);

            [ApiVersion("2026-11-12")]
            public class CustomersController
            {
                public void DoIt(HttpResponseOptionsBuilder<Customer> opts)
                {
                    opts.CreatedAtRoute(
                        "Customers_GetById",
                        c => new RouteValueDictionary { ["id"] = c.Id, ["API-Version"] = "2026-11-12" });
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<CreatedAtRouteMissingApiVersionAnalyzer>(source);
        test.TestState.Sources.Add(("Stubs.cs", StubSource));

        await test.RunAsync();
    }

    [Fact]
    public async Task CreatedAtRoute_on_derived_controller_without_own_ApiVersion_produces_no_warning()
    {
        // [ApiVersion] is declared with Inherited = false. A derived class that doesn't carry
        // its own [ApiVersion] is NOT versioned by API Versioning — the analyzer must match.
        const string source = """
            using Asp.Versioning;
            using Microsoft.AspNetCore.Routing;
            using Trellis.Asp;

            public sealed record Customer(int Id);

            [ApiVersion("2026-11-12")]
            public class BaseController { }

            public class CustomersController : BaseController
            {
                public void DoIt(HttpResponseOptionsBuilder<Customer> opts)
                {
                    opts.CreatedAtRoute(
                        "Customers_GetById",
                        c => new RouteValueDictionary { ["id"] = c.Id });
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<CreatedAtRouteMissingApiVersionAnalyzer>(source);
        test.TestState.Sources.Add(("Stubs.cs", StubSource));

        await test.RunAsync();
    }

    [Fact]
    public async Task CreatedAtRoute_with_anonymous_object_ctor_arg_produces_warning()
    {
        // C# property names cannot contain hyphens, so `new RouteValueDictionary(new { id = 1 })`
        // can never carry an "api-version" key — the analyzer must fire.
        const string source = """
            using Asp.Versioning;
            using Microsoft.AspNetCore.Routing;
            using Trellis.Asp;

            public sealed record Customer(int Id);

            [ApiVersion("2026-11-12")]
            public class CustomersController
            {
                public void DoIt(HttpResponseOptionsBuilder<Customer> opts)
                {
                    opts.CreatedAtRoute(
                        "Customers_GetById",
                        c => new RouteValueDictionary(new { id = c.Id }));
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateDiagnosticTest<CreatedAtRouteMissingApiVersionAnalyzer>(
            source,
            AnalyzerTestHelper.Diagnostic(DiagnosticDescriptors.MissingApiVersionRouteValue)
                .WithLocation(18, 9)
                .WithArguments("CreatedAtRoute"));
        test.TestState.Sources.Add(("Stubs.cs", StubSource));

        // The stubs declare RouteValueDictionary as inheriting from Dictionary<string,object>,
        // which doesn't have an (object) constructor — extend the stub locally just for this test.
        const string ctorStub = """
            namespace Microsoft.AspNetCore.Routing
            {
                public partial class RouteValueDictionary
                {
                    public RouteValueDictionary(object values) { }
                }
            }
            """;
        test.TestState.Sources.Add(("CtorStub.cs", ctorStub));

        await test.RunAsync();
    }

    [Fact]
    public async Task CreatedAtRoute_with_const_string_api_version_key_produces_no_warning()
    {
        // `[ApiVersionKey] = ...` where `ApiVersionKey = "api-version"` is a const string —
        // the analyzer must resolve the constant value via the semantic model and treat it as
        // satisfying the api-version requirement.
        const string source = """
            using Asp.Versioning;
            using Microsoft.AspNetCore.Routing;
            using Trellis.Asp;

            public sealed record Customer(int Id);

            [ApiVersion("2026-11-12")]
            public class CustomersController
            {
                public const string ApiVersionKey = "api-version";

                public void DoIt(HttpResponseOptionsBuilder<Customer> opts)
                {
                    opts.CreatedAtRoute(
                        "Customers_GetById",
                        c => new RouteValueDictionary { ["id"] = c.Id, [ApiVersionKey] = "2026-11-12" });
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<CreatedAtRouteMissingApiVersionAnalyzer>(source);
        test.TestState.Sources.Add(("Stubs.cs", StubSource));

        await test.RunAsync();
    }

    [Fact]
    public async Task CreatedAtRoute_on_method_level_ApiVersionNeutral_action_produces_no_warning()
    {
        // [ApiVersionNeutral] is valid on actions (AttributeTargets.Class | Method). A versioned
        // controller may contain a single neutral action — TRLS023 must respect method-level
        // neutrality and not fire on CreatedAtRoute calls inside that action.
        const string source = """
            using Asp.Versioning;
            using Microsoft.AspNetCore.Routing;
            using Trellis.Asp;

            public sealed record Customer(int Id);

            [ApiVersion("2026-11-12")]
            public class CustomersController
            {
                [ApiVersionNeutral]
                public void DoIt(HttpResponseOptionsBuilder<Customer> opts)
                {
                    opts.CreatedAtRoute(
                        "Customers_GetById",
                        c => new RouteValueDictionary { ["id"] = c.Id });
                }
            }
            """;

        var test = AnalyzerTestHelper.CreateNoDiagnosticTest<CreatedAtRouteMissingApiVersionAnalyzer>(source);
        test.TestState.Sources.Add(("Stubs.cs", StubSource));

        await test.RunAsync();
    }
}
