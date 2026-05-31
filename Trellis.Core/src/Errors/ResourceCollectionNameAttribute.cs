namespace Trellis;

using System;

/// <summary>
/// Overrides the naive plural form used when the framework synthesises a canonical resource URI
/// for <c>ProblemDetails.Instance</c> from a <see cref="ResourceRef"/>. The default form is
/// <c>ResourceRef.Type.ToLowerInvariant() + "s"</c>; this attribute lets the consumer supply
/// an irregular plural (<c>Person</c> → <c>people</c>) or a domain-specific collection segment
/// (<c>UserAccount</c> → <c>accounts</c>).
/// </summary>
/// <remarks>
/// <para>
/// The attribute is read by <c>Trellis.Asp.AddResourceCollectionNames(Assembly)</c> at startup
/// and cached for the lifetime of the host. Apply it to the aggregate or resource type whose
/// CLR name is reported by <see cref="ResourceRef.For{TResource}(object?)"/>.
/// </para>
/// <para>
/// <see cref="Name"/> must be a single URL-safe path segment: no <c>/</c>, <c>?</c>, <c>#</c>,
/// or whitespace. Validation happens in the attribute constructor so a misconfigured override
/// is surfaced at type-load (not at the first failing request).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [ResourceCollectionName("people")]
/// public sealed class Person { /* ... */ }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class ResourceCollectionNameAttribute : Attribute
{
    /// <summary>The collection-name path segment to use in synthesised resource URIs.</summary>
    public string Name { get; }

    /// <summary>
    /// Initialises a new <see cref="ResourceCollectionNameAttribute"/>.
    /// </summary>
    /// <param name="name">The collection-name path segment.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="name"/> is null, whitespace, or contains a character
    /// that would split a path segment (<c>/</c>, <c>?</c>, <c>#</c>, or whitespace).
    /// </exception>
    public ResourceCollectionNameAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (!IsSafePathSegment(name))
        {
            throw new ArgumentException(
                $"Collection name '{name}' must be a single URL-safe path segment (no '/', '?', '#', or whitespace).",
                nameof(name));
        }

        Name = name;
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="name"/> is a single URL-safe path segment
    /// (no <c>/</c>, <c>?</c>, <c>#</c>, or whitespace). Exposed so that other framework
    /// components consuming a <see cref="ResourceCollectionNameAttribute"/>-equivalent
    /// override can apply the same validation rule.
    /// </summary>
    /// <param name="name">The candidate path segment.</param>
    /// <returns><c>true</c> when the value is safe to use unencoded as a single path segment.</returns>
    public static bool IsSafePathSegment(string name)
    {
        foreach (var c in name)
        {
            if (c == '/' || c == '?' || c == '#' || char.IsWhiteSpace(c))
                return false;
        }

        return true;
    }
}
