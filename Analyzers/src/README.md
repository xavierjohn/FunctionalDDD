# FunctionalDDD.Analyzers

Roslyn analyzers for the FunctionalDDD library. These analyzers help enforce Railway Oriented Programming best practices and prevent common mistakes with `Result<T>` and `Maybe<T>` types at compile time.

## Why Use Analyzers?

Writing Railway Oriented Programming code correctly requires discipline. It's easy to:
- Forget to handle a `Result<T>` return value
- Access `.Value` without checking `IsSuccess` 
- Use `Map` instead of `Bind` when the lambda returns a `Result`
- Block on `Task<Result<T>>` instead of awaiting

**FunctionalDDD.Analyzers** catches these mistakes at compile time with **14 diagnostic rules** that guide you toward best practices.

## Installation

### NuGet Package Manager

```bash
dotnet add package FunctionalDdd.Analyzers
```

### Package Reference

Add to your `.csproj` file:

```xml
<PackageReference Include="FunctionalDdd.Analyzers" Version="*" PrivateAssets="all" />
```

**Important:** The `PrivateAssets="all"` attribute ensures the analyzer is only used during compilation and not included in your application's output or referenced by consuming projects.

### Visual Studio

1. Right-click on your project in Solution Explorer
2. Select **Manage NuGet Packages**
3. Search for **FunctionalDdd.Analyzers**
4. Click **Install**

### Verify Installation

After installation, you should see the analyzers in Visual Studio:

**Solution Explorer** → Your Project → **Dependencies** → **Analyzers** → **FunctionalDdd.Analyzers**

## Configuration

### Enable/Disable Specific Rules

You can configure analyzer severity in your `.editorconfig` file:

```ini
# Disable a specific rule
dotnet_diagnostic.FDDD001.severity = none

# Change severity level
dotnet_diagnostic.FDDD002.severity = warning  # Default: info
dotnet_diagnostic.FDDD014.severity = none     # Turn off ternary suggestions

# Enable all rules as warnings
dotnet_diagnostic.FDDD001.severity = warning
dotnet_diagnostic.FDDD002.severity = warning
# ... etc
```

### Global Suppression

To suppress all FunctionalDDD analyzer warnings in a file:

```csharp
#pragma warning disable FDDD001, FDDD002, FDDD003, FDDD004, FDDD005, FDDD006, FDDD007, FDDD008, FDDD009, FDDD010, FDDD011, FDDD012, FDDD013, FDDD014
// Your code here
#pragma warning restore FDDD001, FDDD002, FDDD003, FDDD004, FDDD005, FDDD006, FDDD007, FDDD008, FDDD009, FDDD010, FDDD011, FDDD012, FDDD013, FDDD014
```

## Diagnostic Rules

| ID | Severity | Description |
|----|----------|-------------|
| FDDD001 | Warning | Result return value is not handled |
| FDDD002 | Info | Use Bind instead of Map when lambda returns Result |
| FDDD003 | Warning | Unsafe access to Result.Value |
| FDDD004 | Warning | Unsafe access to Result.Error |
| FDDD005 | Info | Consider using MatchError for error type discrimination |
| FDDD006 | Warning | Unsafe access to Maybe.Value |
| FDDD007 | Info | Use Create instead of TryCreate().Value |
| FDDD008 | Warning | Result is double-wrapped |
| FDDD010 | Warning | Incorrect async Result usage |
| FDDD012 | Warning | Maybe is double-wrapped |

---

## FDDD001: Result return value is not handled

This analyzer warns when a method returning `Result<T>` is called but the return value is discarded. This can lead to silent error handling issues.

```csharp
// Warning: Result not handled
UserService.CreateUser(name, email);

// OK: Result is handled
var result = UserService.CreateUser(name, email);

// OK: Result is chained
UserService.CreateUser(name, email)
    .Bind(user => SendWelcomeEmail(user));
```

## FDDD002: Use Bind instead of Map

This analyzer suggests using `Bind` instead of `Map` when the lambda returns a `Result<T>`. Using `Map` in this case would produce `Result<Result<T>>` which is likely not intended.

```csharp
// Info: Use Bind instead of Map
result.Map(user => ValidateUser(user)); // Returns Result<Result<User>>

// OK: Use Bind for Result-returning lambdas
result.Bind(user => ValidateUser(user)); // Returns Result<User>
```

## FDDD003: Unsafe access to Result.Value

This analyzer warns when accessing `Result<T>.Value` without first checking `IsSuccess`. Accessing `Value` on a failed result throws an exception.

