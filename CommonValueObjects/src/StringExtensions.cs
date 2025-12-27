namespace FunctionalDdd;

/// <summary>
/// Provides internal string manipulation extension methods for the CommonValueObjects library.
/// </summary>
internal static class StringExtensions
{
    /// <summary>
    /// Converts the first character of a string to lowercase while preserving the rest of the string.
    /// Used primarily for converting C# PascalCase property names to camelCase for JSON serialization and validation error field names.
    /// </summary>
    /// <param name="str">The string to convert to camelCase.</param>
    /// <returns>
    /// A new string with the first character in lowercase and the remaining characters unchanged.
    /// If the string is null, empty, or has only one character, returns the lowercase version of the entire string.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is used internally to ensure consistent field naming in validation errors and JSON responses.
    /// For example, converting "Email" to "email" or "FirstName" to "firstName".
    /// </para>
    /// <para>
    /// Examples:
    /// <list type="bullet">
    /// <item>"Email" → "email"</item>
    /// <item>"FirstName" → "firstName"</item>
    /// <item>"A" → "a"</item>
    /// <item>"" → ""</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static string ToCamelCase(this string str)
    {
        if (!string.IsNullOrEmpty(str) && str.Length > 1)
            return char.ToLowerInvariant(str[0]) + str[1..];
        return str.ToLowerInvariant();
    }
}
