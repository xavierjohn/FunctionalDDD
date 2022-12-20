﻿namespace FunctionalDDD.CommonValueObjects;
using System.Linq.Expressions;
using System.Reflection;

public abstract class RequiredGuid<T> : SimpleValueObject<Guid>
    where T : RequiredGuid<T>
{
    private static readonly Lazy<Func<Guid, T>> CreateInstance = new Lazy<Func<Guid, T>>(CreateInstanceFunc);
    private static readonly Error cannotBeEmptyError = Error.Validation($"{typeof(T).Name.ToCamelCase()}", $"{typeof(T).Name.SplitPascalCase()} cannot be empty");

    protected RequiredGuid(Guid value) : base(value)
    {
    }


    public static Result<T> Create(Maybe<Guid> requiredStringOrNothing)
    {
        return requiredStringOrNothing
            .ToResult(cannotBeEmptyError)
            .Ensure(x => x != Guid.Empty, cannotBeEmptyError)
            .Map(guid => CreateInstance.Value(guid));
    }

    public static T CreateUnique() => CreateInstance.Value(Guid.NewGuid());

    private static Func<Guid, T> CreateInstanceFunc()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var ctor = typeof(T).GetTypeInfo().GetConstructors(flags).Single(
            ctors =>
            {
                var parameters = ctors.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(Guid);
            });
        var value = Expression.Parameter(typeof(Guid), "value");
        var body = Expression.New(ctor, value);
        var lambda = Expression.Lambda<Func<Guid, T>>(body, value);

        return lambda.Compile();
    }
}
