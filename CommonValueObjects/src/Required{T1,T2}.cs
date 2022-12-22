namespace FunctionalDDD;

using System.Linq.Expressions;
using System.Reflection;

public abstract class Required<T1, T2> : SimpleValueObject<T1>
    where T1 : IComparable
{
    protected static readonly Lazy<Func<T1, T2>> CreateInstance = new Lazy<Func<T1, T2>>(CreateInstanceFunc);
    protected static readonly Error CannotBeEmptyError = Error.Validation($"{typeof(T2).Name.ToCamelCase()}", $"{typeof(T2).Name.SplitPascalCase()} cannot be empty");

    protected Required(T1 value) : base(value)
    {
    }

    private static Func<T1, T2> CreateInstanceFunc()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var ctor = typeof(T2).GetTypeInfo().GetConstructors(flags).Single(
            ctors =>
            {
                var parameters = ctors.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(T1);
            });
        var value = Expression.Parameter(typeof(T1), "value");
        var body = Expression.New(ctor, value);
        var lambda = Expression.Lambda<Func<T1, T2>>(body, value);

        return lambda.Compile();
    }
}
