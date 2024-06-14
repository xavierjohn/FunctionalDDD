namespace SourceGenerator
{
    using System.Text.RegularExpressions;

    internal static class StringExtenstions
    {
        /// <summary>
        /// Splits pascal case, so "FooBar" would become "Foo Bar".
        /// </summary>
        internal static string SplitPascalCase(this string input)
            => Regex.Replace(input, @"(?<=[a-z])(?=[A-Z])", " ").Trim();

        public static string ToCamelCase(this string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
                return char.ToLowerInvariant(str[0]) + str.Substring(1);

            return str.ToLowerInvariant();
        }
    }
}
