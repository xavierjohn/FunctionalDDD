namespace FunctionalDdd;

/// <summary>
/// Interface for scalar value objects that can be created with validation.
/// Enables automatic ASP.NET Core model binding and JSON serialization.
/// </summary>
/// <typeparam name="TSelf">The value object type itself (CRTP pattern)</typeparam>
/// <typeparam name="TPrimitive">The underlying primitive type (must be IComparable)</typeparam>
/// <remarks>
/// <para>
/// This interface uses the Curiously Recurring Template Pattern (CRTP) to enable
/// static abstract methods on the value object type. This allows model binders and
/// JSON converters to call <see cref="TryCreate"/> without reflection.
/// </para>
/// <para>
/// When a type implements this interface, it can be automatically validated during:
/// <list type="bullet">
/// <item>ASP.NET Core model binding (from route, query, form, or header values)</item>
/// <item>JSON deserialization (from request body)</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Implementing in a custom value object:
/// <code><![CDATA[
/// public class EmailAddress : ScalarValueObject<EmailAddress, string>, IScalarValueObject<EmailAddress, string>
/// {
///     private EmailAddress(string value) : base(value) { }
///     
///     public static Result<EmailAddress> TryCreate(string value) =>
///         value.ToResult(Error.Validation("Email is required"))
///             .Ensure(e => e.Contains("@"), Error.Validation("Invalid email"))
///             .Map(e => new EmailAddress(e));
/// }
/// ]]></code>
/// </example>
public interface IScalarValueObject<TSelf, TPrimitive>
    where TSelf : IScalarValueObject<TSelf, TPrimitive>
    where TPrimitive : IComparable
{
    /// <summary>
    /// Attempts to create a validated value object from a primitive value.
    /// </summary>
    /// <param name="value">The raw primitive value</param>
    /// <returns>Success with the value object, or Failure with validation errors</returns>
    /// <remarks>
    /// <para>
    /// This method is called by model binders and JSON converters to create value objects
    /// with validation. The validation errors are collected and returned through the
    /// standard ASP.NET Core validation infrastructure.
    /// </para>
    /// <para>
    /// Note: This overload does not accept a field name parameter. When automatic
    /// model binding is used, the field name is derived from the model property name.
    /// </para>
    /// </remarks>
    static abstract Result<TSelf> TryCreate(TPrimitive value);

    /// <summary>
    /// Gets the underlying primitive value for serialization.
    /// </summary>
    /// <value>The primitive value wrapped by this value object.</value>
    TPrimitive Value { get; }
}
