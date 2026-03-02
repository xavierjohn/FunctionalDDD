# Trellis — AI API Reference

> **Purpose**: Machine-readable reference for AI coding assistants. Covers every public type, method signature, and usage pattern in the Trellis library ecosystem.

## Quick Facts

- **.NET 10** functional programming library
- **Railway Oriented Programming (ROP)**, DDD primitives, value objects
- Root namespace: `Trellis` for core types, integration packages get their own namespace
- All value objects use `TryCreate` → `Result<T>` and `Create` → `T` (throws) factory pattern
- NuGet packages: `Trellis.Results`, `Trellis.DomainDrivenDesign`, `Trellis.Primitives`, `Trellis.Authorization`, `Trellis.Asp`, `Trellis.Asp.Authorization`, `Trellis.Http`, `Trellis.Mediator`, `Trellis.Testing`, `Trellis.FluentValidation`, `Trellis.Stateless`, `Trellis.EntityFrameworkCore`

---

## Package → Namespace Mapping

| Package | Namespace | Dependency |
|---------|-----------|------------|
| Trellis.Results | `Trellis` | None (core) |
| Trellis.DomainDrivenDesign | `Trellis` | Trellis.Results |
| Trellis.Primitives | `Trellis` (base types), `Trellis.Primitives` (concrete VOs) | Trellis.DomainDrivenDesign |
| Trellis.Authorization | `Trellis.Authorization` | Trellis.Results |
| Trellis.Asp | `Trellis.Asp` | ASP.NET Core |
| Trellis.Asp.Authorization | `Trellis.Asp.Authorization` | Trellis.Authorization, ASP.NET Core |
| Trellis.Http | `Trellis.Http` | Trellis.Results |
| Trellis.Mediator | `Trellis.Mediator` | Mediator, Trellis.Authorization |
| Trellis.Testing | `Trellis.Testing` | FluentAssertions |
| Trellis.FluentValidation | `Trellis.FluentValidation` | FluentValidation |
| Trellis.Stateless | `Trellis.Stateless` | Stateless |
| Trellis.EntityFrameworkCore | `Trellis.EntityFrameworkCore` | EF Core |

---

# 1. Trellis.Results — Core ROP Types

**Namespace: `Trellis`**

## Result\<TValue\> (readonly struct)

Represents success (with value) or failure (with error). Implements `IResult<TValue>`, `IEquatable<Result<TValue>>`, `IFailureFactory<Result<TValue>>`.

### Properties & Methods

```csharp
TValue Value { get; }              // throws if failure
Error Error { get; }               // throws if success
bool IsSuccess { get; }
bool IsFailure { get; }
bool TryGetValue(out TValue value)
bool TryGetError(out Error error)
void Deconstruct(out bool isSuccess, out TValue? value, out Error? error)
```

### Operators

```csharp
implicit operator Result<TValue>(TValue value)   // auto-wrap success
implicit operator Result<TValue>(Error error)     // auto-wrap failure
```

### Static Factories (on `Result`)

```csharp
Result<TValue> Success<TValue>(TValue value)
Result<TValue> Success<TValue>(Func<TValue> funcOk)
Result<Unit> Success()
Result<TValue> Failure<TValue>(Error error)
Result<TValue> Failure<TValue>(Func<Error> error)
Result<Unit> Failure(Error error)
Result<TValue> SuccessIf<TValue>(bool isSuccess, in TValue value, Error error)
Result<(T1, T2)> SuccessIf<T1, T2>(bool isSuccess, in T1 t1, in T2 t2, Error error)
Result<TValue> FailureIf<TValue>(bool isFailure, TValue value, Error error)
Result<TValue> FailureIf<TValue>(Func<bool> failurePredicate, in TValue value, Error error)
Task<Result<TValue>> SuccessIfAsync<TValue>(Func<Task<bool>> predicate, TValue value, Error error)
Task<Result<TValue>> FailureIfAsync<TValue>(Func<Task<bool>> failurePredicate, TValue value, Error error)
Result<T> Try<T>(Func<T> func, Func<Exception, Error>? map = null)
Task<Result<T>> TryAsync<T>(Func<Task<T>> func, Func<Exception, Error>? map = null)
Result<Unit> FromException(Exception ex, Func<Exception, Error>? map = null)
Result<T> FromException<T>(Exception ex, Func<Exception, Error>? map = null)
Result<(T1, T2)> Combine<T1, T2>(Result<T1> r1, Result<T2> r2)
```

## Unit (record struct)

Represents void/no value. Used as `Result<Unit>` for operations that succeed without returning data.

## Maybe\<T\> (readonly struct, where T : notnull)

Domain-level optionality. Use instead of `T?` for optional value objects.

```csharp
T Value { get; }                    // throws if none
bool HasValue { get; }
bool HasNoValue { get; }
T GetValueOrThrow(string? errorMessage = null)
T GetValueOrDefault(T defaultValue)
bool TryGetValue(out T value)
Maybe<TResult> Map<TResult>(Func<T, TResult> selector) where TResult : notnull
TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none)
implicit operator Maybe<T>(T value)
```

### Maybe Static Methods

```csharp
Maybe<T> None<T>() where T : notnull
Maybe<T> From<T>(T? value) where T : notnull
Result<Maybe<TOut>> Optional<TIn, TOut>(TIn? value, Func<TIn, Result<TOut>> function) where TOut : notnull
```

### Maybe Extension Methods

```csharp
// AsMaybe
Maybe<T> AsMaybe<T>(this T? value) where T : struct
Maybe<T> AsMaybe<T>(this T value) where T : class

// AsNullable
T? AsNullable<T>(this Maybe<T> maybe) where T : struct

// ToResult (from Maybe)
Result<T> ToResult<T>(this Maybe<T>, Error) where T : notnull
Result<T> ToResult<T>(this Maybe<T>, Func<Error>) where T : notnull
Task<Result<T>> ToResultAsync<T>(this Task<Maybe<T>>, Error)
ValueTask<Result<T>> ToResultAsync<T>(this ValueTask<Maybe<T>>, Error)
```

---

## Error Hierarchy

### Error (base class)

```csharp
string Code { get; }
string Detail { get; }
string? Instance { get; }
```

### Factory Methods

