# TRLS019: Combine chain exceeds maximum supported tuple size

## Cause

A chain of `.Combine()` calls produces a tuple with more than 9 elements, which exceeds the maximum supported by downstream methods like `Bind`, `Map`, `Tap`, and `Match`.

## Rule Description

`Combine` aggregates multiple `Result<T>` values into a single result containing a tuple. Trellis supports tuples up to 9 elements. When a `Combine` chain exceeds this limit, downstream operations cannot destructure the result.

This rule fires as an **Error** because the code will fail to compile when you attempt to use `Bind`, `Map`, or other methods on the over-sized result.

## How to Fix Violations

Group related validations into intermediate value objects or sub-results, then combine those groups:

```csharp
// ❌ Bad - 10 elements exceeds the maximum of 9
var result = FirstName.TryCreate(request.FirstName)
    .Combine(LastName.TryCreate(request.LastName))
    .Combine(EmailAddress.TryCreate(request.Email))
    .Combine(PhoneNumber.TryCreate(request.Phone))
    .Combine(Age.TryCreate(request.Age))
    .Combine(Street.TryCreate(request.Street))
    .Combine(City.TryCreate(request.City))
    .Combine(State.TryCreate(request.State))
    .Combine(ZipCode.TryCreate(request.Zip))
    .Combine(CountryCode.TryCreate(request.Country)); // 10th element!

// ✅ Good - Group related fields into sub-objects
var personalInfo = FirstName.TryCreate(request.FirstName)
    .Combine(LastName.TryCreate(request.LastName))
    .Combine(EmailAddress.TryCreate(request.Email))
    .Combine(PhoneNumber.TryCreate(request.Phone))
    .Combine(Age.TryCreate(request.Age));

var address = Street.TryCreate(request.Street)
    .Combine(City.TryCreate(request.City))
    .Combine(State.TryCreate(request.State))
    .Combine(ZipCode.TryCreate(request.Zip))
    .Combine(CountryCode.TryCreate(request.Country));

var result = personalInfo
    .Combine(address)
    .Bind((personal, addr) => User.TryCreate(personal, addr));
```

## Examples

### Example 1: Order with Many Fields

```csharp
// ❌ Bad - Too many fields in one chain
var order = ProductId.TryCreate(req.ProductId)
    .Combine(Quantity.TryCreate(req.Quantity))
    .Combine(Price.TryCreate(req.Price))
    .Combine(CurrencyCode.TryCreate(req.Currency))
    .Combine(FirstName.TryCreate(req.ShipFirstName))
    .Combine(LastName.TryCreate(req.ShipLastName))
    .Combine(Street.TryCreate(req.ShipStreet))
    .Combine(City.TryCreate(req.ShipCity))
    .Combine(ZipCode.TryCreate(req.ShipZip))
    .Combine(CountryCode.TryCreate(req.ShipCountry)); // 10 elements

// ✅ Good - Separate into logical groups
var lineItem = ProductId.TryCreate(req.ProductId)
    .Combine(Quantity.TryCreate(req.Quantity))
    .Combine(Money.TryCreate(req.Price, req.Currency));

var shippingAddress = FirstName.TryCreate(req.ShipFirstName)
    .Combine(LastName.TryCreate(req.ShipLastName))
    .Combine(Street.TryCreate(req.ShipStreet))
    .Combine(City.TryCreate(req.ShipCity))
    .Combine(ZipCode.TryCreate(req.ShipZip))
    .Combine(CountryCode.TryCreate(req.ShipCountry));

var order = lineItem
    .Combine(shippingAddress)
    .Bind((item, address) => Order.TryCreate(item, address));
```

### Example 2: Boundary Case (9 Elements Is Valid)

```csharp
// ✅ OK - 9 elements is the maximum supported
var result = Field1.TryCreate(a)
    .Combine(Field2.TryCreate(b))
    .Combine(Field3.TryCreate(c))
    .Combine(Field4.TryCreate(d))
    .Combine(Field5.TryCreate(e))
    .Combine(Field6.TryCreate(f))
    .Combine(Field7.TryCreate(g))
    .Combine(Field8.TryCreate(h))
    .Combine(Field9.TryCreate(i)); // 9 elements - OK
```

## Related Rules

- [TRLS012](TRLS012.md) - Consider using Result.Combine
- [TRLS008](TRLS008.md) - Result is double-wrapped

## See Also

- [Basics - Combining Results](../basics.md) - How Combine works
- [Clean Architecture](../clean-architecture.md) - Grouping validations into value objects
