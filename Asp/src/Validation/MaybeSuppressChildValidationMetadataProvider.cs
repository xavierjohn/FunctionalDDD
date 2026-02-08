namespace FunctionalDdd.Asp.Validation;

using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

/// <summary>
/// Prevents MVC's <see cref="ValidationVisitor"/> from recursing into <see cref="Maybe{T}"/> properties.
/// Without this, the visitor accesses <see cref="Maybe{T}.Value"/> via reflection, which throws
/// <see cref="InvalidOperationException"/> when <see cref="Maybe{T}.HasNoValue"/> is true.
/// </summary>
/// <remarks>
/// The built-in <c>SuppressChildValidationMetadataProvider(typeof(Maybe&lt;&gt;))</c> doesn't work
/// because it uses <c>Type.IsAssignableFrom</c>, which returns <c>false</c> for open generic types
/// compared against closed generic types like <c>Maybe&lt;FirstName&gt;</c>.
/// </remarks>
internal sealed class MaybeSuppressChildValidationMetadataProvider : IValidationMetadataProvider
{
    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        if (context.Key.ModelType.IsGenericType &&
            context.Key.ModelType.GetGenericTypeDefinition() == typeof(Maybe<>))
        {
            context.ValidationMetadata.ValidateChildren = false;
        }
    }
}