```csharp
// Default code factories
ValidationError Error.Validation(string fieldDetail, string fieldName = "", string? detail = null, string? instance = null)
ValidationError Error.Validation(ImmutableArray<FieldError> fieldDetails, string detail = "", string? instance = null)
BadRequestError Error.BadRequest(string detail, string? instance = null)
ConflictError Error.Conflict(string detail, string? instance = null)
NotFoundError Error.NotFound(string detail, string? instance = null)
UnauthorizedError Error.Unauthorized(string detail, string? instance = null)
ForbiddenError Error.Forbidden(string detail, string? instance = null)
UnexpectedError Error.Unexpected(string detail, string? instance = null)
DomainError Error.Domain(string detail, string? instance = null)
RateLimitError Error.RateLimit(string detail, string? instance = null)
ServiceUnavailableError Error.ServiceUnavailable(string detail, string? instance = null)

// Custom code factories (same types with additional code parameter)
BadRequestError Error.BadRequest(string detail, string code, string? instance = null)
// ... same pattern for all non-Validation types
```

### Concrete Error Types

| Type | Default Code |
|------|-------------|
| `ValidationError` | `"validation.error"` |
| `BadRequestError` | `"bad.request"` |
| `ConflictError` | `"conflict.error"` |
| `NotFoundError` | `"not.found"` |
| `UnauthorizedError` | `"unauthorized.access"` |
| `ForbiddenError` | `"forbidden.access"` |
| `UnexpectedError` | `"unexpected.error"` |
| `DomainError` | `"domain.error"` |
| `RateLimitError` | `"rate.limit"` |
| `ServiceUnavailableError` | `"service.unavailable"` |

### ValidationError (extends Error)

```csharp
ImmutableArray<FieldError> FieldErrors { get; }
readonly record struct FieldError(string FieldName, ImmutableArray<string> Details)

static ValidationError For(string fieldName, string message, string code = "validation.error", string? detail = null, string? instance = null)
ValidationError And(string fieldName, string message)
ValidationError And(string fieldName, params string[] messages)
ValidationError Merge(ValidationError other)
IDictionary<string, string[]> ToDictionary()
```

### AggregateError (extends Error)

```csharp
IReadOnlyList<Error> Errors { get; }
AggregateError(IReadOnlyList<Error> errors)
AggregateError(IReadOnlyList<Error> errors, string code)
```

---

## Extension Methods — ROP Pipeline Operations

All extension methods follow a consistent async pattern:
- **Sync**: `Method(this Result<T>, ...)` → `Result<TOut>`
- **Task Left-only**: `MethodAsync(this Task<Result<T>>, sync_predicate)` → `Task<Result<TOut>>`
- **Task Right-only**: `MethodAsync(this Result<T>, async_predicate)` → `Task<Result<TOut>>`
- **Task Both**: `MethodAsync(this Task<Result<T>>, async_predicate)` → `Task<Result<TOut>>`
- **ValueTask**: Same three patterns with `ValueTask<Result<T>>`

### Bind — FlatMap / Chain

Transforms value inside Result, function returns `Result<TOut>`. Short-circuits on failure.

```csharp
// Sync
Result<TOut> Bind<TIn, TOut>(this Result<TIn>, Func<TIn, Result<TOut>>)

// Async (all 6 variants)
Task<Result<TOut>> BindAsync<TIn, TOut>(this Task<Result<TIn>>, Func<TIn, Task<Result<TOut>>>)
Task<Result<TOut>> BindAsync<TIn, TOut>(this Task<Result<TIn>>, Func<TIn, Result<TOut>>)
Task<Result<TOut>> BindAsync<TIn, TOut>(this Result<TIn>, Func<TIn, Task<Result<TOut>>>)
ValueTask<Result<TOut>> BindAsync<TIn, TOut>(this ValueTask<Result<TIn>>, Func<TIn, ValueTask<Result<TOut>>>)
ValueTask<Result<TOut>> BindAsync<TIn, TOut>(this ValueTask<Result<TIn>>, Func<TIn, Result<TOut>>)
ValueTask<Result<TOut>> BindAsync<TIn, TOut>(this Result<TIn>, Func<TIn, ValueTask<Result<TOut>>>)
```

### Map — Transform Value

Transforms value, wraps in new Result. Short-circuits on failure.

```csharp
Result<TOut> Map<TIn, TOut>(this Result<TIn>, Func<TIn, TOut>)
// + 6 async variants (same pattern as Bind)
```

### Ensure — Add Validation

Validates value, returns failure if predicate fails. Short-circuits on prior failure.

```csharp
// Bool predicate + static error
Result<T> Ensure<T>(this Result<T>, Func<T, bool> predicate, Error error)
Result<T> Ensure<T>(this Result<T>, Func<bool> predicate, Error error)

// Bool predicate + error factory
Result<T> Ensure<T>(this Result<T>, Func<T, bool> predicate, Func<T, Error> error)

// Result-returning predicate
Result<T> Ensure<T>(this Result<T>, Func<T, Result<T>> predicate)
Result<T> Ensure<T>(this Result<T>, Func<Result<T>> predicate)

// Static helpers
static Result<Unit> Ensure(bool flag, Error error)
static Result<string> EnsureNotNullOrWhiteSpace(this string?, Error error)

// Async: 5 overloads × 6 async patterns (Task Left/Right/Both + ValueTask Left/Right/Both) = 30 variants
```

### Tap — Side Effects on Success

Executes action on success, returns original Result unchanged.

```csharp
Result<T> Tap<T>(this Result<T>, Action)
Result<T> Tap<T>(this Result<T>, Action<T>)
// + 12 async variants (Task and ValueTask with Action, Func<Task>, Func<T,Task>, Func<ValueTask>, Func<T,ValueTask>)
```

### TapOnFailure — Side Effects on Failure

Executes action on failure, returns original Result unchanged.

```csharp
Result<T> TapOnFailure<T>(this Result<T>, Action)
Result<T> TapOnFailure<T>(this Result<T>, Action<Error>)
// + 14 async variants
```

### Match — Terminal Pattern Match

Unwraps Result into a single value by providing both success and failure handlers.

```csharp
TOut Match<TIn, TOut>(this Result<TIn>, Func<TIn, TOut> onSuccess, Func<Error, TOut> onFailure)
void Switch<TIn>(this Result<TIn>, Action<TIn> onSuccess, Action<Error> onFailure)
// + async variants (Task/ValueTask, with CancellationToken overloads)
```

### MatchError — Typed Error Pattern Match

Pattern match on specific error types for fine-grained error handling.

