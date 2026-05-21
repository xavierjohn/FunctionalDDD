# Trellis.Http.Abstractions

[![NuGet Package](https://img.shields.io/nuget/v/Trellis.Http.Abstractions.svg)](https://www.nuget.org/packages/Trellis.Http.Abstractions)

HTTP-aware error and representation primitives shared by Trellis.Asp on the server side and Trellis.Http on the client side.

`Trellis.Core` now keeps its domain error union transport-neutral. HTTP-specific failures live in the closed `HttpError` union and flow through `Result<T>` via `Error.TransportFault(ITransportFault)`. This package also owns the HTTP vocabulary that used to live in Core: entity tags, precondition kinds, authentication challenges, retry-after values, representation metadata, and write outcomes.

Use this package when boundary code needs to construct, inspect, or round-trip HTTP failure/context payloads without reintroducing HTTP/RFC concepts into the domain core.

## Installation

```bash
dotnet add package Trellis.Http.Abstractions
```

## Quick example

```csharp
using Trellis;

Error error = new Error.TransportFault(
    new HttpError.MethodNotAllowed(EquatableArray.Create("GET", "PUT"))
    {
        Detail = "The target resource does not support PATCH.",
    });
```