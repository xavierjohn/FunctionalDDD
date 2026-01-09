# Migration Guide: v2.x → v3.0

## Breaking Changes Summary

FunctionalDDD v3.0 introduces clearer naming for failure track operations to make Railway-Oriented Programming more explicit and easier to learn. All **failure track operations** now have an `OnFailure` suffix.

**Success track operations remain unchanged** - this is NOT a complete rewrite, just clearer naming for error handling.

---

## Renamed Operations

### Failure Track Operations (Breaking Changes)

| v2.x Method | v3.0 Method | Track | Find & Replace |
|-------------|-------------|-------|----------------|
| `TapError` | **`TapOnFailure`** | 🔴 Failure | `.TapError(` → `.TapOnFailure(` |
| `TapErrorAsync` | **`TapOnFailureAsync`** | 🔴 Failure | `.TapErrorAsync(` → `.TapOnFailureAsync(` |
| `MapError` | **`MapOnFailure`** | 🔴 Failure | `.MapError(` → `.MapOnFailure(` |
| `MapErrorAsync` | **`MapOnFailureAsync`** | 🔴 Failure | `.MapErrorAsync(` → `.MapOnFailureAsync(` |
| `Compensate` | **`RecoverOnFailure`** | 🔴 Failure | `.Compensate(` → `.RecoverOnFailure(` |
| `CompensateAsync` | **`RecoverOnFailureAsync`** | 🔴 Failure | `.CompensateAsync(` → `.RecoverOnFailureAsync(` |

### Success Track Operations (No Changes) ✅

These methods are **unchanged** - no migration needed:

- `Bind`, `BindAsync` - Chain operations that can fail
- `Map`, `MapAsync` - Transform success values
- `Tap`, `TapAsync` - Execute side effects on success
- `Ensure`, `EnsureAsync` - Validate conditions (can switch tracks)
- `When`, `WhenAsync`, `Unless`, `UnlessAsync` - Conditional execution

### Universal/Terminal Operations (No Changes) ✅

These methods are **unchanged**:

- `Combine` - Merge multiple results
- `Match`, `MatchAsync` - Pattern match success/failure
- `MatchError` - Pattern match specific error types
- `ToResult`, `ToResultAsync` - Convert nullables to Result

---

## Why This Change?

### Problem: Track Behavior Wasn't Obvious

```csharp
// v2.x - Which track do these run on?
.Tap(user => Log(user))          // Success? Not obvious
.TapError(err => LogError(err))  // Failure? "Error" hints at it
.Map(user => user.Name)          // Success? Not obvious
.MapError(err => AddContext(err)) // Failure? "Error" hints at it
.Compensate(err => GetDefault()) // Failure? Not obvious at all
```

### Solution: Explicit `OnFailure` Suffix

```csharp
// v3.0 - Crystal clear track indicators
.Tap(user => Log(user))                    // 🟢 Success (no suffix)
.TapOnFailure(err => LogError(err))       // 🔴 Failure (OnFailure suffix)
.Map(user => user.Name)                   // 🟢 Success (no suffix)
.MapOnFailure(err => AddContext(err))     // 🔴 Failure (OnFailure suffix)
.RecoverOnFailure(err => GetDefault())    // 🔴 Failure (OnFailure suffix)
```

**Pattern:**
- **Success track** = No suffix
- **Failure track** = `OnFailure` suffix

---

## Automated Migration

### Visual Studio / Rider

1. **Edit** → **Find and Replace** → **Replace in Files**
2. **Match case:** ✅ Enabled
3. **Match whole word:** ✅ Enabled  
4. **Use regular expressions:** ❌ Disabled

Apply these replacements **in order**:

```
Find: .TapError(
Replace: .TapOnFailure(

Find: .TapErrorAsync(
Replace: .TapOnFailureAsync(

Find: .MapError(
Replace: .MapOnFailure(

Find: .MapErrorAsync(
Replace: .MapOnFailureAsync(

Find: .Compensate(
Replace: .RecoverOnFailure(

Find: .CompensateAsync(
Replace: .RecoverOnFailureAsync(
```

### VS Code

1. **Edit** → **Find in Files** (Ctrl+Shift+F / Cmd+Shift+F)
2. Enable **Match Case** (Aa button)
3. Enable **Match Whole Word** (Ab| button)
4. Apply replacements from table above

### Command Line (PowerShell)