```csharp
TOut MatchError<TIn, TOut>(
    this Result<TIn>,
    Func<TIn, TOut> onSuccess,
    Func<ValidationError, TOut>? onValidation = null,
    Func<NotFoundError, TOut>? onNotFound = null,
    Func<ConflictError, TOut>? onConflict = null,
    Func<BadRequestError, TOut>? onBadRequest = null,
    Func<UnauthorizedError, TOut>? onUnauthorized = null,
    Func<ForbiddenError, TOut>? onForbidden = null,
    Func<DomainError, TOut>? onDomain = null,
    Func<RateLimitError, TOut>? onRateLimit = null,
    Func<ServiceUnavailableError, TOut>? onServiceUnavailable = null,
    Func<UnexpectedError, TOut>? onUnexpected = null,
    Func<Error, TOut>? onError = null)
```

### Combine — Merge Multiple Results

Combines two Results into a tuple Result. If any fails, returns failure.

```csharp
Result<(T1, T2)> Combine<T1, T2>(this Result<T1>, Result<T2>)
Result<T1> Combine<T1>(this Result<T1>, Result<Unit>)  // Unit variant
// + 8 async variants (Task/ValueTask permutations)
// + T4-generated overloads to grow tuples from 2 to 9 elements
```

### MapOnFailure — Transform Error

Transforms the error inside a failed Result, preserves success.

```csharp
Result<T> MapOnFailure<T>(this Result<T>, Func<Error, Error>)
// + 6 async variants
```

### RecoverOnFailure — Recover from Failure

Attempts to recover from a failed Result by providing an alternative.

```csharp
Result<T> RecoverOnFailure<T>(this Result<T>, Func<Result<T>>)
Result<T> RecoverOnFailure<T>(this Result<T>, Func<Error, Result<T>>)
Result<T> RecoverOnFailure<T>(this Result<T>, Func<Error, bool> predicate, Func<Result<T>>)
Result<T> RecoverOnFailure<T>(this Result<T>, Func<Error, bool> predicate, Func<Error, Result<T>>)
// + 22 async variants (Task and ValueTask Left/Right/Both patterns)
```

### When / Unless — Conditional Pipeline

```csharp
Result<T> When<T>(this Result<T>, Func<T, bool> predicate, Func<T, Result<T>> action)
Result<T> When<T>(this Result<T>, bool condition, Func<T, Result<T>> action)
Result<T> Unless<T>(this Result<T>, Func<T, bool> predicate, Func<T, Result<T>> action)
Result<T> Unless<T>(this Result<T>, bool condition, Func<T, Result<T>> action)
// + async variants
```

### Traverse — Apply to Collection

```csharp
Result<IEnumerable<TOut>> Traverse<TIn, TOut>(this IEnumerable<TIn>, Func<TIn, Result<TOut>>)
Task<Result<IEnumerable<TOut>>> TraverseAsync<TIn, TOut>(this IEnumerable<TIn>, Func<TIn, Task<Result<TOut>>>)
// + CancellationToken overloads, ValueTask variants
```

### Nullable → Result

```csharp
Result<T> ToResult<T>(this T? value, Error error) where T : struct
Result<T> ToResult<T>(this T? value, Error error) where T : class
// + Task/ValueTask async variants
```

### ToResult — Wrap as Success

```csharp
Result<T> ToResult<T>(this T value)  // wraps value as Success
```

### LINQ Support

```csharp
Result<TOut> Select<TIn, TOut>(this Result<TIn>, Func<TIn, TOut>)            // = Map
Result<TResult> SelectMany<TSource, TCollection, TResult>(...)                // = Bind+Map
Result<TSource> Where<TSource>(this Result<TSource>, Func<TSource, bool>)     // = Ensure
```

### WhenAll — Parallel Execution

Runs multiple `Task<Result<T>>` in parallel and combines into a tuple result.

```csharp
Task<Result<(T1, T2)>> WhenAllAsync<T1, T2>(Task<Result<T1>>, Task<Result<T2>>)
// ... through 9-tuple arity
```

### ParallelAsync — Launch Parallel Operations

Launches multiple async operations in parallel, returning tuple of tasks.

```csharp
(Task<Result<T1>>, Task<Result<T2>>) ParallelAsync<T1, T2>(Func<Task<Result<T1>>>, Func<Task<Result<T2>>>)
// ... through 9-tuple arity
```

### Tuple Destructuring Extensions (T4-generated, arities 2-9)

All pipeline methods support tuple destructuring for `Result<(T1, T2, ...)>`:

```csharp
// Bind with destructured arguments
Result<TResult> Bind<T1, T2, TResult>(this Result<(T1, T2)>, Func<T1, T2, Result<TResult>>)

// Map with destructured arguments
Result<TOut> Map<T1, T2, TOut>(this Result<(T1, T2)>, Func<T1, T2, TOut>)

// Tap with destructured arguments
Result<(T1, T2)> Tap<T1, T2>(this Result<(T1, T2)>, Action<T1, T2>)

// Match with destructured arguments
TOut Match<T1, T2, TOut>(this Result<(T1, T2)>, Func<T1, T2, TOut>, Func<Error, TOut>)

// Combine growing tuples
Result<(T1, T2, T3)> Combine<T1, T2, T3>(this Result<(T1, T2)>, Result<T3>)
```

Each has sync + Task (3 variants) + ValueTask (3 variants) async overloads.

### Debug — Pipeline Inspection

```csharp
Result<T> Debug<T>(this Result<T>, string? label = null)
Result<T> DebugDetailed<T>(this Result<T>, string? label = null)
Result<T> DebugWithStack<T>(this Result<T>, string? label = null, bool includeStack = true)
Result<T> DebugOnSuccess<T>(this Result<T>, Action<T>)
Result<T> DebugOnFailure<T>(this Result<T>, Action<Error>)
// + async variants
```

---

# 2. Trellis.DomainDrivenDesign — DDD Primitives

**Namespace: `Trellis`**

## Entity\<TId\> (abstract class, where TId : notnull)

Identity-based equality. Two entities are equal iff same type and same non-default ID.

```csharp
public TId Id { get; init; }
protected Entity(TId id)
// Operators: ==, !=
// Overrides: Equals, GetHashCode
```

## Aggregate\<TId\> (abstract class, extends Entity\<TId\>, implements IAggregate)

```csharp
protected Aggregate(TId id)
protected List<IDomainEvent> DomainEvents { get; }
bool IsChanged { get; }                    // true if DomainEvents.Count > 0
IReadOnlyList<IDomainEvent> UncommittedEvents()
void AcceptChanges()                       // clears DomainEvents
```

## IDomainEvent (interface)

```csharp
DateTime OccurredAt { get; }
```

## ValueObject (abstract class)