```csharp
// Warning: Unsafe access
var user = result.Value;

// OK: Guarded access
if (result.IsSuccess)
{
    var user = result.Value;
}

// OK: Using TryGetValue
if (result.TryGetValue(out var user))
{
    // use user
}

// OK: Using Match
result.Match(
    onSuccess: user => HandleUser(user),
    onFailure: error => HandleError(error));
```

## FDDD004: Unsafe access to Result.Error

Similar to FDDD003, this analyzer warns when accessing `Result<T>.Error` without first checking `IsFailure`.

```csharp
// Warning: Unsafe access
var error = result.Error;

// OK: Guarded access
if (result.IsFailure)
{
    var error = result.Error;
}
```

## FDDD006: Unsafe access to Maybe.Value

This analyzer warns when accessing `Maybe<T>.Value` without first checking `HasValue`.

```csharp
// Warning: Unsafe access
var value = maybe.Value;

// OK: Guarded access
if (maybe.HasValue)
{
    var value = maybe.Value;
}

// OK: Using GetValueOrDefault
var value = maybe.GetValueOrDefault(defaultValue);

// OK: Using TryGetValue
if (maybe.TryGetValue(out var value))
{
    // use value
}

// OK: Converting to Result
maybe.ToResult(Error.NotFound("Not found"))
    .Bind(value => ProcessValue(value));
```

## FDDD007: Use Create instead of TryCreate().Value

This analyzer detects when `.Value` is accessed directly on a `TryCreate()` result for scalar value objects implementing `IScalarValue<TSelf, TPrimitive>`. This pattern is unclear and defeats the purpose of using `TryCreate`. Both `TryCreate().Value` and `Create()` throw the same exception on invalid input, but `Create()` shows clearer intent.

```csharp
// Info: Unclear usage (for types implementing IScalarValue)
var email = EmailAddress.TryCreate("test@example.com").Value;

// OK: Use Create when you expect the value to be valid
var email = EmailAddress.Create("test@example.com");

// OK: Or properly handle the Result
var result = EmailAddress.TryCreate(userInput);
if (result.IsFailure)
    return result.ToHttpResult();
var email = result.Value;
```

**Note:** This analyzer only applies to scalar value objects that implement `IScalarValue<TSelf, TPrimitive>`, which guarantees both `TryCreate` and `Create` methods exist with the documented behavior.

## FDDD008: Result is double-wrapped

This analyzer detects when a `Result` is wrapped inside another `Result`, creating `Result<Result<T>>`. This is almost always unintended and indicates misuse of `Map` instead of `Bind`, or unnecessary wrapping of an existing Result.

```csharp
// Warning: Double wrapping in type declaration
Result<Result<User>> user;
public Result<Result<Order>> GetOrder() { }

// Warning: Wrapping an existing Result
Result<int> existingResult = GetValue();
var wrapped = Result.Success(existingResult); // Creates Result<Result<int>>

// OK: Single wrapping
Result<User> user;
public Result<Order> GetOrder() { }
var result = Result.Success(42); // Result<int>

// OK: Use Bind for Result-returning functions (see FDDD002)
result.Bind(x => ValidateUser(x)); // Returns Result<User>, not Result<Result<User>>
```

**Common causes:**
1. Using `Map` instead of `Bind` when the lambda returns a `Result` (also caught by FDDD002)
2. Calling `Result.Success()` or `Result.Failure()` on a value that's already a `Result`
3. Declaring variables, properties, or return types with `Result<Result<T>>`

## FDDD009: Maybe.ToResult called without error parameter

This analyzer detects when you convert a `Maybe<T>` to `Result<T>` without providing an error for the None case. Without an error parameter, it's unclear what error should be returned when the Maybe has no value.

```csharp
// Warning: What error for None case?
Maybe<User> maybeUser = GetUser();
var result = maybeUser.ToResult(); // Missing error parameter

// OK: Error provided for None case
var result = maybeUser.ToResult(Error.NotFound("User not found"));

// OK: Handle None case explicitly
if (maybeUser.HasNoValue)
    return Error.NotFound("User not found");
var user = maybeUser.Value;
```

**Why provide an error:**
- Makes None handling explicit and intentional
- Provides meaningful error messages to callers
- Prevents silent failures
- Documents what "None" means in your domain

## FDDD010: Incorrect async Result usage

This analyzer detects when you block on `Task<Result<T>>` or `ValueTask<Result<T>>` using `.Result` or `.Wait()` instead of properly awaiting. Blocking can cause deadlocks and prevents proper async execution.