```powershell
# Navigate to your solution directory
cd C:\MyProject

# Replace TapError → TapOnFailure
Get-ChildItem -Recurse -Include *.cs | ForEach-Object {
    (Get-Content $_) -replace '\.TapError\(', '.TapOnFailure(' | Set-Content $_
}

# Replace TapErrorAsync → TapOnFailureAsync  
Get-ChildItem -Recurse -Include *.cs | ForEach-Object {
    (Get-Content $_) -replace '\.TapErrorAsync\(', '.TapOnFailureAsync(' | Set-Content $_
}

# Replace MapError → MapOnFailure
Get-ChildItem -Recurse -Include *.cs | ForEach-Object {
    (Get-Content $_) -replace '\.MapError\(', '.MapOnFailure(' | Set-Content $_
}

# Replace MapErrorAsync → MapOnFailureAsync
Get-ChildItem -Recurse -Include *.cs | ForEach-Object {
    (Get-Content $_) -replace '\.MapErrorAsync\(', '.MapOnFailureAsync(' | Set-Content $_
}

# Replace Compensate → RecoverOnFailure
Get-ChildItem -Recurse -Include *.cs | ForEach-Object {
    (Get-Content $_) -replace '\.Compensate\(', '.RecoverOnFailure(' | Set-Content $_
}

# Replace CompensateAsync → RecoverOnFailureAsync
Get-ChildItem -Recurse -Include *.cs | ForEach-Object {
    (Get-Content $_) -replace '\.CompensateAsync\(', '.RecoverOnFailureAsync(' | Set-Content $_
}
```

---

## Migration Examples

### Example 1: Simple Error Logging

#### Before (v2.x)
```csharp
public async Task<IActionResult> GetUser(string id)
{
    return await UserId.TryCreate(id)
        .BindAsync(GetUserAsync)
        .TapError(err => _logger.LogWarning("Failed to get user: {Error}", err))
        .Match(
            onSuccess: user => Ok(user),
            onFailure: error => NotFound(error.Detail)
        );
}
```

#### After (v3.0)
```csharp
public async Task<IActionResult> GetUser(string id)
{
    return await UserId.TryCreate(id)
        .BindAsync(GetUserAsync)
        .TapOnFailure(err => _logger.LogWarning("Failed to get user: {Error}", err)) // ✅ Changed
        .Match(
            onSuccess: user => Ok(user),
            onFailure: error => NotFound(error.Detail)
        );
}
```

### Example 2: Error Recovery with Fallback

#### Before (v2.x)
```csharp
public async Task<Result<User>> GetUserWithFallback(UserId userId)
{
    return await GetUserFromCache(userId)
        .Compensate(() => GetUserFromDatabase(userId))
        .Compensate(() => GetGuestUser())
        .TapError(err => _metrics.RecordFailure("user.get", err.Code));
}
```

#### After (v3.0)
```csharp
public async Task<Result<User>> GetUserWithFallback(UserId userId)
{
    return await GetUserFromCache(userId)
        .RecoverOnFailure(() => GetUserFromDatabase(userId))        // ✅ Changed
        .RecoverOnFailure(() => GetGuestUser())                     // ✅ Changed
        .TapOnFailure(err => _metrics.RecordFailure("user.get", err.Code)); // ✅ Changed
}
```

### Example 3: Complex Error Handling Pipeline

#### Before (v2.x)
```csharp
public async Task<IActionResult> ProcessOrder(CreateOrderRequest request)
{
    return await CustomerId.TryCreate(request.CustomerId)
        .Combine(ProductId.TryCreate(request.ProductId))
        .Combine(Quantity.TryCreate(request.Quantity))
        
        .BindAsync((customerId, productId, qty) => 
            CreateOrderAsync(customerId, productId, qty))
        .Tap(order => _logger.LogInformation("Order created: {OrderId}", order.Id))
        .TapError(err => _logger.LogWarning("Order creation failed: {Error}", err))
        
        .EnsureAsync(order => HasInventoryAsync(order.ProductId, order.Quantity),
            Error.Conflict("Insufficient inventory"))
        .TapError(err => _metrics.RecordFailure("order.create", err.Code))
        
        .Compensate(err => err is ConflictError 
            ? SuggestAlternativeProductsAsync(request.ProductId)
            : Result.Failure<Order>(err))
        
        .MapError(err => Error.Domain($"Order processing failed: {err.Detail}"))
        
        .TapAsync(order => SaveOrderAsync(order))
        .TapAsync(order => PublishOrderCreatedEventAsync(order))
        
        .Match(
            onSuccess: order => Created($"/orders/{order.Id}", order),
            onFailure: error => error.ToHttpResult()
        );
}
```

