# Railway Oriented Programming

## Structs

### [Result](RailwayOrientedProgramming/src/Result/Result{TValue}.cs)

 Result object holds the result of an operation or `Error`
 It is defined as

```csharp
public readonly struct Result<TValue>
{
    public TValue Value => IsFailure ? throw new ResultFailureException(Error) : _value!;
    public Error Error => _error ?? throw new ResultSuccessException();

    public bool IsFailure { get; }
    public bool IsSuccess => !IsFailure;

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

### Tee

Tee calls the given function if the result is in success state and returns the same result.

### Compensate

 Compensate for failed result by calling the given function.

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

 Parallel runs multiple tasks in parallel and returns multiple tasks. `BindAsync` will await all the task and call the given function.

### Finally

 Finally unwraps the `Result` and returns the success value or the error.

 ### Maybe

 Maybe states if it contains a value or not.
 It has the following methods:

- HasValue - returns true if it has a value.
- HasNoValue - returns true if it does not have a value.
- Value - returns the value if it has a value. Otherwise `InvalidOperationException`