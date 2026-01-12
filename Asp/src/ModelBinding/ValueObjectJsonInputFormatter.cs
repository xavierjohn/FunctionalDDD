namespace FunctionalDdd.Asp.ModelBinding;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

/// <summary>
/// Custom input formatter that handles JSON deserialization for DTOs containing value objects implementing <see cref="ITryCreatable{T}"/>.
/// This formatter intercepts JSON deserialization and validates value objects before creating the DTO instance.
/// </summary>
/// <remarks>
/// <para>
/// This formatter solves the limitation where JSON deserialization happens before model binding.
/// It deserializes JSON properties as strings first, validates them using ITryCreatable.TryCreate,
/// and adds validation errors to ModelState before constructing the DTO.
/// </para>
/// <para>
/// <strong>Registration (Optional):
/// </strong>
/// <code>
/// builder.Services.AddControllers(options =>
/// {
///     options.AddValueObjectJsonInputFormatter(); // Enable JSON body validation
///     options.AddValueObjectModelBinding();       // Enable route/query parameter validation
/// });
/// </code>
/// </para>
/// <para>
/// <strong>Usage:</strong>
/// <code>
/// public record CreateUserRequest(
///     FirstName FirstName,     // Validated from JSON
///     LastName LastName,       // Validated from JSON
///     EmailAddress Email       // Validated from JSON
/// );
/// 
/// [HttpPost]
/// public ActionResult&lt;User&gt; Create([FromBody] CreateUserRequest request) =>
///     User.TryCreate(request.FirstName, request.LastName, request.Email)
///         .ToActionResult(this);
/// 
/// // Invalid JSON returns 400 Bad Request with field-level errors
/// </code>
/// </para>
/// <para>
/// <strong>Limitations:</strong>
/// <list type="bullet">
/// <item>Requires DTOs to use record-style primary constructors</item>
/// <item>Uses reflection - performance overhead compared to standard JSON deserialization</item>
/// <item>Only handles top-level properties (nested value objects work if the nested type uses this formatter)</item>
/// </list>
/// </para>
/// </remarks>
public class ValueObjectJsonInputFormatter : TextInputFormatter
{
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueObjectJsonInputFormatter"/> class.
    /// </summary>
    /// <param name="jsonOptions">Optional JSON serializer options. If null, uses default options.</param>
    public ValueObjectJsonInputFormatter(JsonSerializerOptions? jsonOptions = null)
    {
        _jsonOptions = jsonOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);
        
