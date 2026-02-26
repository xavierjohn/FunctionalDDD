# Changelog

All notable changes to the FunctionalDDD project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

#### Maybe<T> — First-Class Domain-Level Optionality

`Maybe<T>` now has a `notnull` constraint and new transformation methods, making it a proper domain-level optionality type:

- **`notnull` constraint** — `Maybe<T> where T : notnull` prevents wrapping nullable types
- **`Map<TResult>`** — Transform the inner value: `maybe.Map(url => url.Value)` returns `Maybe<string>`
- **`Match<TResult>`** — Pattern match: `maybe.Match(url => url.Value, () => "none")`
- **Implicit operator** — `Maybe<Url> m = url;` works naturally

#### ASP.NET Core Maybe<T> Integration

Full support for optional value object properties in DTOs:

- **`MaybeScalarValueJsonConverter<TValue,TPrimitive>`** — JSON deserialization: `null` → `Maybe.None`, valid → `Maybe.From(validated)`, invalid → validation error collected
- **`MaybeScalarValueJsonConverterFactory`** — Auto-discovers `Maybe<T>` properties on DTOs
- **`MaybeModelBinder<TValue,TPrimitive>`** — MVC model binding: absent/empty → `Maybe.None`, valid → `Maybe.From(result)`, invalid → ModelState error
- **`MaybeSuppressChildValidationMetadataProvider`** — Prevents MVC from requiring child properties on `Maybe<T>` (fixes MVC crash)
- **`ScalarValueTypeHelper`** additions — `IsMaybeScalarValue()`, `GetMaybeInnerType()`, `GetMaybePrimitiveType()`
- **SampleWeb apps** updated — `Maybe<Url> Website` on User/RegisterUserDto, `Maybe<FirstName> AssignedTo` on UpdateOrderDto

### Changed

- `Maybe<T>` now requires `where T : notnull` — see [Migration Guide](MIGRATION_v3.md#maybe-notnull-constraint) for details

---

#### FunctionalDDD.Analyzers - NEW Package! 🎉

A comprehensive suite of 18 Roslyn analyzers to enforce Railway Oriented Programming best practices at compile time:

**Safety Rules (Warnings):**
- **TRLS001**: Detect unhandled Result return values
- **TRLS003**: Prevent unsafe `Result.Value` access without `IsSuccess` check
- **TRLS004**: Prevent unsafe `Result.Error` access without `IsFailure` check
- **TRLS006**: Prevent unsafe `Maybe.Value` access without `HasValue` check
- **TRLS007**: Suggest `Create()` instead of `TryCreate().Value` for clearer intent
- **TRLS008**: Detect `Result<Result<T>>` double wrapping
- **TRLS009**: Prevent blocking on `Task<Result<T>>` with `.Result` or `.Wait()`
- **TRLS011**: Detect `Maybe<Maybe<T>>` double wrapping
- **TRLS014**: Detect async lambda used with sync method (Map instead of MapAsync)
- **TRLS015**: Don't throw exceptions in Result chains (defeats ROP purpose)
- **TRLS016**: Empty error messages provide no debugging context
- **TRLS017**: Don't compare Result/Maybe to null (they're structs)
- **TRLS018**: Unsafe `.Value` access in LINQ without filtering first

**Best Practice Rules (Info):**
- **TRLS002**: Suggest `Bind` instead of `Map` when lambda returns Result
- **TRLS005**: Suggest `MatchError` for type-safe error discrimination
- **TRLS010**: Suggest specific error types instead of base `Error` class
- **TRLS012**: Suggest `Result.Combine()` for multiple Result checks
- **TRLS013**: Suggest `GetValueOrDefault`/`Match` instead of ternary operator

**Benefits:**
- ✅ Catch common ROP mistakes at compile time
- ✅ Guide developers toward best practices
- ✅ Improve code quality and maintainability
- ✅ 149 comprehensive tests ensuring accuracy

**Installation:**
```bash
dotnet add package FunctionalDdd.Analyzers
```

**Documentation:** [Analyzer Documentation](Analyzers/src/README.md)

---

## Previous Releases


[Unreleased]: https://github.com/xavierjohn/FunctionalDDD/compare/v1.0.0...HEAD