Structural equality based on `GetEqualityComponents()`. Hash code is cached (immutability assumed).

```csharp
protected abstract IEnumerable<IComparable> GetEqualityComponents()
// Operators: ==, !=, <, <=, >, >=
// Implements: IComparable<ValueObject>, IEquatable<ValueObject>
```

## ScalarValueObject\<TSelf, T\> (abstract class, extends ValueObject)

Single-value wrapper. Constraints: `TSelf : ScalarValueObject<TSelf, T>, IScalarValue<TSelf, T>` and `T : IComparable`.

```csharp
T Value { get; }
protected ScalarValueObject(T value)
static TSelf Create(T value)               // calls TryCreate, throws on failure
implicit operator T(ScalarValueObject<TSelf, T> vo)  // unwrap to primitive
// Implements IConvertible
```

## IScalarValue\<TSelf, TPrimitive\> (interface)

```csharp
static abstract Result<TSelf> TryCreate(TPrimitive value, string? fieldName = null)
static virtual TSelf Create(TPrimitive value)  // default: TryCreate + throw
TPrimitive Value { get; }
```

## Specification\<T\> (abstract class)

Composable business rules that produce `Expression<Func<T, bool>>`.

```csharp
abstract Expression<Func<T, bool>> ToExpression()
bool IsSatisfiedBy(T entity)
Specification<T> And(Specification<T> other)
Specification<T> Or(Specification<T> other)
Specification<T> Not()
implicit operator Expression<Func<T, bool>>(Specification<T> spec)
```

---

# 3. Trellis.Primitives — Base Types & Concrete Value Objects

## Base Types (namespace: `Trellis`)

### RequiredString\<TSelf\>

Inherits `ScalarValueObject<TSelf, string>`. Source generator provides on each `partial class Foo : RequiredString<Foo>`:

```csharp
// Auto-generated
static Result<Foo> TryCreate(string? value, string? fieldName = null)  // rejects null/empty/whitespace, auto-trims
static Foo Create(string? value, string? fieldName = null)
static explicit operator Foo(string value)
// IParsable<Foo>: Parse, TryParse
// [JsonConverter(typeof(ParsableJsonConverter<Foo>))]
```

### RequiredGuid\<TSelf\>

Inherits `ScalarValueObject<TSelf, Guid>`. Source generator provides:

```csharp
static Foo NewUniqueV4()
static Foo NewUniqueV7()
static Result<Foo> TryCreate(Guid value, string? fieldName = null)      // rejects Guid.Empty
static Result<Foo> TryCreate(Guid? value, string? fieldName = null)
static Result<Foo> TryCreate(string? value, string? fieldName = null)   // validates GUID format
static new Foo Create(Guid value)
static Foo Create(string stringValue)
static explicit operator Foo(Guid value)
// IParsable<Foo>: Parse, TryParse
// [JsonConverter(typeof(ParsableJsonConverter<Foo>))]
```

### RequiredInt\<TSelf\>

Inherits `ScalarValueObject<TSelf, int>`. Source generator provides:

```csharp
static Result<Foo> TryCreate(int value, string? fieldName = null)       // rejects zero
static Result<Foo> TryCreate(int? value, string? fieldName = null)
static Result<Foo> TryCreate(string? value, string? fieldName = null)
static new Foo Create(int value)
static Foo Create(string stringValue)
// IParsable<Foo>, explicit operator, JsonConverter
```

### RequiredDecimal\<TSelf\>

Inherits `ScalarValueObject<TSelf, decimal>`. Same pattern as RequiredInt with `decimal`.

### RequiredEnum\<TSelf\>

**NOT a ScalarValueObject** — standalone hierarchy. Smart enum pattern.

```csharp
string Name { get; }      // auto-derived from field name
int Value { get; }         // auto-assigned (0, 1, 2...)

static IReadOnlyCollection<TSelf> GetAll()
static Result<TSelf> TryFromName(string? name, string? fieldName = null)  // case-insensitive
bool Is(params TSelf[] values)
bool IsNot(params TSelf[] values)

// Source-generated:
static Result<Foo> TryCreate(string? value, string? fieldName = null)
// IParsable<Foo>, [JsonConverter(typeof(RequiredEnumJsonConverter<Foo>))]
```

## Concrete Value Objects (namespace: `Trellis.Primitives`)

All have `TryCreate` → `Result<T>` and `Create` → `T` (throws). All implement `IParsable<T>` and have `[JsonConverter]`.

| Type | Primitive | Validation | Extra Members |
|------|-----------|------------|---------------|
| `EmailAddress` | `string` | RFC 5322 regex, case-insensitive, trims | — |
| `PhoneNumber` | `string` | E.164 format (`^\+[1-9]\d{7,14}$`), normalizes | `GetCountryCode()` |
| `Url` | `string` | Valid absolute URI, HTTP/HTTPS only | `Scheme`, `Host`, `Port`, `Path`, `Query`, `IsSecure`, `ToUri()` |
| `Hostname` | `string` | RFC 1123 compliant, ≤255 chars | — |
| `IpAddress` | `string` | `System.Net.IPAddress.TryParse` (v4/v6) | `ToIPAddress()` |
| `Slug` | `string` | Lowercase alphanumeric + hyphens, no consecutive/leading/trailing | — |
| `CountryCode` | `string` | 2 letters, ISO 3166-1 alpha-2, uppercase | — |
| `CurrencyCode` | `string` | 3 letters, ISO 4217, uppercase | — |
| `LanguageCode` | `string` | 2 letters, ISO 639-1, lowercase | — |
| `Age` | `int` | 0–150 inclusive | — |
| `Percentage` | `decimal` | 0–100 inclusive | `Zero`, `Full`, `AsFraction()`, `Of(decimal)`, `FromFraction(decimal)` |
| `Money` | multi-value | Amount ≥ 0, valid currency code | See below |

### Money (extends ValueObject, NOT ScalarValueObject)

Multi-value: `Amount` (decimal) + `Currency` (CurrencyCode). JSON: `{"amount": 99.99, "currency": "USD"}`.

```csharp
decimal Amount { get; }
CurrencyCode Currency { get; }

static Result<Money> TryCreate(decimal amount, string currencyCode, string? fieldName = null)
static Money Create(decimal amount, string currencyCode)
static Result<Money> Zero(string currencyCode = "USD")

// Arithmetic (returns Result — enforces same currency)
Result<Money> Add(Money other)
Result<Money> Subtract(Money other)
Result<Money> Multiply(decimal multiplier)
Result<Money> Multiply(int quantity)
Result<Money> Divide(decimal divisor)
Result<Money> Divide(int divisor)
Result<Money[]> Allocate(params int[] ratios)

// Comparison
bool IsGreaterThan(Money other)
bool IsGreaterThanOrEqual(Money other)
bool IsLessThan(Money other)
bool IsLessThanOrEqual(Money other)
```