        SupportedMediaTypes.Add("application/json");
        SupportedMediaTypes.Add("text/json");
        SupportedMediaTypes.Add("application/*+json");
        
        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }

    /// <summary>
    /// Determines if this formatter can read the given type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type has properties implementing ITryCreatable; otherwise false.</returns>
    protected override bool CanReadType(Type type) => HasTryCreatableProperties(type);

    /// <summary>
    /// Reads and deserializes the JSON request body, validating value objects.
    /// </summary>
    /// <param name="context">The input formatter context.</param>
    /// <param name="encoding">The encoding to use when reading.</param>
    /// <returns>The deserialized and validated object, or failure if validation errors occurred.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "DTO types are known at compile time")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Input formatting occurs at startup, not in AOT paths")]
    public override async Task<InputFormatterResult> ReadRequestBodyAsync(
        InputFormatterContext context,
        Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(context);

        var httpContext = context.HttpContext;
        var modelType = context.ModelType;

        try
        {
            using var reader = new StreamReader(httpContext.Request.Body, encoding);
            var json = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(json))
            {
                return InputFormatterResult.Failure();
            }

            var jsonDocument = JsonDocument.Parse(json);
            var result = CreateObjectWithValidation(
                modelType,
                jsonDocument.RootElement,
                context.ModelState,
                string.Empty);

            if (result.IsSuccess)
            {
                return InputFormatterResult.Success(result.Value);
            }

            // Validation errors already added to ModelState
            return InputFormatterResult.Failure();
        }
        catch (JsonException ex)
        {
            context.ModelState.AddModelError(string.Empty, $"Invalid JSON: {ex.Message}");
            return InputFormatterResult.Failure();
        }
    }

    /// <summary>
    /// Recursively creates an object from JSON, validating value objects using ITryCreatable.TryCreate.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "DTO types are known at compile time")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Input formatting occurs at startup")]
    private Result<object> CreateObjectWithValidation(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type modelType,
        JsonElement jsonElement,
        ModelStateDictionary modelState,
        string propertyPath)
    {
        // Find primary constructor (record-style)
        var constructor = modelType.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        if (constructor == null)
        {
            modelState.AddModelError(propertyPath, $"No suitable constructor found for {modelType.Name}");
            return Error.Validation($"No suitable constructor found for {modelType.Name}");
        }

        var parameters = constructor.GetParameters();
        var args = new List<object?>();
        var hasErrors = false;

        foreach (var param in parameters)
        {
            var paramType = param.ParameterType;
            var paramName = param.Name!;
            var fullPath = string.IsNullOrEmpty(propertyPath)
                ? paramName
                : $"{propertyPath}.{paramName}";

            // Try to get JSON property (case-insensitive)
            if (!TryGetJsonProperty(jsonElement, paramName, out var jsonValue))
            {
                // Check if parameter has default value
                if (param.HasDefaultValue)
                {
                    args.Add(param.DefaultValue);
                    continue;
                }

                // Check if it's a nullable type
                if (Nullable.GetUnderlyingType(paramType) != null || !paramType.IsValueType)
                {
                    args.Add(null);
                    continue;
                }

                modelState.AddModelError(fullPath, $"{paramName} is required");
                hasErrors = true;
                args.Add(GetDefaultValue(paramType));
                continue;
            }

            // Handle null JSON values
            if (jsonValue.ValueKind == JsonValueKind.Null)
            {
                if (Nullable.GetUnderlyingType(paramType) != null || !paramType.IsValueType)
                {
                    args.Add(null);
                    continue;
                }

                modelState.AddModelError(fullPath, $"{paramName} cannot be null");
                hasErrors = true;
                args.Add(GetDefaultValue(paramType));
                continue;
            }

            // Check if it's ITryCreatable
            var tryCreatableInterface = GetTryCreatableInterface(paramType);

            if (tryCreatableInterface != null)
            {
                // It's a value object - validate it!
                var stringValue = jsonValue.ValueKind == JsonValueKind.String
                    ? jsonValue.GetString()
                    : jsonValue.GetRawText();

                // Pass the parameter name as the field name for error messages
                var valueResult = CallTryCreate(paramType, stringValue, paramName);

                if (valueResult.IsFailure)
                {
                    // Add validation errors to ModelState
                    AddErrorsToModelState(modelState, fullPath, valueResult.Error);
                    hasErrors = true;
                    args.Add(GetDefaultValue(paramType));
                }
                else
                {
                    args.Add(valueResult.Value);
                }
            }
            else if (HasTryCreatableProperties(paramType))
            {
                // Nested complex type with value objects - recurse
                var nestedResult = CreateObjectWithValidation(paramType, jsonValue, modelState, fullPath);

                if (nestedResult.IsFailure)
                {
                    hasErrors = true;
                    args.Add(GetDefaultValue(paramType));
                }
                else
                {
                    args.Add(nestedResult.Value);
                }
            }
            else
            {
                // Regular type - deserialize normally using System.Text.Json
                try
                {
                    var value = JsonSerializer.Deserialize(jsonValue, paramType, _jsonOptions);
                    args.Add(value);
                }
                catch (JsonException ex)
                {
                    modelState.AddModelError(fullPath, $"Invalid value: {ex.Message}");
                    hasErrors = true;
                    args.Add(GetDefaultValue(paramType));
                }
            }
        }

        if (hasErrors)
        {
            return Error.Validation("Validation failed");
        }

        // Create the object using the constructor
        try
        {
            var instance = constructor.Invoke(args.ToArray());
            return Result.Success(instance!);
        }
        catch (Exception ex)
        {
            modelState.AddModelError(propertyPath, $"Failed to create instance: {ex.Message}");
            return Error.Validation($"Failed to create instance: {ex.Message}");
        }
    }

    /// <summary>
    /// Tries to get a JSON property by name (case-insensitive).
    /// </summary>
    private static bool TryGetJsonProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        // Try exact match first
        if (element.TryGetProperty(propertyName, out value))
            return true;

        // Try case-insensitive
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Gets the ITryCreatable interface from a type if it implements it.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "DTO types are known at compile time")]
    private static Type? GetTryCreatableInterface([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type) =>
        type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType &&
                                 i.GetGenericTypeDefinition() == typeof(ITryCreatable<>));

    /// <summary>
    /// Checks if a type has any properties implementing ITryCreatable.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "DTO types are known at compile time")]
    private static bool HasTryCreatableProperties(Type type)
    {
        try
        {
            var constructors = type.GetConstructors();
            if (constructors.Length == 0)
                return false;

            var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
            if (constructor == null)
                return false;

            return constructor.GetParameters()
                .Any(p => GetTryCreatableInterface(p.ParameterType) != null ||
                         HasTryCreatableProperties(p.ParameterType));
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Calls ITryCreatable&lt;T&gt;.TryCreate(string?, string?) using reflection.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "TryCreate is guaranteed by ITryCreatable interface")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Input formatting occurs at startup")]
    private static Result<object> CallTryCreate(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type valueObjectType,
        string? value,
        string? fieldName = null)
    {
        try
        {
            // Get TryCreate method with optional fieldName parameter
            // Look for: TryCreate(string?, string?)
            var tryCreateMethod = valueObjectType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m =>
                {
                    if (m.Name != "TryCreate")
                        return false;

                    var parameters = m.GetParameters();
                    if (parameters.Length != 2)
                        return false;

                    return parameters[0].ParameterType == typeof(string) &&
                           parameters[1].ParameterType == typeof(string);
                });

            if (tryCreateMethod == null)
            {
                return Error.Validation($"TryCreate method not found on {valueObjectType.Name}");
            }

            // Call TryCreate with value and fieldName
            var result = tryCreateMethod.Invoke(null, [value, fieldName]);

            if (result == null)
            {
                return Error.Validation("TryCreate returned null");
            }

            // Extract Result<T> properties
            var resultType = result.GetType();
            var isFailureProp = resultType.GetProperty("IsFailure");
            var isFailure = (bool)isFailureProp!.GetValue(result)!;

            if (isFailure)
            {
                var errorProp = resultType.GetProperty("Error");
                var error = (Error)errorProp!.GetValue(result)!;
                return Result.Failure<object>(error);
            }

            var valueProp = resultType.GetProperty("Value");
            var valueObject = valueProp!.GetValue(result)!;
            return Result.Success(valueObject);
        }
        catch (Exception ex)
        {
            return Error.Validation($"Failed to call TryCreate: {ex.Message}");
        }
    }

    /// <summary>
    /// Adds validation errors to ModelState.
    /// </summary>
    private static void AddErrorsToModelState(
        ModelStateDictionary modelState,
        string propertyPath,
        Error error)
    {
        if (error is ValidationError validationError)
        {
            foreach (var fieldError in validationError.FieldErrors)
            {
                var fieldName = string.IsNullOrEmpty(fieldError.FieldName)
                    ? propertyPath
                    : fieldError.FieldName;

                foreach (var detail in fieldError.Details)
                {
                    modelState.AddModelError(fieldName, detail);
                }
            }
        }
        else
        {
            modelState.AddModelError(propertyPath, error.Detail);
        }
    }

    /// <summary>
    /// Gets the default value for a type.
    /// </summary>
    private static object? GetDefaultValue([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type) =>
        type.IsValueType ? Activator.CreateInstance(type) : null;
}
