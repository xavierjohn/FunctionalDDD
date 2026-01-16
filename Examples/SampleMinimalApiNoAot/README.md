# Non-AOT Example: Reflection-based Value Object Validation

This example demonstrates automatic value object validation **without** the `Asp.Generator` source generator.

## When to Use This Approach

✅ **Use this approach when:**
- You don't need Native AOT compilation
- You want simpler setup (no generator reference needed)
- You're okay with reflection at runtime
- You're building a standard JIT-compiled .NET application

❌ **Don't use this approach when:**
- You need Native AOT compilation
- You need assembly trimming
- You want faster startup (no reflection overhead)

For AOT-compatible applications, see `SampleWebApplication` which uses the source generator.

## Setup

No special setup needed! Just reference `FunctionalDDD.Asp`:

```xml
<ProjectReference Include="..\..\Asp\src\Asp.csproj" />
<!-- Note: Asp.Generator is NOT referenced -->
```

Then configure in `Program.cs`:

```csharp
builder.Services.AddValueObjectValidation();
app.UseValueObjectValidation();
```

## How It Works

When the source generator is not present:

1. `ValidatingJsonConverterFactory` detects types implementing `ITryCreatable<T>` using reflection
2. Converters are created at runtime via `Activator.CreateInstance`
3. `PropertyNameAwareConverterFactory.CreateWithReflection` wraps converters for property-name awareness

This happens transparently - your application code is identical to the AOT approach.

## Example Endpoints

### POST /users/register
```json
{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "password": "secret123"
}
```

### POST /names/validate
```json
{
    "fname": "",
    "lname": ""
}
```

Response (400 Bad Request):
```json
{
    "errors": {
        "fname": ["Name cannot be empty."],
        "lname": ["Name cannot be empty."]
    }
}
```

## Performance Comparison

| Metric | Without Generator | With Generator |
|--------|-------------------|----------------|
| First request | ~50μs slower (reflection) | Fastest |
| Subsequent requests | Same | Same |
| Memory at startup | Slightly higher | Lower |
| AOT compatible | ❌ No | ✅ Yes |

For most applications, the reflection overhead is negligible.

## Running the Example

```bash
cd Examples/SampleMinimalApiNoAot
dotnet run
```

Then test with:
```bash
curl -X POST http://localhost:5000/users/register \
  -H "Content-Type: application/json" \
  -d '{"firstName":"","lastName":"","email":"invalid","password":"test"}'
```