---

# 4. Trellis.Authorization

**Namespace: `Trellis.Authorization`**

## Actor (sealed record)

```csharp
Actor(string Id, IReadOnlySet<string> Permissions, IReadOnlySet<string> ForbiddenPermissions, IReadOnlyDictionary<string, string> Attributes)

static Actor Create(string id, IReadOnlySet<string> permissions)
bool HasPermission(string permission)
bool HasPermission(string permission, string scope)     // checks "permission:scope"
bool HasAllPermissions(IEnumerable<string> permissions)
bool HasAnyPermission(IEnumerable<string> permissions)
bool IsOwner(string resourceOwnerId)
bool HasAttribute(string key)
string? GetAttribute(string key)
```

## Interfaces

```csharp
interface IActorProvider { Actor GetCurrentActor(); }
interface IAuthorize { IReadOnlyList<string> RequiredPermissions { get; } }
interface IAuthorizeResource { IResult Authorize(Actor actor); }
```

## ActorAttributes Constants

```csharp
const string TenantId = "tid";
const string PreferredUsername = "preferred_username";
const string AuthorizedParty = "azp";
const string IpAddress = "ip_address";
const string MfaAuthenticated = "mfa";
// ... etc.
```

---

# 5. Trellis.Asp — ASP.NET Core Integration

**Namespace: `Trellis.Asp`**

## Error → HTTP Status Mapping

| Error Type | HTTP Status |
|-----------|-------------|
| `ValidationError` | 400 |
| `BadRequestError` | 400 |
| `UnauthorizedError` | 401 |
| `ForbiddenError` | 403 |
| `NotFoundError` | 404 |
| `ConflictError` | 409 |
| `DomainError` | 422 |
| `RateLimitError` | 429 |
| `UnexpectedError` | 500 |
| `ServiceUnavailableError` | 503 |

Customizable via `TrellisAspOptions.MapError<TError>(int statusCode)`.

## MVC Controller Extensions

```csharp
ActionResult<T> ToActionResult<T>(this Result<T> result, ControllerBase controller)
ActionResult<T> ToCreatedAtActionResult<T>(this Result<T> result, ControllerBase controller,
    string actionName, Func<T, object?> routeValues, string? controllerName = null)
// + async variants for Task<Result<T>> and ValueTask<Result<T>>
// + partial content (206) variant with ContentRangeHeaderValue
```

## Minimal API Extensions

```csharp
IResult ToHttpResult<T>(this Result<T> result, TrellisAspOptions? options = null)
IResult ToCreatedAtRouteHttpResult<T>(this Result<T> result,
    string routeName, Func<T, RouteValueDictionary> routeValues, TrellisAspOptions? options = null)
// + async variants
```

## Registration

```csharp
// MVC — registers model binders, JSON converters, validation filters
builder.Services.AddControllers().AddScalarValueValidation();

// Minimal API
builder.Services.AddScalarValueValidationForMinimalApi();
app.UseScalarValueValidation();  // middleware

// Full setup
builder.Services.AddTrellisAsp();
builder.Services.AddTrellisAsp(options => options.MapError<MyCustomError>(418));
```

---

# 6. Trellis.Asp.Authorization — Entra ID Actor Provider

**Namespace: `Trellis.Asp.Authorization`**

```csharp
// Registration
services.AddEntraActorProvider();
services.AddEntraActorProvider(options => {
    options.IdClaimType = "sub";
    options.MapPermissions = claims => /* custom extraction */;
});

// EntraActorProvider : IActorProvider
// Extracts Actor from HttpContext claims (Entra ID / Azure AD)
```

---

# 7. Trellis.Http — HttpClient → Result Extensions

**Namespace: `Trellis.Http`**

Fluent pipeline for `HttpResponseMessage` → `Result<T>`:

```csharp
// Status handlers (chainable, each returns Result<HttpResponseMessage>)
HandleNotFound(this HttpResponseMessage, NotFoundError)
HandleUnauthorized(this HttpResponseMessage, UnauthorizedError)
HandleForbidden(this HttpResponseMessage, ForbiddenError)
HandleConflict(this HttpResponseMessage, ConflictError)
HandleClientError(this HttpResponseMessage, Func<HttpStatusCode, Error>)
HandleServerError(this HttpResponseMessage, Func<HttpStatusCode, Error>)
EnsureSuccess(this HttpResponseMessage, Func<HttpStatusCode, Error>? errorFactory = null)

// Also chainable on Result<HttpResponseMessage> for fluent error handling
HandleNotFound(this Result<HttpResponseMessage>, NotFoundError)
// ... etc.

// JSON deserialization
Task<Result<T>> ReadResultFromJsonAsync<T>(this HttpResponseMessage, JsonTypeInfo<T>, CancellationToken)
Task<Result<Maybe<T>>> ReadResultMaybeFromJsonAsync<T>(this HttpResponseMessage, JsonTypeInfo<T>, CancellationToken)
// + overloads on Task<HttpResponseMessage>, Result<HttpResponseMessage>, Task<Result<HttpResponseMessage>>
```

### Usage Pattern

```csharp
var result = await httpClient.GetAsync($"/api/orders/{id}")
    .HandleNotFoundAsync(Error.NotFound($"Order {id} not found"))
    .HandleUnauthorizedAsync(Error.Unauthorized("Not authenticated"))
    .EnsureSuccessAsync()
    .ReadResultFromJsonAsync(OrderJsonContext.Default.Order, ct);
```

---

# 8. Trellis.Mediator — CQRS Pipeline Behaviors

**Namespace: `Trellis.Mediator`**

### Pipeline Order

Exception → Tracing → Logging → Authorization → ResourceAuthorization → Validation

### Behaviors

| Behavior | Constraint on TMessage | Purpose |
|----------|----------------------|---------|
| `ExceptionBehavior` | `IMessage` | Catches unhandled exceptions → `Error.Unexpected` |
| `TracingBehavior` | `IMessage` | OpenTelemetry Activity span |
| `LoggingBehavior` | `IMessage` | Structured logging with duration |
| `AuthorizationBehavior` | `IAuthorize, IMessage` | Checks `HasAllPermissions` → `Error.Forbidden` |
| `ResourceAuthorizationBehavior` | `IAuthorizeResource, IMessage` | Delegates to `message.Authorize(actor)` |
| `ValidationBehavior` | `IValidate, IMessage` | Calls `message.Validate()`, short-circuits |

