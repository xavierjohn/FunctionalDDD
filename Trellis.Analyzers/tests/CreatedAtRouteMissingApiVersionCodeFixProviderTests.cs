namespace Trellis.Analyzers.Tests;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

/// <summary>
/// Tests for <see cref="CreatedAtRouteMissingApiVersionCodeFixProvider"/> (TRLS023 code fix).
/// Verifies the rewrite produces compilable code by adding the
/// <c>Trellis.Asp.ApiVersioning</c> using directive when missing.
/// </summary>
public sealed class CreatedAtRouteMissingApiVersionCodeFixProviderTests
{
    private const string StubSource = """
        namespace Microsoft.AspNetCore.Routing
        {
            using System.Collections.Generic;
            public class RouteValueDictionary : Dictionary<string, object> { }
        }

        namespace Asp.Versioning
        {
            using System;
            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
            public sealed class ApiVersionAttribute : Attribute
            {
                public ApiVersionAttribute(string version) { }
            }
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

        namespace Trellis.Asp.ApiVersioning
        {
            using System;
            using Microsoft.AspNetCore.Routing;
            using Trellis.Asp;

            public static class HttpResponseOptionsBuilderApiVersioningExtensions
            {
                public static HttpResponseOptionsBuilder<TDomain> CreatedAtVersionedRoute<TDomain>(
                    this HttpResponseOptionsBuilder<TDomain> builder,
                    string routeName,
                    Func<TDomain, RouteValueDictionary> routeValues) => builder;
            }
        }
        """;

    [Fact]
    public async Task Fix_renames_member_and_adds_using_directive()
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

        const string fixedSource = """
            using Asp.Versioning;
            using Microsoft.AspNetCore.Routing;
            using Trellis.Asp;
            using Trellis.Asp.ApiVersioning;

            public sealed record Customer(int Id);

            [ApiVersion("2026-11-12")]
            public class CustomersController
            {
                public void DoIt(HttpResponseOptionsBuilder<Customer> opts)
                {
                    opts.CreatedAtVersionedRoute(
                        "Customers_GetById",
                        c => new RouteValueDictionary { ["id"] = c.Id });
                }
            }
            """;

        var test = new CSharpCodeFixTest<
            CreatedAtRouteMissingApiVersionAnalyzer,
            CreatedAtRouteMissingApiVersionCodeFixProvider,
            DefaultVerifier>
        {
            TestCode = source,
            FixedCode = fixedSource,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        test.TestState.Sources.Add(("Stubs.cs", StubSource));
        test.FixedState.Sources.Add(("Stubs.cs", StubSource));
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(DiagnosticDescriptors.MissingApiVersionRouteValue)
                .WithLocation(12, 9));

        await test.RunAsync();
    }

    [Fact]
    public async Task Fix_does_not_duplicate_existing_using_directive()
    {
        const string source = """
            using Asp.Versioning;
            using Microsoft.AspNetCore.Routing;
            using Trellis.Asp;
            using Trellis.Asp.ApiVersioning;

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

        const string fixedSource = """
            using Asp.Versioning;
            using Microsoft.AspNetCore.Routing;
            using Trellis.Asp;
            using Trellis.Asp.ApiVersioning;

            public sealed record Customer(int Id);

            [ApiVersion("2026-11-12")]
            public class CustomersController
            {
                public void DoIt(HttpResponseOptionsBuilder<Customer> opts)
                {
                    opts.CreatedAtVersionedRoute(
                        "Customers_GetById",
                        c => new RouteValueDictionary { ["id"] = c.Id });
                }
            }
            """;

        var test = new CSharpCodeFixTest<
            CreatedAtRouteMissingApiVersionAnalyzer,
            CreatedAtRouteMissingApiVersionCodeFixProvider,
            DefaultVerifier>
        {
            TestCode = source,
            FixedCode = fixedSource,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        test.TestState.Sources.Add(("Stubs.cs", StubSource));
        test.FixedState.Sources.Add(("Stubs.cs", StubSource));
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(DiagnosticDescriptors.MissingApiVersionRouteValue)
                .WithLocation(13, 9));

        await test.RunAsync();
    }
}