```csharp
// Warning: Blocking on async Result
Task<Result<User>> userTask = GetUserAsync();
var result = userTask.Result; // Deadlock risk!
userTask.Wait(); // Also blocks

// OK: Await properly
var result = await GetUserAsync();
```

**Why this is dangerous:**
- Can cause deadlocks in UI apps and some server contexts
- Blocks threads unnecessarily
- Defeats the purpose of async/await
- May hide exceptions differently than await

## FDDD012: Maybe is double-wrapped

This analyzer detects when a `Maybe` is wrapped inside another `Maybe`, creating `Maybe<Maybe<T>>`. This is almost always unintended.

```csharp
// Warning: Double wrapping
Maybe<Maybe<User>> user;
public Maybe<Maybe<Order>> GetOrder() { }

// OK: Single wrapping
Maybe<User> user;
public Maybe<Order> GetOrder() { }

// OK: Convert to Result for better composability
Maybe<User> maybeUser = GetUser();
Result<User> userResult = maybeUser.ToResult(Error.NotFound("User not found"));
```

**Common causes:**
1. Using `Map` when the transformation function returns a `Maybe`
2. Declaring variables, properties, or return types with `Maybe<Maybe<T>>`
3. Consider converting to `Result` with `ToResult()` for operations that need error handling

## FDDD011: Use specific error type instead of base Error class

This analyzer detects when you instantiate the base `Error` class directly instead of using specific error types like `ValidationError`, `NotFoundError`, etc.

```csharp
// Info: Use specific error type
var error = new Error("Something went wrong");

// OK: Use factory methods
var validation = Error.Validation("Invalid input");
var notFound = Error.NotFound("User not found");

// OK: Use specific error types directly
var validationError = new ValidationError("Invalid email");
var notFoundError = new NotFoundError("Order not found");
```

**Why use specific error types:**
- Enables type-safe error handling with `MatchError`
- Makes error categories explicit and searchable
- Supports better logging and monitoring
- Allows middleware to map errors to appropriate HTTP status codes

## FDDD014: Consider using GetValueOrDefault or Match

This analyzer detects the ternary pattern `result.IsSuccess ? result.Value : defaultValue` and suggests using functional methods instead.

```csharp
// Info: Consider functional approach
var user = userResult.IsSuccess ? userResult.Value : null;
var count = countResult.IsSuccess ? countResult.Value : 0;

// OK: Use GetValueOrDefault
var user = userResult.GetValueOrDefault(null);
var count = countResult.GetValueOrDefault(0);

// OK: Use Match for different logic
var userName = userResult.Match(
    onSuccess: u => u.Name,
    onFailure: _ => "Unknown");
```

**Benefits of functional methods:**
- More idiomatic Railway Oriented Programming style
- Clearer intent (GetValueOrDefault vs manual ternary)
- Safer - no risk of accessing `.Value` on failure
- Better for chaining with other ROP methods
- Match allows different transformations for success/failure

## FDDD013: Consider using Result.Combine

This analyzer detects manual combination of multiple `Result.IsSuccess` checks and suggests using `Result.Combine()` for cleaner code.

```csharp
// Info: Manual combination
Result<int> result1 = GetFirst();
Result<int> result2 = GetSecond();
Result<int> result3 = GetThird();

if (result1.IsSuccess && result2.IsSuccess && result3.IsSuccess)
{
    var combined = (result1.Value, result2.Value, result3.Value);
}

// OK: Use Result.Combine
var combined = Result.Combine(result1, result2, result3);
combined.Bind(tuple => ProcessValues(tuple.Item1, tuple.Item2, tuple.Item3));
```

**Benefits of Result.Combine:**
- **Cleaner code**: No manual `IsSuccess` checks
- **Safer**: No risk of accessing `.Value` on failed results
- **Automatic error handling**: Returns the first error encountered
- **Better composability**: Result of Combine is itself a Result, ready for Bind/Map
- **Scales better**: Works with 2-9 Results without becoming unwieldy

## Suppressing Diagnostics

### Per-Instance Suppression

If you need to suppress a diagnostic for a specific case:

```csharp
#pragma warning disable FDDD001
SomeMethodReturningResult();  // Intentionally ignored
#pragma warning restore FDDD001
```

### Method-Level Suppression

Use the `[SuppressMessage]` attribute:

```csharp
using System.Diagnostics.CodeAnalysis;

[SuppressMessage("FunctionalDDD", "FDDD001", 
    Justification = "Fire-and-forget operation")]
public void MyMethod()
{
    ProcessAsync();  // Result intentionally not awaited
}
```

### File-Level Suppression

