namespace FunctionalDDD;

internal static class StringExtensions
{
    public static string ToCamelCase(this string str)
    {
        if (!string.IsNullOrEmpty(str) && str.Length > 1)
            return char.ToLowerInvariant(str[0]) + str[1..];
        return str.ToLowerInvariant();
    }
}
