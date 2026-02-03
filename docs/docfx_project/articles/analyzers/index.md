# FunctionalDDD Analyzers

The FunctionalDDD analyzers help you write safer, more functional code by detecting common mistakes and suggesting best practices for Railway Oriented Programming.

## Installation

```bash
dotnet add package FunctionalDdd.Analyzers
```

The analyzer package is automatically included when you install any FunctionalDDD package.

## Analyzer Rules

### Error Handling (FDDD001-FDDD006)

| ID | Title | Severity | Has Code Fix |
|----|-------|----------|--------------|
| [FDDD001](FDDD001.md) | Result return value is not handled | Warning | ❌ |
| [FDDD002](FDDD002.md) | Use Bind instead of Map when lambda returns Result | Info | ✅ |
| [FDDD003](FDDD003.md) | Unsafe access to Result.Value | Warning | ✅ |
| [FDDD004](FDDD004.md) | Unsafe access to Result.Error | Warning | ✅ |
| [FDDD005](FDDD005.md) | Consider using MatchError for error type discrimination | Info | ❌ |
| [FDDD006](FDDD006.md) | Unsafe access to Maybe.Value | Warning | ✅ |

### Value Objects (FDDD007)

| ID | Title | Severity | Has Code Fix |
|----|-------|----------|--------------|
| [FDDD007](FDDD007.md) | Use Create instead of TryCreate().Value | Warning | ✅ |

### Type Safety (FDDD008-FDDD012)

| ID | Title | Severity | Has Code Fix |
|----|-------|----------|--------------|
| [FDDD008](FDDD008.md) | Result is double-wrapped | Warning | ❌ |
| [FDDD009](FDDD009.md) | Maybe.ToResult called without error parameter | Warning | ❌ |
| [FDDD010](FDDD010.md) | Incorrect async Result usage | Warning | ❌ |
| [FDDD011](FDDD011.md) | Use specific error type instead of base Error class | Info | ❌ |
| [FDDD012](FDDD012.md) | Maybe is double-wrapped | Warning | ❌ |

### Code Quality (FDDD013-FDDD014)

| ID | Title | Severity | Has Code Fix |
|----|-------|----------|--------------|
| [FDDD013](FDDD013.md) | Consider using Result.Combine | Info | ❌ |
| [FDDD014](FDDD014.md) | Consider using GetValueOrDefault or Match | Info | ❌ |

## Severity Levels

- **Warning**: Potential runtime errors or correctness issues that should be fixed
- **Info**: Suggestions for more idiomatic or maintainable code

## Code Fixes

Several analyzers provide automatic code fixes (✅ in the table above):

- **FDDD002**: Automatically replaces `Map` with `Bind`
- **FDDD003**: Wraps unsafe `result.Value` access in `if (result.IsSuccess)` guard
- **FDDD004**: Wraps unsafe `result.Error` access in `if (result.IsFailure)` guard
- **FDDD006**: Wraps unsafe `maybe.Value` access in `if (maybe.HasValue)` guard
- **FDDD007**: Replaces `TryCreate().Value` with `Create()`

## Configuration

### Disabling Specific Rules

You can disable specific rules in your `.editorconfig`:

```ini
# Disable FDDD003 for test projects
[**Tests/**/*.cs]
dotnet_diagnostic.FDDD003.severity = none

# Change FDDD007 to suggestion only
dotnet_diagnostic.FDDD007.severity = suggestion
```

### Suppressing Warnings

Use `#pragma` directives for local suppression:

```csharp
#pragma warning disable FDDD003
var customer = result.Value; // Intentionally unsafe in test
#pragma warning restore FDDD003
```

Or use `[SuppressMessage]` attribute:

```csharp
[SuppressMessage("FunctionalDDD", "FDDD003:Unsafe access to Result.Value",
    Justification = "Test code - validating success scenario")]
public void TestMethod()
{
    var customer = result.Value;
}
```

## Common Patterns

### API Controller Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateCustomer(CreateCustomerDto dto)
    {
        return await Result.Combine(
                EmailAddress.TryCreate(dto.Email),
                Name.TryCreate(dto.Name))
            .BindAsync((email, name) => 
                customerService.CreateAsync(email, name))
            .MapAsync(customer => customer.ToDto())
            .MatchAsync(
                onSuccess: dto => Ok(dto),
                onFailure: error => error.ToHttpResult());
    }
}
```

This pattern avoids all analyzer warnings!

## Feedback

Found a false positive or have suggestions? [Open an issue on GitHub](https://github.com/xavierjohn/FunctionalDDD/issues).
