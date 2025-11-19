# Railway Oriented Programming

## Structs

### [Result](RailwayOrientedProgramming/src/Result/Result{TValue}.cs)

 Result object holds the result of an operation or `Error`
 It is defined as

```csharp
public readonly struct Result<TValue>
{
    public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException;
    public Error Error => IsFailure ? _error! : throw new InvalidOperationException;

    public bool IsSuccess => !IsFailure;
    public bool IsFailure { get; }

    public static implicit operator Result<TValue>(TValue value) => Result.Success(value);

    public static implicit operator Result<TValue>(Error error) => Result.Failure<TValue>(error);
 }
 ```
 
 ### [Maybe](RailwayOrientedProgramming\src\Maybe\Maybe{T}.cs)

Maybe object holds a value or nothing. It is defined as

```csharp
 public readonly struct Maybe<T> :
    IEquatable<T>,
    IEquatable<Maybe<T>>
    where T : notnull
{
   public T Value;
   public bool HasValue;
   public bool HasNoValue;
}
```

### Functions

### Bind

Bind calls the given function if the result is in success state and return the new result.

### Tap

Tap calls the given function if the result is in success state and returns the same result.

### Compensate

Compensate for failed result by calling the given function. Useful for error recovery and providing fallback values.

#### Basic Compensation

```csharp
// Compensate without accessing the error
Result<User> result = GetUser(userId)
    .Compensate(() => CreateGuestUser());

// Compensate with access to the error
Result<User> result = GetUser(userId)
    .Compensate(error => CreateUserFromError(error));
```

#### Conditional Compensation with Predicate

Compensate only when specific error conditions are met:

```csharp
// Compensate only for NotFound errors (without error parameter)
Result<User> result = GetUser(userId)
    .Compensate(
        predicate: error => error is NotFoundError,
        func: () => CreateDefaultUser()
    );

// Compensate only for NotFound errors (with error parameter for context-aware recovery)
Result<User> result = GetUser(userId)
    .Compensate(
        predicate: error => error is NotFoundError,
        func: error => CreateUserFromError(error)
    );

// Compensate based on error code
Result<Data> result = FetchData(id)
    .Compensate(
        predicate: error => error.Code == "not.found.error",
        func: () => GetCachedData(id)
    );

// Compensate for multiple error types
Result<Config> result = LoadConfig()
    .Compensate(
        predicate: error => error is NotFoundError or UnauthorizedError,
        func: () => GetDefaultConfig()
    );
```

All Compensate methods have async variants (`CompensateAsync`) for working with `Task<Result<T>>`.

### Ensure

 Ensure calls the given function if the result is in success state.
 If the function returns false, the attached error is returned.

### Map

 Map calls the given function if the result is in success state.
 The return value is wrapped in `Result` as success.

### Combine

 Combine combines multiple `Result` objects. It will return a `Result` with all the errors if any of the `Result` objects have failed.
 If all the errors are of type `ValidationError`, then it will return a `ValidationError` with all the errors. 
 Otherwise it will return an `AggregatedError`

### ParallelAsync

 Parallel runs multiple tasks in parallel and returns multiple tasks. `AwaitAsync` will await all the task and return the combined result.

### Finally

 Finally unwraps the `Result` and returns the success value or the error.

### Maybe

 Maybe states if it contains a value or not.
 It has the following methods:

- HasValue - returns true if it has a value.
- HasNoValue - returns true if it does not have a value.
- Value - returns the value if it has a value. Otherwise `InvalidOperationException`

## LINQ Query Syntax

You can use C# query expressions with `Result` via `Select`, `SelectMany`, and `Where`:

```csharp
var total = from a in Result.Success(2) from b in Result.Success(3) from c in Result.Success(5) select a + b + c;          // Success(10)
var filtered = from x in Result.Success(5) where x > 10               // predicate false -> failure (UnexpectedError) select x;
```

`where` uses an `Unexpected` error if the predicate fails. For domain-specific errors prefer `Ensure`.

## Error Transformation (MapError)

```csharp
Result<int> r = GetUserPoints(userId);
var apiResult = r.MapError(err => Error.NotFound("User not found")); // Success passes through unchanged; failure error replaced.
```

## Pattern Matching (Match / Switch)
```csharp
var description = r.Match( ok  => $"Points: {ok}", err => $"Error: {err.Code}");
await r.MatchAsync( async ok  => { await LogAsync(ok); return Unit.Value; }, async err => { await LogErrorAsync(err); return Unit.Value; });
```

## Exception Capture (Try / TryAsync)
Add these patterns to keep chains free from explicit try/catch and error plumbing.

```csharp
var loaded = Result.Try(() => LoadFromDisk(path));          // captures exceptions
var loadedAsync = await Result.TryAsync(() => FetchAsync()); // async variant
```