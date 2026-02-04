# FDDD018: Unsafe access to Value in LINQ expression

## Cause

Accessing `.Value` on `Result<T>` or `Maybe<T>` inside a LINQ expression like `Select`, `ToDictionary`, `GroupBy`, etc. without first filtering by `IsSuccess` or `HasValue`.

## Rule Description

When you have a collection of `Result<T>` or `Maybe<T>` values and use LINQ to project them, accessing `.Value` without filtering first can throw exceptions for failed results or empty maybes.

## How to Fix Violations

Filter the collection before projecting, or use safe extraction methods:

```csharp
// ❌ Bad - May throw for failed Results
var values = results.Select(r => r.Value);

// ✅ Good - Filter first
var values = results.Where(r => r.IsSuccess).Select(r => r.Value);

// ✅ Also good - Use Match
var values = results.Select(r => r.Match(
    onSuccess: v => v,
    onFailure: _ => default));
```

## Examples

### Example 1: Select on Results

```csharp
List<Result<Customer>> customerResults = await GetCustomersAsync(ids);

// ❌ Bad - Throws if any result is a failure
var names = customerResults.Select(r => r.Value.Name);

// ✅ Good - Filter first
var names = customerResults
    .Where(r => r.IsSuccess)
    .Select(r => r.Value.Name);

// ✅ Also good - Get successful results only
var successfulCustomers = customerResults
    .Where(r => r.IsSuccess)
    .Select(r => r.Value)
    .ToList();
```

### Example 2: Select on Maybe Values

```csharp
List<Maybe<User>> maybeUsers = GetOptionalUsers(ids);

// ❌ Bad - Throws if any Maybe is empty
var emails = maybeUsers.Select(m => m.Value.Email);

// ✅ Good - Filter first
var emails = maybeUsers
    .Where(m => m.HasValue)
    .Select(m => m.Value.Email);

// ✅ Also good - Use SelectMany pattern
var emails = maybeUsers
    .SelectMany(m => m.HasValue ? new[] { m.Value.Email } : Array.Empty<string>());
```

### Example 3: ToDictionary

```csharp
List<Result<(Guid Id, string Name)>> results = GetResults();

// ❌ Bad - Both key and value selectors are unsafe
var dict = results.ToDictionary(r => r.Value.Id, r => r.Value.Name);

// ✅ Good - Filter first
var dict = results
    .Where(r => r.IsSuccess)
    .ToDictionary(r => r.Value.Id, r => r.Value.Name);
```

### Example 4: GroupBy

```csharp
List<Result<Order>> orderResults = await GetOrdersAsync();

// ❌ Bad - Accessing Value in key selector without filter
var byStatus = orderResults.GroupBy(r => r.Value.Status);

// ✅ Good - Filter first
var byStatus = orderResults
    .Where(r => r.IsSuccess)
    .GroupBy(r => r.Value.Status);
```

## Handling Both Success and Failure

If you need to handle both successful and failed results:

```csharp
var results = await ProcessItemsAsync(items);

// Separate successes and failures
var successes = results.Where(r => r.IsSuccess).Select(r => r.Value);
var failures = results.Where(r => r.IsFailure).Select(r => r.Error);

// Or use Match for transformation
var processed = results.Select(r => r.Match(
    onSuccess: value => new ProcessedItem(value, null),
    onFailure: error => new ProcessedItem(null, error.Message)));
```

## LINQ Methods That Trigger This Rule

- `Select`
- `SelectMany`
- `ToDictionary`
- `ToLookup`
- `GroupBy`
- `OrderBy` / `OrderByDescending`
- `ThenBy` / `ThenByDescending`

## When This Rule Doesn't Apply

The analyzer is smart enough to not report when you've already filtered:

```csharp
// ✅ No warning - Where clause filters by IsSuccess
var values = results.Where(r => r.IsSuccess).Select(r => r.Value);

// ✅ No warning - Where clause filters by HasValue
var values = maybes.Where(m => m.HasValue).Select(m => m.Value);
```

## Related Rules

- [FDDD003](FDDD003.md) - Unsafe access to Result.Value
- [FDDD006](FDDD006.md) - Unsafe access to Maybe.Value

## See Also

- [LINQ Best Practices](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/)
