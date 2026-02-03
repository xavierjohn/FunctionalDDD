# FDDD010: Incorrect async Result usage

## Cause

Accessing `Task<Result<T>>` using blocking calls like `.Result` or `.Wait()` instead of using `await`.

## Rule Description

Blocking on async operations can cause:
- **Deadlocks** in UI applications or ASP.NET contexts
- **Thread pool starvation** under load
- **Poor performance** by blocking threads

When working with `Task<Result<T>>`, always use `await` to get the `Result<T>`.

## How to Fix Violations

Use `await` instead of blocking:

```csharp
// ❌ Bad - Blocking call (can deadlock!)
var result = GetCustomerAsync().Result;

// ✅ Good - Async/await
var result = await GetCustomerAsync();
```

## Examples

### Example 1: Accessing .Result

```csharp
// ❌ Bad
public CustomerDto GetCustomer(Guid id)
{
    var result = customerService.GetCustomerAsync(id).Result;  // Deadlock risk!
    return result.Match(
        onSuccess: c => c.ToDto(),
        onFailure: _ => null);
}

// ✅ Good
public async Task<CustomerDto> GetCustomerAsync(Guid id)
{
    var result = await customerService.GetCustomerAsync(id);
    return result.Match(
        onSuccess: c => c.ToDto(),
        onFailure: _ => null);
}
```

### Example 2: Accessing .Wait()

```csharp
// ❌ Bad
public void ProcessCustomer(Guid id)
{
    var task = customerService.GetCustomerAsync(id);
    task.Wait();  // Deadlock risk!
    var result = task.Result;
    // ...
}

// ✅ Good
public async Task ProcessCustomerAsync(Guid id)
{
    var result = await customerService.GetCustomerAsync(id);
    // ...
}
```

## Async Best Practices with Result

### Use async/await all the way

```csharp
// ✅ Good - Async all the way
public async Task<IActionResult> CreateCustomer(CreateCustomerDto dto)
{
    return await EmailAddress.TryCreate(dto.Email)
        .BindAsync(email => customerService.CreateAsync(email, dto.Name))
        .MapAsync(customer => customer.ToDto())
        .MatchAsync(
            onSuccess: dto => Ok(dto),
            onFailure: error => BadRequest(error));
}
```

### Use ConfigureAwait in library code

```csharp
// ✅ Library code - use ConfigureAwait(false)
public async Task<Result<Customer>> GetCustomerAsync(Guid id)
{
    var result = await repository
        .FindByIdAsync(id)
        .ConfigureAwait(false);
    
    return result.ToResult(
        Error.NotFound($"Customer {id} not found"));
}
```

## When to Suppress Warnings

**Never suppress this warning.** If you absolutely must block (rare cases like `Main` method or console apps), use explicit synchronous methods instead of blocking on async methods.

## Related Rules

None - this is a general async/await best practice.