At the top of a file:

```csharp
#pragma warning disable FDDD003, FDDD004
// Entire file ignores unsafe Value/Error access warnings
```

### Project-Level Configuration

In your `.editorconfig`:

```ini
# Turn off info-level suggestions in test projects
[*Tests.cs]
dotnet_diagnostic.FDDD002.severity = none
dotnet_diagnostic.FDDD005.severity = none
dotnet_diagnostic.FDDD013.severity = none
dotnet_diagnostic.FDDD014.severity = none

# Make safety warnings errors in production code
[src/**.cs]
dotnet_diagnostic.FDDD001.severity = error
dotnet_diagnostic.FDDD003.severity = error
dotnet_diagnostic.FDDD006.severity = error
dotnet_diagnostic.FDDD010.severity = error
```

---

## IDE Integration

### Visual Studio

Analyzers appear in the **Error List** window and provide:
- ✅ Inline squiggly underlines
- ✅ Quick info tooltips on hover
- ✅ Error list filtering by severity

**Tip:** Press `Ctrl+.` on a diagnostic to see available actions.

### Visual Studio Code

Requires the **C# Dev Kit** extension:

1. Install [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
2. Analyzers work automatically after package installation
3. View diagnostics in the **Problems** panel (`Ctrl+Shift+M`)

### JetBrains Rider

Analyzers are automatically detected:

1. After package installation, rebuild the project
2. Diagnostics appear in the **Solution-Wide Analysis** window
3. Configure severity in **Settings** → **Editor** → **Inspection Severity**

---

## FAQ

### Q: Do I need the FunctionalDDD.RailwayOrientedProgramming package?

**A:** Yes! The analyzers detect patterns related to `Result<T>`, `Maybe<T>`, and `Error` types from the core ROP package.

```bash
# Install both packages
dotnet add package FunctionalDDD.RailwayOrientedProgramming
dotnet add package FunctionalDDD.Analyzers
```

### Q: Can I use these analyzers in my library/package?

**A:** Absolutely! The analyzers help maintain high code quality. Make sure to set `PrivateAssets="all"` so consumers don't inherit the analyzer:

```xml
<PackageReference Include="FunctionalDdd.Analyzers" Version="*" PrivateAssets="all" />
```

### Q: The analyzers are too strict! Can I relax them?

**A:** Yes! You can:
1. Configure severity levels in `.editorconfig` (see [Configuration](#configuration))
2. Suppress specific instances with `#pragma` directives
3. Turn off info-level rules (FDDD002, FDDD005, FDDD007, FDDD011, FDDD013, FDDD014)

We recommend keeping warning-level rules enabled for safety.

### Q: Do the analyzers impact build performance?

**A:** Minimal impact. Analyzers run during compilation, typically adding <1 second to build time.

### Q: Can I contribute new analyzer rules?

**A:** Yes! See the [Contributing](../../CONTRIBUTING.md) guide. We welcome suggestions for new rules that enforce ROP best practices.

---

## Troubleshooting

### Analyzers Not Appearing

1. **Verify installation:**
   - Check **Solution Explorer** → **Dependencies** → **Analyzers**
   - Look for **FunctionalDdd.Analyzers**

2. **Restart Visual Studio/Rider:**
   - Analyzers may require an IDE restart after installation

3. **Clean and rebuild:**
   ```bash
   dotnet clean
   dotnet build
   ```

4. **Check .NET SDK version:**
   - Requires .NET SDK 6.0 or later
   - Run `dotnet --version` to verify

### False Positives

If you encounter a false positive diagnostic:

1. Check if your code pattern is actually unsafe
2. Review the diagnostic documentation (above)
3. If it's a genuine false positive, [file an issue](https://github.com/xavierjohn/FunctionalDDD/issues)

### Performance Issues

If builds become slow:

1. Check if you have many analyzer violations (fix them!)
2. Configure `.editorconfig` to disable info-level rules in large files
3. Use `<NoWarn>` for generated code:
   ```xml
   <NoWarn>FDDD001;FDDD002;FDDD003</NoWarn>
   ```

---

## Learn More

- 📖 [FunctionalDDD Documentation](https://xavierjohn.github.io/FunctionalDDD/)
- 🎓 [Railway Oriented Programming Introduction](https://xavierjohn.github.io/FunctionalDDD/articles/intro.html)
- 💡 [Common ROP Patterns](https://xavierjohn.github.io/FunctionalDDD/articles/patterns.html)
- 🐛 [Report Issues](https://github.com/xavierjohn/FunctionalDDD/issues)

---

## License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.