### IValidate Interface

```csharp
interface IValidate { IResult Validate(); }
```

### Registration

```csharp
services.AddTrellisBehaviors();
```

---

# 9. Trellis.Testing — FluentAssertions Extensions

**Namespace: `Trellis.Testing`**

## Result Assertions

```csharp
result.Should().BeSuccess()                              // returns AndWhichConstraint with value
result.Should().BeFailure()                              // returns AndWhichConstraint with Error
result.Should().BeFailureOfType<NotFoundError>()
result.Should().HaveValue(expected)
result.Should().HaveValueMatching(v => v.Name == "test")
result.Should().HaveValueEquivalentTo(expected)
result.Should().HaveErrorCode("not.found")
result.Should().HaveErrorDetail("Order not found")
result.Should().HaveErrorDetailContaining("not found")

// Async
await result.Should().BeSuccessAsync()
await result.Should().BeFailureAsync()
await result.Should().BeFailureOfTypeAsync<ValidationError>()
```

## Maybe Assertions

```csharp
maybe.Should().HaveValue()
maybe.Should().BeNone()
maybe.Should().HaveValueEqualTo(expected)
maybe.Should().HaveValueMatching(v => v > 0)
maybe.Should().HaveValueEquivalentTo(expected)
```

## Error Assertions

```csharp
error.Should().Be(expectedError)
error.Should().HaveCode("validation.error")
error.Should().HaveDetail("Field is required")
error.Should().HaveDetailContaining("required")
error.Should().HaveInstance("/orders/123")
error.Should().BeOfType<ValidationError>()
```

## ValidationError Assertions

```csharp
validationError.Should().HaveFieldError("email")
validationError.Should().HaveFieldErrorWithDetail("email", "Email is required")
validationError.Should().HaveFieldCount(2)
```

## Test Builders

```csharp
// ResultBuilder
ResultBuilder.Success(value)
ResultBuilder.Failure<T>(error)
ResultBuilder.NotFound<T>("Order not found")
ResultBuilder.NotFound<T>("Order", "123")      // "Order '123' not found"
ResultBuilder.Validation<T>("Invalid", "field")
ResultBuilder.Unauthorized<T>()
ResultBuilder.Forbidden<T>()
// ... Conflict, Unexpected, Domain, RateLimit, BadRequest, ServiceUnavailable

// ValidationErrorBuilder
ValidationErrorBuilder.Create()
    .WithFieldError("email", "Required")
    .WithFieldError("name", "Too short", "Too long")
    .Build()           // → ValidationError
    .BuildFailure<T>() // → Result<T>
```

## FakeRepository

```csharp
var repo = new FakeRepository<Order, OrderId>();
await repo.SaveAsync(order);
var result = await repo.GetByIdAsync(orderId);        // Result<Order> (NotFound if missing)
var maybe = await repo.FindByIdAsync(orderId);        // Result<Maybe<Order>>
await repo.DeleteAsync(orderId);
repo.PublishedEvents                                   // IReadOnlyList<IDomainEvent>
```

---

# 10. Trellis.FluentValidation

**Namespace: `Trellis.FluentValidation`**

```csharp
// Convert ValidationResult to Result<T>
Result<T> ToResult<T>(this ValidationResult validationResult, T value)

// Direct validate-and-return
Result<T> ValidateToResult<T>(this IValidator<T> validator, T value)
Task<Result<T>> ValidateToResultAsync<T>(this IValidator<T> validator, T value, CancellationToken ct = default)
```

---

# 11. Trellis.Stateless — State Machine Integration

**Namespace: `Trellis.Stateless`**

```csharp
Result<TState> FireResult<TState, TTrigger>(this StateMachine<TState, TTrigger> stateMachine, TTrigger trigger)
// Success → new state | Invalid transition → Error.Domain with code "state.machine.invalid.transition"
```

---

# 12. Trellis.EntityFrameworkCore

**Namespace: `Trellis.EntityFrameworkCore`**

### DbContext Extensions

```csharp
Task<Result<int>> SaveChangesResultAsync(this DbContext context, CancellationToken ct = default)
Task<Result<Unit>> SaveChangesResultUnitAsync(this DbContext context, CancellationToken ct = default)
// DbUpdateConcurrencyException → Error.Conflict
// Duplicate key → Error.Conflict
// FK violation → Error.Domain
```

### Queryable Extensions

```csharp
Task<Maybe<T>> FirstOrDefaultMaybeAsync<T>(this IQueryable<T> query, CancellationToken ct = default)
Task<Maybe<T>> FirstOrDefaultMaybeAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, CancellationToken ct = default)
Task<Maybe<T>> SingleOrDefaultMaybeAsync<T>(this IQueryable<T> query, CancellationToken ct = default)
Task<Maybe<T>> SingleOrDefaultMaybeAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, CancellationToken ct = default)
Task<Result<T>> FirstOrDefaultResultAsync<T>(this IQueryable<T> query, Error notFoundError, CancellationToken ct = default)
Task<Result<T>> FirstOrDefaultResultAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, Error notFoundError, CancellationToken ct = default)
IQueryable<T> Where<T>(this IQueryable<T> query, Specification<T> specification)
```

### Value Converter Registration

```csharp
// In OnModelCreating or ConfigureConventions
configurationBuilder.ApplyTrellisConventions(typeof(Order).Assembly);
// Auto-registers converters for all IScalarValue and RequiredEnum types
```

### Maybe\<T\> Property Mapping

`Maybe<T>` is a `readonly struct`. EF Core cannot mark non-nullable struct properties as optional — calling `IsRequired(false)` or setting `IsNullable = true` throws `InvalidOperationException`. Use the backing-field pattern with `MaybeProperty`:

```csharp
// Extension method
PropertyBuilder MaybeProperty<TEntity, TInner>(
    this EntityTypeBuilder<TEntity> builder,
    Expression<Func<TEntity, Maybe<TInner>>> propertyExpression)
    where TEntity : class
    where TInner : notnull
// 1. Ignores the Maybe<T> CLR property
// 2. Maps private _camelCase backing field as optional
// 3. Returns PropertyBuilder for chaining (HasMaxLength, etc.)

// Entity — backing field + computed Maybe<T> property
public class Customer
{
    public CustomerId Id { get; set; } = null!;

    private PhoneNumber? _phone;
    public Maybe<PhoneNumber> Phone
    {
        get => _phone is not null ? Maybe.From(_phone) : Maybe.None<PhoneNumber>();
        set => _phone = value.HasValue ? value.Value : null;
    }
}

// OnModelCreating
modelBuilder.Entity<Customer>(b =>
{
    b.HasKey(c => c.Id);
    b.MaybeProperty(c => c.Phone).HasMaxLength(20);
});
```

