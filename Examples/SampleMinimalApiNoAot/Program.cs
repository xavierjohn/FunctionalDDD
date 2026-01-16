// =============================================================================
// Non-AOT Example: Reflection-based Value Object Validation
// =============================================================================
// 
// This example demonstrates automatic value object validation WITHOUT the
// Asp.Generator source generator. It uses reflection to create converters
// at runtime, which works perfectly in JIT-compiled applications.
//
// Key differences from AOT approach:
// - No Asp.Generator reference needed
// - Converters created via reflection at runtime
// - Works in all .NET environments except Native AOT
// - Slightly slower first-request (reflection overhead)
//
// For Native AOT applications, see SampleWebApplication which uses the generator.
// =============================================================================

using FunctionalDdd;
using SampleMinimalApiNoAot;
using SampleUserLibrary;

var builder = WebApplication.CreateBuilder(args);

// Add value object validation services
// This works via reflection - no source generator needed!
builder.Services.AddValueObjectValidation();

var app = builder.Build();

// Enable validation scope per request
app.UseValueObjectValidation();

// =============================================================================
// Example 1: DTO with value objects - automatic validation
// =============================================================================
// The EmailAddress and Name types are validated automatically during JSON
// deserialization using reflection-based converters.

app.MapPost("/users/register", (CreateUserRequest request) =>
User.TryCreate(request.FirstName, request.LastName, request.Email, request.Password ?? "")
    .ToHttpResult())
.WithValueObjectValidation()
.WithName("RegisterUser")
.WithDescription("Register a new user with automatic value object validation");

// =============================================================================
// Example 2: Same type for multiple properties - property names preserved
// =============================================================================
// When the same value object type is used for multiple properties,
// error messages correctly identify each property by its DTO property name.

app.MapPost("/names/validate", (NameTestRequest request) =>
    // Both fname and lname use the Name type
    // Errors will show "fname" and "lname", not "name"
    Results.Ok(new NameTestResponse(request.fname.Value, request.lname.Value)))
    .WithValueObjectValidation()
    .WithName("ValidateNames")
    .WithDescription("Demonstrates property-name-aware validation for same-type properties");

// =============================================================================
// Example 3: Manual validation (for comparison)
// =============================================================================
// This shows the traditional approach without automatic validation.
// Use this for non-JSON content types or when you need explicit control.

app.MapPost("/users/register-manual", (ManualRegisterRequest request) =>
FirstName.TryCreate(request.FirstName)
    .Combine(LastName.TryCreate(request.LastName))
    .Combine(EmailAddress.TryCreate(request.Email))
    .Bind((first, last, email) => User.TryCreate(first, last, email, request.Password ?? ""))
    .ToHttpResult())
.WithName("RegisterUserManual")
.WithDescription("Manual validation approach (for comparison)");

app.Run();