#### After (v3.0)
```csharp
public async Task<IActionResult> ProcessOrder(CreateOrderRequest request)
{
    return await CustomerId.TryCreate(request.CustomerId)
        .Combine(ProductId.TryCreate(request.ProductId))
        .Combine(Quantity.TryCreate(request.Quantity))
        
        .BindAsync((customerId, productId, qty) => 
            CreateOrderAsync(customerId, productId, qty))
        .Tap(order => _logger.LogInformation("Order created: {OrderId}", order.Id))
        .TapOnFailure(err => _logger.LogWarning("Order creation failed: {Error}", err)) // ✅ Changed
        
        .EnsureAsync(order => HasInventoryAsync(order.ProductId, order.Quantity),
            Error.Conflict("Insufficient inventory"))
        .TapOnFailure(err => _metrics.RecordFailure("order.create", err.Code)) // ✅ Changed
        
        .RecoverOnFailure(err => err is ConflictError                           // ✅ Changed
            ? SuggestAlternativeProductsAsync(request.ProductId)
            : Result.Failure<Order>(err))
        
        .MapOnFailure(err => Error.Domain($"Order processing failed: {err.Detail}")) // ✅ Changed
        
        .TapAsync(order => SaveOrderAsync(order))
        .TapAsync(order => PublishOrderCreatedEventAsync(order))
        
        .Match(
            onSuccess: order => Created($"/orders/{order.Id}", order),
            onFailure: error => error.ToHttpResult()
        );
}
```

---

## Testing Migration

### Update Test Methods

Test method names should also be updated for clarity:

#### Before (v2.x)
```csharp
[Fact]
public void TapError_WithAction_FailureResult_ExecutesAction()
{
    var result = Result.Failure<int>(Error.Unexpected("Error"));
    
    var actual = result.TapError(() => _actionExecuted = true);
    
    _actionExecuted.Should().BeTrue();
}
```

#### After (v3.0)
```csharp
[Fact]
public void TapOnFailure_WithAction_FailureResult_ExecutesAction()  // ✅ Test name changed
{
    var result = Result.Failure<int>(Error.Unexpected("Error"));
    
    var actual = result.TapOnFailure(() => _actionExecuted = true);  // ✅ Method changed
    
    _actionExecuted.Should().BeTrue();
}
```

---

## Validation After Migration

### Compile Your Solution

```bash
dotnet build
```

All compile errors will point to missed renames. The compiler is your friend!

### Common Compile Errors

```
error CS1061: 'Result<User>' does not contain a definition for 'TapError'
```

**Fix:** Replace with `TapOnFailure`

```
error CS1061: 'Result<Order>' does not contain a definition for 'Compensate'  
```

**Fix:** Replace with `RecoverOnFailure`

### Run Your Tests

```bash
dotnet test
```

If tests fail, check for:
- Test method names referencing old operation names
- Assertions checking for old method behavior

---

## Benefits of v3.0 Naming

### 1. Self-Documenting Code

```csharp
// Track behavior is obvious from method names
.Bind(...)              // Runs on success
.TapOnFailure(...)      // Runs on failure - explicit!
.RecoverOnFailure(...)  // Recovery on failure - clear!
```

### 2. Easier to Learn

New developers can understand track behavior **without reading documentation**.

### 3. IDE Support

The new `[RailwayTrack]` attribute enables future IDE tooling:
- Inline hints showing track behavior
- Code analysis and suggestions
- Better IntelliSense grouping

### 4. Consistent Pattern

**Rule:** Failure track = `OnFailure` suffix, Success track = no suffix

Easy to remember, easy to teach.

---

## Rollback Plan

If you need to temporarily roll back to v2.x:

```bash
# Downgrade to last v2.x version
dotnet remove package FunctionalDDD.RailwayOrientedProgramming
dotnet add package FunctionalDDD.RailwayOrientedProgramming --version 2.9.0
```

Then revert your code changes using source control:

```bash
git checkout main -- .
```

---

## Getting Help

- **Documentation:** [https://xavierjohn.github.io/FunctionalDDD/](https://xavierjohn.github.io/FunctionalDDD/)
- **Issues:** [https://github.com/xavierjohn/FunctionalDDD/issues](https://github.com/xavierjohn/FunctionalDDD/issues)
- **Discussions:** [https://github.com/xavierjohn/FunctionalDDD/discussions](https://github.com/xavierjohn/FunctionalDDD/discussions)

---

## Summary Checklist

- [ ] Update NuGet package to v3.0
- [ ] Run find & replace for all 6 renamed methods
- [ ] Compile solution and fix any errors
- [ ] Update test method names
- [ ] Run all tests
- [ ] Update any documentation/comments in your code
- [ ] Commit changes with message: "Migrate to FunctionalDDD v3.0"

**Estimated migration time:** 5-15 minutes for most projects (depending on size)