Backing field naming: `Phone` → `_phone`, `SubmittedAt` → `_submittedAt`, `AlternateEmail` → `_alternateEmail`.

### Maybe\<T\> Queryable Extensions

Because `MaybeProperty` ignores the `Maybe<T>` CLR property, EF Core cannot translate direct LINQ references to it. Use these extension methods instead of raw `EF.Property` calls:

```csharp
// WhereNone — WHERE backing_field IS NULL
IQueryable<TEntity> WhereNone<TEntity, TInner>(
    this IQueryable<TEntity> source,
    Expression<Func<TEntity, Maybe<TInner>>> propertySelector)

// WhereHasValue — WHERE backing_field IS NOT NULL
IQueryable<TEntity> WhereHasValue<TEntity, TInner>(
    this IQueryable<TEntity> source,
    Expression<Func<TEntity, Maybe<TInner>>> propertySelector)

// WhereEquals — WHERE backing_field = @value
IQueryable<TEntity> WhereEquals<TEntity, TInner>(
    this IQueryable<TEntity> source,
    Expression<Func<TEntity, Maybe<TInner>>> propertySelector,
    TInner value)

// Usage
var withoutPhone = await context.Customers.WhereNone(c => c.Phone).ToListAsync(ct);
var withPhone    = await context.Customers.WhereHasValue(c => c.Phone).ToListAsync(ct);
var matches      = await context.Customers.WhereEquals(c => c.Phone, phone).ToListAsync(ct);
```

### Exception Classification

```csharp
bool DbExceptionClassifier.IsDuplicateKey(DbUpdateException ex)       // SQL Server, PostgreSQL, SQLite
bool DbExceptionClassifier.IsForeignKeyViolation(DbUpdateException ex)
string? DbExceptionClassifier.ExtractConstraintDetail(DbUpdateException ex)
```

---

# Known Issues & Workarounds

## Trellis.Unit vs Mediator.Unit

Projects referencing both `Trellis.Results` and `Mediator` will encounter ambiguous `Unit` references. Both libraries define a `Unit` type.

```csharp
// Preferred: Use parameterless Result.Success() — avoids referencing Unit entirely
return Result.Success();  // instead of Result.Success(Unit.Value)

// Alternative: Using alias (if you need to reference Unit directly)
using Unit = Trellis.Unit;
```

---

# Usage Patterns & Recipes

## Create a Custom Value Object (RequiredGuid ID)

```csharp
using Trellis;

public partial class OrderId : RequiredGuid<OrderId> { }

// Usage
var id = OrderId.NewUniqueV7();
var parsed = OrderId.TryCreate("550e8400-e29b-41d4-a716-446655440000");
```

## Create a Custom Value Object (RequiredString)

```csharp
using Trellis;

public partial class FirstName : RequiredString<FirstName> { }

// Usage
var name = FirstName.Create("Alice");
var result = FirstName.TryCreate(userInput);
```

## Create a Custom Value Object (RequiredEnum — Smart Enum)

```csharp
using Trellis;

public partial class OrderStatus : RequiredEnum<OrderStatus>
{
    public static readonly OrderStatus Draft = new();
    public static readonly OrderStatus Pending = new();
    public static readonly OrderStatus Confirmed = new();
    public static readonly OrderStatus Shipped = new();
    public static readonly OrderStatus Delivered = new();
    public static readonly OrderStatus Cancelled = new();
}

// Usage
var status = OrderStatus.Draft;
var all = OrderStatus.GetAll();
var parsed = OrderStatus.TryFromName("Pending");
if (status.Is(OrderStatus.Draft, OrderStatus.Pending)) { /* ... */ }
```

## Create a Custom ScalarValueObject with Custom Validation

```csharp
using Trellis;

public class Temperature : ScalarValueObject<Temperature, decimal>,
    IScalarValue<Temperature, decimal>
{
    private Temperature(decimal value) : base(value) { }

    public static Result<Temperature> TryCreate(decimal value, string? fieldName = null) =>
        value.ToResult()
            .Ensure(v => v >= -273.15m, Error.Validation("Below absolute zero", fieldName ?? "temperature"))
            .Map(v => new Temperature(v));

    // Create is inherited automatically from ScalarValueObject
}
```

## Define an Aggregate

```csharp
using Trellis;

public class Order : Aggregate<OrderId>
{
    private readonly List<OrderLine> _lines = [];
    public CustomerId CustomerId { get; }
    public OrderStatus Status { get; private set; } = OrderStatus.Draft;
    public Money Total { get; private set; }

    private Order(CustomerId customerId) : base(OrderId.NewUniqueV7())
    {
        CustomerId = customerId;
        Total = Money.Create(0m, "USD");
        DomainEvents.Add(new OrderCreated(Id, customerId, DateTime.UtcNow));
    }

    public static Result<Order> TryCreate(CustomerId customerId) =>
        Result.Success(new Order(customerId));

    public Result<Order> AddLine(ProductId productId, string name, Money price, int quantity) =>
        this.ToResult()
            .Ensure(_ => Status == OrderStatus.Draft, Error.Conflict("Cannot modify non-draft order"))
            .Bind(_ => OrderLine.TryCreate(productId, name, price, quantity))
            .Tap(line => _lines.Add(line))
            .Bind(_ => RecalculateTotal())
            .Map(_ => this);

    public Result<Order> Submit() =>
        this.ToResult()
            .Ensure(_ => Status == OrderStatus.Draft, Error.Conflict($"Cannot submit order in {Status} status"))
            .Ensure(_ => _lines.Count > 0, Error.Domain("Cannot submit empty order"))
            .Tap(_ =>
            {
                Status = OrderStatus.Pending;
                DomainEvents.Add(new OrderSubmitted(Id, DateTime.UtcNow));
            })
            .Map(_ => this);

    private Result<Unit> RecalculateTotal() =>
        _lines.Select(l => l.LineTotal)
            .Aggregate(Money.Zero("USD"), (acc, next) => acc.Bind(a => a.Add(next)))
            .Tap(total => Total = total)
            .Map(_ => Unit.Value);
}
```

