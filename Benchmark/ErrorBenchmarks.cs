namespace Benchmark;

using BenchmarkDotNet.Attributes;
using FunctionalDdd;
using static FunctionalDdd.ValidationError;

/// <summary>
/// Benchmarks for Error type testing error creation, combining, and type checking.
/// Error handling is critical in railway-oriented programming for failures.
/// </summary>
[MemoryDiagnoser]
public class ErrorBenchmarks
{
    private Error _validationError = default!;
    private Error _notFoundError = default!;
    private ValidationError _complexValidationError = default!;

    [GlobalSetup]
    public void Setup()
    {
        _validationError = Error.Validation("Test validation error", "field1");
        _notFoundError = Error.NotFound("Resource not found", "resource123");
        _complexValidationError = ValidationError.For("email", "Email is required")
            .And("password", "Password is required")
            .And("age", "Age must be 18 or older");
    }

    [Benchmark(Baseline = true)]
    public Error CreateValidationError_Simple()
    {
        return Error.Validation("Field is required", "fieldName");
    }

    [Benchmark]
    public Error CreateNotFoundError()
    {
        return Error.NotFound("Resource not found", "id123");
    }

    [Benchmark]
    public Error CreateUnauthorizedError()
    {
        return Error.Unauthorized("Access denied");
    }

    [Benchmark]
    public Error CreateConflictError()
    {
        return Error.Conflict("Resource already exists", "duplicate-id");
    }

    [Benchmark]
    public Error CreateUnexpectedError()
    {
        return Error.Unexpected("Internal server error");
    }

    [Benchmark]
    public ValidationError CreateValidationError_MultipleFields()
    {
        return ValidationError.For("field1", "Error 1")
            .And("field2", "Error 2")
            .And("field3", "Error 3");
    }

    [Benchmark]
    public ValidationError CreateValidationError_SingleFieldMultipleMessages()
    {
        return ValidationError.For("password", "Too short")
            .And("password", "No special characters")
            .And("password", "No numbers");
    }

    [Benchmark]
    public Error CombineErrors_TwoValidationErrors()
    {
        var error1 = Error.Validation("Email required", "email");
        var error2 = Error.Validation("Password required", "password");
        return error1.Combine(error2);
    }

    [Benchmark]
    public Error CombineErrors_ValidationAndNotFound()
    {
        var error1 = Error.Validation("Invalid input");
        var error2 = Error.NotFound("Resource not found");
        return error1.Combine(error2);
    }

    [Benchmark]
    public Error CombineErrors_ThreeValidationErrors()
    {
        var error1 = Error.Validation("Error 1", "field1");
        var error2 = Error.Validation("Error 2", "field2");
        var error3 = Error.Validation("Error 3", "field3");
        return error1.Combine(error2).Combine(error3);
    }

    [Benchmark]
    public Error CombineErrors_FiveErrors()
    {
        var error1 = Error.Validation("Error 1", "field1");
        var error2 = Error.Validation("Error 2", "field2");
        var error3 = Error.NotFound("Not found");
        var error4 = Error.Unauthorized("Unauthorized");
        var error5 = Error.Validation("Error 5", "field5");
        return error1
            .Combine(error2)
            .Combine(error3)
            .Combine(error4)
            .Combine(error5);
    }

    [Benchmark]
    public ValidationError MergeValidationErrors_DifferentFields()
    {
        var error1 = ValidationError.For("field1", "Error 1");
        var error2 = ValidationError.For("field2", "Error 2");
        return error1.Merge(error2);
    }

    [Benchmark]
    public ValidationError MergeValidationErrors_SameField()
    {
        var error1 = ValidationError.For("password", "Too short");
        var error2 = ValidationError.For("password", "No special chars");
        return error1.Merge(error2);
    }

    [Benchmark]
    public ValidationError MergeValidationErrors_Complex()
    {
        var error1 = ValidationError.For("field1", "Error 1")
            .And("field2", "Error 2");
        var error2 = ValidationError.For("field3", "Error 3")
            .And("field1", "Another error for field1");
        return error1.Merge(error2);
    }

    [Benchmark]
    public bool ErrorEquality_SameCode()
    {
        var error1 = Error.NotFound("Resource 1 not found");
        var error2 = Error.NotFound("Resource 2 not found");
        return error1.Equals(error2);
    }

    [Benchmark]
    public bool ErrorEquality_DifferentCode()
    {
        return _validationError.Equals(_notFoundError);
    }

    [Benchmark]
    public int GetHashCode_ValidationError()
    {
        return _validationError.GetHashCode();
    }

    [Benchmark]
    public int GetHashCode_ComplexValidationError()
    {
        return _complexValidationError.GetHashCode();
    }

    [Benchmark]
    public string ToString_SimpleError()
    {
        return _validationError.ToString();
    }

    [Benchmark]
    public string ToString_ComplexValidationError()
    {
        return _complexValidationError.ToString();
    }

    [Benchmark]
    public ValidationError ValidationError_WithFieldErrors()
    {
        var fieldErrors = new[]
        {
            new FieldError("field1", new[] { "Error 1", "Error 2" }),
            new FieldError("field2", new[] { "Error 3" }),
            new FieldError("field3", new[] { "Error 4", "Error 5", "Error 6" })
        };
        return new ValidationError(fieldErrors, "validation.error", "Multiple validation errors");
    }

    [Benchmark]
    public Error CreateErrorFromException()
    {
        var exception = new InvalidOperationException("Test exception");
        return Error.Unexpected(exception.Message);
    }

    [Benchmark]
    public bool IsErrorType_Validation()
    {
        return _validationError is ValidationError;
    }

    [Benchmark]
    public bool IsErrorType_NotFound()
    {
        return _notFoundError is NotFoundError;
    }

    [Benchmark]
    public Error CreateErrorChain_WithDetails()
    {
        return Error.Validation("Field validation failed", "email", "User registration failed", "user-123");
    }
}