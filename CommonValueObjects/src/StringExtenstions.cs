namespace FunctionalDDD;

using System.Text;

internal static class StringExtenstions
{
    /// <summary>
    /// Splits pascal case, so "FooBar" would become "Foo Bar".
    /// </summary>
    /// <remarks>
    /// Pascal case strings with periods delimiting the upper case letters,
    /// such as "Address.Line1", will have the periods removed.
    /// </remarks>
    internal static string SplitPascalCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var retVal = new StringBuilder(input.Length + 5);

        for (int i = 0; i < input.Length; ++i)
        {
            var currentChar = input[i];
            if (char.IsUpper(currentChar))
            {
                if ((i > 1 && !char.IsUpper(input[i - 1]))
                    || (i + 1 < input.Length && !char.IsUpper(input[i + 1])))
                    retVal.Append(' ');
            }

            if (!char.Equals('.', currentChar)
               || i + 1 == input.Length
               || !char.IsUpper(input[i + 1]))
            {
                retVal.Append(currentChar);
            }
        }

        return retVal.ToString().Trim();
    }

    public static string ToCamelCase(this string str)
    {
        if (!string.IsNullOrEmpty(str) && str.Length > 1)
            return char.ToLowerInvariant(str[0]) + str[1..];
        return str.ToLowerInvariant();
    }
}
