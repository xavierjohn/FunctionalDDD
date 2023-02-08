# Railway Oriented Programming

## Structs

### [Result](RailwayOrientedProgramming/src/Result/Result{TOk,TErr}.cs)

 Result object holds the result of an operation. It can be either `Ok` or `Error`.
 It is defined as

```csharp
 public readonly struct Result<TOk, TErr>
{
    public TOk Ok => IsError ? throw new ResultFailureException<TErr>(Error) : _ok!;
    public TErr Error => _error ?? throw new ResultSuccessException();

    public bool IsError { get; }
    public bool IsOk => !IsError;
    
    public static implicit operator Result<TOk, TErr>(TOk value) => Result.Success<TOk, TErr>(value);

    public static implicit operator Result<TOk, TErr>(TErr errors) => Result.Failure<TOk, TErr>(errors);
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

### OnOk

OnOk calls the given function if the result is in `Ok` state.

### OnError

 OnError calls the given function if the result is in `Error` state.
 It given an oppertunity to compensate for the error and return Okay.

### Ensure

 Ensure calls the given function if the result is in `Ok` state.
 If the function returns false, the attached error is returned.

### Map

 Map calls the given function if the result is in `Ok` state.
 The return value is wrapped in `Result` as Ok.

### Combine

 Combine combines multiple `Result` objects. It will return a `Result` with all the errors if any of the `Result` objects have failed.
 If all the errors are of type `ValidationError`, then it will return a `ValidationError` with all the errors. 
 Otherwise it will return an `AggregatedError`

### ParallelAsync

 Parallel runs multiple tasks in parallel and returns multiple tasks. `IfOkAsync` will await all the task and call the given function.

### Unwarp

 Unwrap unwraps the `Result` and returns the `Ok` value or the error.

### Maybe

 Maybe states if it contains a value or not.
 It has the following methods:

- HasValue - returns true if it has a value.
- HasNoValue - returns true if it does not have a value.
- Value - returns the value if it has a value. Otherwise `InvalidOperationException`