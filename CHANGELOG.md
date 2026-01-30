# Changelog

All notable changes to the FunctionalDDD project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

#### FunctionalDDD.Analyzers - NEW Package! 🎉

A comprehensive suite of 14 Roslyn analyzers to enforce Railway Oriented Programming best practices at compile time:

**Safety Rules (Warnings):**
- **FDDD001**: Detect unhandled Result return values
- **FDDD003**: Prevent unsafe `Result.Value` access without `IsSuccess` check
- **FDDD004**: Prevent unsafe `Result.Error` access without `IsFailure` check
- **FDDD006**: Prevent unsafe `Maybe.Value` access without `HasValue` check
- **FDDD008**: Detect `Result<Result<T>>` double wrapping
- **FDDD009**: Require error parameter for `Maybe.ToResult()`
- **FDDD010**: Prevent blocking on `Task<Result<T>>` with `.Result` or `.Wait()`
- **FDDD012**: Detect `Maybe<Maybe<T>>` double wrapping

**Best Practice Rules (Info):**
- **FDDD002**: Suggest `Bind` instead of `Map` when lambda returns Result
- **FDDD005**: Suggest `MatchError` for type-safe error discrimination
- **FDDD007**: Suggest `Create()` instead of `TryCreate().Value` for clearer intent
- **FDDD011**: Suggest specific error types instead of base `Error` class
- **FDDD013**: Suggest `Result.Combine()` for multiple Result checks
- **FDDD014**: Suggest `GetValueOrDefault`/`Match` instead of ternary operator

**Benefits:**
- ✅ Catch common ROP mistakes at compile time
- ✅ Guide developers toward best practices
- ✅ Improve code quality and maintainability
- ✅ 92 comprehensive tests ensuring accuracy

**Installation:**
```bash
dotnet add package FunctionalDdd.Analyzers
```

**Documentation:** [Analyzer Documentation](Analyzers/src/README.md)

---

## Previous Releases

[Unreleased]: https://github.com/xavierjohn/FunctionalDDD/compare/v1.0.0...HEAD