## Build an ROP Pipeline

```csharp
// Validation + transformation
var result = EmailAddress.TryCreate(dto.Email)
    .Combine(FirstName.TryCreate(dto.FirstName))
    .Combine(LastName.TryCreate(dto.LastName))
    .Bind((email, first, last) => CreateUser(email, first, last));

// Async pipeline with side effects
var result = await OrderId.TryCreate(request.OrderId)
    .BindAsync(id => _repository.GetByIdAsync(id, ct))
    .EnsureAsync(order => order.Status == OrderStatus.Draft, Error.Conflict("Order already submitted"))
    .BindAsync(order => order.Submit())
    .TapAsync(order => _repository.SaveAsync(order, ct))
    .TapAsync(order => _eventBus.PublishAsync(order.UncommittedEvents(), ct));

// Recovery
var result = await ProcessPayment(order, paymentInfo)
    .RecoverOnFailureAsync(
        predicate: err => err is ServiceUnavailableError,
        funcAsync: () => RetryPaymentAsync(order, paymentInfo));
```

## Use Maybe\<T\> for Optional Fields

```csharp
public record CreateProfileRequest(
    string Email,
    string FirstName,
    Maybe<string> MiddleName,    // optional
    string LastName,
    Maybe<Url> Website           // optional value object
);

// Validation with Optional
var result = EmailAddress.TryCreate(dto.Email)
    .Combine(Maybe.Optional(dto.MiddleName.AsNullable(), MiddleName.TryCreate))
    .Bind((email, middleName) => CreateProfile(email, middleName));

// EF Core persistence — use backing-field pattern (see §12 MaybeProperty)
public class Profile
{
    private Url? _website;
    public Maybe<Url> Website
    {
        get => _website is not null ? Maybe.From(_website) : Maybe.None<Url>();
        set => _website = value.HasValue ? value.Value : null;
    }
}
// OnModelCreating: b.MaybeProperty(p => p.Website);
```

## Convert Result to HTTP Response

```csharp
// MVC Controller
[HttpGet("{id}")]
public async Task<ActionResult<OrderDto>> GetOrder(string id)
{
    return await OrderId.TryCreate(id)
        .BindAsync(orderId => _service.GetOrderAsync(orderId))
        .MapAsync(order => order.ToDto())
        .ToActionResultAsync(this);
}

[HttpPost]
public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderRequest request)
{
    return await _service.CreateOrderAsync(request)
        .MapAsync(order => order.ToDto())
        .ToCreatedAtActionResultAsync(this, nameof(GetOrder), dto => new { id = dto.Id });
}

// Minimal API
app.MapGet("/orders/{id}", async (string id, IOrderService service) =>
    await OrderId.TryCreate(id)
        .BindAsync(orderId => service.GetOrderAsync(orderId))
        .MapAsync(order => order.ToDto())
        .ToHttpResultAsync());
```

## HTTP Client → Result Pipeline

```csharp
var result = await _httpClient.GetAsync($"/api/orders/{id}", ct)
    .HandleNotFoundAsync(Error.NotFound($"Order {id} not found"))
    .HandleUnauthorizedAsync(Error.Unauthorized("Authentication required"))
    .EnsureSuccessAsync()
    .ReadResultFromJsonAsync(JsonContext.Default.OrderDto, ct);
```

## EF Core Integration

```csharp
// DbContext configuration
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
{
    configurationBuilder.ApplyTrellisConventions(typeof(Order).Assembly);
}

// Repository
public async Task<Result<Order>> GetByIdAsync(OrderId id, CancellationToken ct) =>
    await _dbContext.Orders
        .FirstOrDefaultResultAsync(o => o.Id == id, Error.NotFound($"Order {id} not found"), ct);

public async Task<Result<Maybe<Order>>> FindByIdAsync(OrderId id, CancellationToken ct) =>
    Result.Success(await _dbContext.Orders.FirstOrDefaultMaybeAsync(o => o.Id == id, ct));

public async Task<Result<Unit>> SaveAsync(Order order, CancellationToken ct)
{
    _dbContext.Orders.Update(order);
    return await _dbContext.SaveChangesResultUnitAsync(ct);
}

// Specification queries
var highValueOrders = await _dbContext.Orders
    .Where(new HighValueOrderSpec(1000m).And(new OrderStatusSpec(OrderStatus.Confirmed)))
    .ToListAsync(ct);
```

## CQRS Command with Authorization

```csharp
using Mediator;
using Trellis;
using Trellis.Authorization;
using Trellis.Mediator;

public sealed record CreateOrderCommand(CustomerId CustomerId, List<OrderLineDto> Items)
    : ICommand<Result<Order>>, IAuthorize, IValidate
{
    // Permission-based authorization
    public IReadOnlyList<string> RequiredPermissions => ["Orders.Create"];

    // Self-validation
    public IResult Validate() =>
        Result.Ensure(Items.Count > 0, Error.Validation("At least one item required", "items"));
}

public sealed class CreateOrderHandler(IOrderRepository repo)
    : ICommandHandler<CreateOrderCommand, Result<Order>>
{
    public async ValueTask<Result<Order>> Handle(CreateOrderCommand command, CancellationToken ct) =>
        await Order.TryCreate(command.CustomerId)
            .BindAsync(order => AddItemsAsync(order, command.Items, ct))
            .BindAsync(order => order.Submit())
            .TapAsync(order => repo.SaveAsync(order, ct));
}
```

## Test Patterns

```csharp
using Trellis.Testing;

[Fact]
public void CreateOrder_ValidInput_ReturnsSuccess()
{
    var customerId = CustomerId.NewUniqueV4();
    var result = Order.TryCreate(customerId);

    result.Should().BeSuccess()
        .Which.CustomerId.Should().Be(customerId);
}

[Fact]
public void CreateOrder_EmptySubmit_ReturnsFailure()
{
    var order = Order.TryCreate(CustomerId.NewUniqueV4()).Value;
    var result = order.Submit();

    result.Should().BeFailure()
        .Which.Should().BeOfType<DomainError>()
        .Which.Should().HaveDetailContaining("empty");
}

[Fact]
public async Task GetOrder_NotFound_ReturnsNotFoundError()
{
    var repo = new FakeRepository<Order, OrderId>();
    var result = await repo.GetByIdAsync(OrderId.NewUniqueV4());

    result.Should().BeFailure()
        .Which.Should().BeOfType<NotFoundError>();
}
```
