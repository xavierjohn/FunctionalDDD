namespace Trellis.EntityFrameworkCore;

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
/// Value converter for Trellis <see cref="IScalarValue{TSelf, TPrimitive}"/> types.
/// <para>
/// Converts to database using the <c>Value</c> property and from database
/// using the static <c>Create</c> factory method. Expression trees are
/// preserved so EF Core can inspect them for query translation.
/// </para>
/// </summary>
/// <typeparam name="TModel">The Trellis value object type (e.g., <c>EmailAddress</c>).</typeparam>
/// <typeparam name="TProvider">The database provider type (e.g., <c>string</c>).</typeparam>
public class TrellisScalarConverter<TModel, TProvider> : ValueConverter<TModel, TProvider>
    where TModel : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TrellisScalarConverter{TModel, TProvider}"/> class.
    /// </summary>
    public TrellisScalarConverter() : base(
        BuildToProviderExpression(),
        BuildToModelExpression())
    {
    }

    private static Expression<Func<TModel, TProvider>> BuildToProviderExpression()
    {
        var param = Expression.Parameter(typeof(TModel), "v");
        var valueProp = typeof(TModel).GetProperty("Value")
            ?? throw new InvalidOperationException(
                $"{typeof(TModel).Name} must have a Value property.");
        var body = Expression.Property(param, valueProp);
        return Expression.Lambda<Func<TModel, TProvider>>(body, param);
    }

    private static Expression<Func<TProvider, TModel>> BuildToModelExpression()
    {
        var param = Expression.Parameter(typeof(TProvider), "v");
        var createMethod = typeof(TModel).GetMethod(
                "Create",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy,
                [typeof(TProvider)])
            ?? throw new InvalidOperationException(
                $"{typeof(TModel).Name} must have a static Create({typeof(TProvider).Name}) method.");
        var body = Expression.Call(createMethod, param);
        return Expression.Lambda<Func<TProvider, TModel>>(body, param);
    }
}