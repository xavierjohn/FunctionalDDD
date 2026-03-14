namespace Trellis.EntityFrameworkCore;

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

internal readonly record struct MaybePropertyDescriptor(
    PropertyInfo Property,
    string PropertyName,
    string BackingFieldName,
    Type InnerType,
    Type StoreType);

internal static class MaybePropertyResolver
{
    private static readonly Type s_maybeOpenGenericType = typeof(Maybe<>);

    internal static IEnumerable<MaybePropertyDescriptor> GetMaybeProperties(Type clrType) =>
        clrType
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(IsMaybeProperty)
            .Select(CreateDescriptor);

    internal static bool IsMaybeProperty(PropertyInfo property) =>
        IsMaybeType(property.PropertyType);

    internal static bool IsMaybeType(Type type) =>
        type.IsGenericType
        && type.GetGenericTypeDefinition() == s_maybeOpenGenericType;

    internal static MaybePropertyDescriptor Resolve(LambdaExpression propertySelector)
    {
        if (TryGetProperty(propertySelector.Body, out var property) && IsMaybeProperty(property))
            return CreateDescriptor(property);

        throw new ArgumentException(
            "Expression must be a simple Maybe<T> property access (e.g., e => e.Phone).",
            nameof(propertySelector));
    }

    internal static IReadOnlyList<string> ResolveMappedPropertyNames(LambdaExpression propertySelector)
    {
        var propertyNames = new List<string>();
        CollectMappedPropertyNames(propertySelector.Body, propertyNames);

        if (propertyNames.Count == 0)
        {
            throw new ArgumentException(
                "Expression must be a property access or anonymous object of property accesses (e.g., e => e.Phone or e => new { e.Status, e.SubmittedAt }).",
                nameof(propertySelector));
        }

        return propertyNames;
    }

    internal static LambdaExpression BuildBackingFieldLambda<TEntity, TInner>(
        Expression<Func<TEntity, Maybe<TInner>>> propertySelector)
        where TEntity : class
        where TInner : notnull
    {
        var descriptor = Resolve(propertySelector);
        var body = BuildEfPropertyAccess(propertySelector.Parameters[0], descriptor);
        var delegateType = typeof(Func<,>).MakeGenericType(typeof(TEntity), descriptor.StoreType);

        return Expression.Lambda(delegateType, body, propertySelector.Parameters);
    }

    internal static Expression BuildEfPropertyAccess(
        ParameterExpression parameter,
        MaybePropertyDescriptor descriptor)
    {
        var genericMethod = s_efPropertyMethodInfo.MakeGenericMethod(descriptor.StoreType);

        return Expression.Call(
            genericMethod,
            Expression.Convert(parameter, typeof(object)),
            Expression.Constant(descriptor.BackingFieldName));
    }

    internal static string ResolveMappedPropertyName(PropertyInfo property) =>
        IsMaybeProperty(property)
            ? CreateDescriptor(property).BackingFieldName
            : property.Name;

    private static readonly MethodInfo s_efPropertyMethodInfo =
        typeof(EF).GetMethod(nameof(EF.Property))!;

    private static MaybePropertyDescriptor CreateDescriptor(PropertyInfo property)
    {
        if (!IsMaybeProperty(property))
            throw new ArgumentException($"Property '{property.Name}' is not of type Maybe<T>.", nameof(property));

        var innerType = property.PropertyType.GetGenericArguments()[0];
        var backingFieldName = MaybeFieldNaming.ToBackingFieldName(property.Name);
        var storeType = innerType.IsValueType
            ? typeof(Nullable<>).MakeGenericType(innerType)
            : innerType;

        return new MaybePropertyDescriptor(property, property.Name, backingFieldName, innerType, storeType);
    }

    private static void CollectMappedPropertyNames(Expression expression, ICollection<string> propertyNames)
    {
        expression = UnwrapConvert(expression);

        switch (expression)
        {
            case MemberExpression { Member: PropertyInfo property }:
                propertyNames.Add(ResolveMappedPropertyName(property));
                return;

            case NewExpression newExpression:
                foreach (var argument in newExpression.Arguments)
                    CollectMappedPropertyNames(argument, propertyNames);

                return;

            default:
                throw new ArgumentException(
                    "Expression must be a property access or anonymous object of property accesses.",
                    nameof(expression));
        }
    }

    private static bool TryGetProperty(Expression expression, out PropertyInfo property)
    {
        expression = UnwrapConvert(expression);

        if (expression is MemberExpression { Member: PropertyInfo propertyInfo })
        {
            property = propertyInfo;
            return true;
        }

        property = null!;
        return false;
    }

    private static Expression UnwrapConvert(Expression expression)
    {
        while (expression is UnaryExpression
               {
                   NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked,
                   Operand: { } operand
               })
        {
            expression = operand;
        }

        return expression;
    }
}