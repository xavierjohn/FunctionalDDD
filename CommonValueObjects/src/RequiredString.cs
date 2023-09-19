namespace FunctionalDDD.Domain.ValueObjects;

using FunctionalDDD.Domain;

/// <summary>
/// Represents a required string value object.
/// 
/// <example>
/// This example shows how to create a strongly named Value Object FirstName that checks for null or empty string.
/// <code>
/// partial class FirstName : RequiredString&lt;FirstName&gt;
/// </code>
/// </example>
/// </summary>
/// <typeparam name="T"></typeparam>
/// <seealso cref="ValueObject"/>
public abstract class RequiredString<T> : SimpleValueObject<string>
    where T : RequiredString<T>
{
    protected RequiredString(string value) : base(value)
    {
    }
}
