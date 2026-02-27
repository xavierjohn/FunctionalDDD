# Migration from FunctionalDDD

**Time:** 15-30 min | **Prerequisites:** Existing project using `FunctionalDdd.*` packages

This guide covers migrating from the `FunctionalDdd.*` NuGet packages to the renamed `Trellis.*` packages. No API changes — all types, methods, and behaviors remain identical.

## Package Mapping

| Old Package | New Package |
|-------------|-------------|
| `FunctionalDdd.RailwayOrientedProgramming` | `Trellis.Results` |
| `FunctionalDdd.DomainDrivenDesign` | `Trellis.DomainDrivenDesign` |
| `FunctionalDdd.PrimitiveValueObjects` | `Trellis.Primitives` |
| `FunctionalDdd.PrimitiveValueObjectGenerator` | `Trellis.Primitives.Generator` |
| `FunctionalDdd.Asp` | `Trellis.Asp` |
| `FunctionalDdd.Http` | `Trellis.Http` |
| `FunctionalDdd.FluentValidation` | `Trellis.FluentValidation` |
| `FunctionalDdd.ArdalisSpecification` | Removed (replaced by native `Specification<T>` in `Trellis.DomainDrivenDesign`) |

New packages with no previous equivalent:

| Package | Purpose |
|---------|---------|
| `Trellis.Analyzers` | 19 Roslyn analyzers enforcing ROP best practices |
| `Trellis.Testing` | FluentAssertions extensions, test builders, fakes |
| `Trellis.Stateless` | Wraps Stateless state machine Fire() to return Result\<T\> |

## Step-by-Step Migration

### 1. Update Package References

If you use `Directory.Packages.props` (recommended), update it in one place:

```xml
<!-- Before -->
<PackageVersion Include="FunctionalDdd.RailwayOrientedProgramming" Version="2.x.x" />
<PackageVersion Include="FunctionalDdd.DomainDrivenDesign" Version="2.x.x" />
<PackageVersion Include="FunctionalDdd.Asp" Version="2.x.x" />
<PackageVersion Include="FunctionalDdd.PrimitiveValueObjects" Version="2.x.x" />
<PackageVersion Include="FunctionalDdd.PrimitiveValueObjectGenerator" Version="2.x.x" />

<!-- After -->
<PackageVersion Include="Trellis.Results" Version="3.x.x" />
<PackageVersion Include="Trellis.DomainDrivenDesign" Version="3.x.x" />
<PackageVersion Include="Trellis.Asp" Version="3.x.x" />
<PackageVersion Include="Trellis.Primitives" Version="3.x.x" />
<PackageVersion Include="Trellis.Primitives.Generator" Version="3.x.x" />
<PackageVersion Include="Trellis.Analyzers" Version="3.x.x" />
```

Otherwise, update each `.csproj` file individually.

### 2. Update Using Statements

Global find-and-replace in source files:

| Find | Replace |
|------|---------|
| `using FunctionalDdd;` | `using Trellis;` |
| `using FunctionalDdd.PrimitiveValueObjects;` | `using Trellis;` |

All types are now in the `Trellis` namespace.

### 3. Update OpenTelemetry Method Names

If you use auto-instrumentation:

| Find | Replace |
|------|---------|
| `.AddFunctionalDddRopInstrumentation()` | `.AddResultsInstrumentation()` |
| `.AddFunctionalDddCvoInstrumentation()` | `.AddPrimitiveValueObjectInstrumentation()` |

### 4. Build and Test

```bash
dotnet build
dotnet test
```

The compiler and Trellis.Analyzers will flag any issues. All types, methods, and behaviors remain identical — only the namespace and package names have changed.

## No API Changes

All public APIs are identical between FunctionalDdd v2 and Trellis v3:

- `Result<T>`, `Maybe<T>`, all error types — unchanged
- `Bind`, `Map`, `Tap`, `Ensure`, `Match`, `Combine` — unchanged
- `Aggregate<T>`, `Entity<T>`, `ValueObject` — unchanged
- `RequiredString`, `RequiredGuid`, `RequiredInt`, `RequiredDecimal` — unchanged
- `ToActionResult()`, `ToHttpResult()` — unchanged
- All async extensions — unchanged

## Deprecation Notice

The old `FunctionalDdd.*` NuGet packages will be deprecated (not unlisted) with a pointer to the corresponding `Trellis.*` package. Existing code using the old packages will continue to work, but no new features or bug fixes will be applied to the old packages.
