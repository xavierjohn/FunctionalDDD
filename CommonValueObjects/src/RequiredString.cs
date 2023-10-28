namespace FunctionalDDD.Domain;
/// <summary>
/// Create a strongly typed string value object that checks for null or empty string.
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <seealso cref="ScalarValueObject{TValue}"/>
/// <example>
/// This example shows how to create a strongly named Value Object FirstName that checks for null or empty string.
/// <code>
/// partial class FirstName : RequiredString
/// </code>
/// **Note** The partial keyword is required to allow the code generator to add the generated methods.
/// </example>

public abstract class RequiredString : ScalarValueObject<string>
{
    protected RequiredString(string value) : base(value)
    {
    }
}
