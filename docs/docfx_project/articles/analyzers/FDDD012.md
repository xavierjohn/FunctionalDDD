# FDDD012: Consider using Result.Combine

## Cause

Manually checking `IsSuccess` on multiple `Result<T>` values when `Result.Combine()` or `.Combine()` chaining provides a cleaner alternative.

## Rule Description

When you need to validate multiple inputs and proceed only if all are successful, `Combine` provides a declarative approach that:
- Automatically collects all validation errors
- Returns success only if all inputs are successful
- Reduces boilerplate code

Two equivalent syntaxes are available:

| Syntax | Best when |
|--------|----------|
| `Result.Combine(r1, r2, r3)` | You already have results in separate variables |
| `r1.Combine(r2).Combine(r3)` | You want to chain inline expressions |

## How to Fix Violations

Replace manual checks with `Result.Combine()`:

```csharp
// ❌ Verbose - Manual checks
var emailResult = EmailAddress.TryCreate(dto.Email);
var phoneResult = PhoneNumber.TryCreate(dto.Phone);

if (emailResult.IsFailure)
    return emailResult.Error;
if (phoneResult.IsFailure)
    return phoneResult.Error;

var customer = Customer.Create(emailResult.Value, phoneResult.Value);

// ✅ Option A - Static Combine (when you have variables)
var emailResult = EmailAddress.TryCreate(dto.Email);
var phoneResult = PhoneNumber.TryCreate(dto.Phone);

return Result.Combine(emailResult, phoneResult)
    .Bind((email, phone) => Customer.Create(email, phone));

// ✅ Option B - Combine chaining (inline)
return EmailAddress.TryCreate(dto.Email)
    .Combine(PhoneNumber.TryCreate(dto.Phone))
    .Bind((email, phone) => Customer.Create(email, phone));
```

## Examples

### Example 1: Combining 2 Results

```csharp
public Result<Customer> CreateCustomer(CreateCustomerDto dto)
{
    return Result.Combine(
            EmailAddress.TryCreate(dto.Email),
            Name.TryCreate(dto.Name))
        .Map((email, name) => new Customer(email, name));
}
```

### Example 2: Combining 3 Results

```csharp
public Result<Address> CreateAddress(AddressDto dto)
{
    return Result.Combine(
            Street.TryCreate(dto.Street),
            City.TryCreate(dto.City),
            PostalCode.TryCreate(dto.PostalCode))
        .Map((street, city, postalCode) => 
            new Address(street, city, postalCode));
}
```

### Example 3: Combining up to 9 Results

```csharp
// Both styles support up to 9-element tuples
return Result.Combine(r1, r2, r3, r4, r5, r6, r7, r8, r9)
    .Map((v1, v2, v3, v4, v5, v6, v7, v8, v9) => 
        CreateEntity(v1, v2, v3, v4, v5, v6, v7, v8, v9));
```

## Benefits

### Collects All Errors

```csharp
// Manual approach - Returns first error only
var emailResult = EmailAddress.TryCreate(invalidEmail);
if (emailResult.IsFailure)
    return emailResult.Error;  // Returns immediately

var phoneResult = PhoneNumber.TryCreate(invalidPhone);
if (phoneResult.IsFailure)
    return phoneResult.Error;  // Never reached if email failed

// Result.Combine - Collects all errors
Result.Combine(
    EmailAddress.TryCreate(invalidEmail),
    PhoneNumber.TryCreate(invalidPhone))
// Returns both errors if both fail!
```

### Declarative Intent

```csharp
// ✅ Clear intent - "combine these validations"
Result.Combine(
    ValidateEmail(dto.Email),
    ValidateAge(dto.Age),
    ValidateAddress(dto.Address))
```

## When to Use Manual Checks

Use manual checks when:
- You need short-circuit behavior (stop at first error)
- You have conditional validation logic
- The results are not independent

```csharp
// Manual checks appropriate here - conditional logic
var userResult = GetUser(userId);
if (userResult.IsFailure)
    return userResult.Error;

// Only validate permissions if user exists
var permissionResult = ValidatePermissions(userResult.Value);
if (permissionResult.IsFailure)
    return permissionResult.Error;
```

## When to Suppress Warnings

This is a suggestion-level diagnostic. Suppress it if:
- You need short-circuit behavior
- You prefer explicit control flow
- You're validating a variable number of items

## Related Rules

None - this is a suggestion to improve code clarity.
