using System.Linq.Expressions;
using System.Reflection;
using FunctionalDDD;

namespace FunctionalDDD.CommonValueObjects;
public abstract class RequiredString<T> : SimpleValueObject<string>
    where T : RequiredString<T>
{
    private static readonly Lazy<Func<string, T>> CreateInstance = new Lazy<Func<string, T>>(CreateInstanceFunc);
    private static readonly Error cannotBeEmptyError= Error.Validation($"{typeof(T).Name}", $"{typeof(T).Name} cannot be empty");
    
    protected RequiredString(string value) : base(value)
    {
    }


    public static Result<T> Create(Maybe<string> requiredStringOrNothing)
    {
        return requiredStringOrNothing
            .EnsureNotNullOrWhiteSpace(cannotBeEmptyError)
            .Map(name => CreateInstance.Value(name));
    }

    private static Func<string, T> CreateInstanceFunc()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var ctor = typeof(T).GetTypeInfo().GetConstructors(flags).Single(
            ctors =>
            {
                var parameters = ctors.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
            });
        var value = Expression.Parameter(typeof(string), "value");
        var body = Expression.New(ctor, value);
        var lambda = Expression.Lambda<Func<string, T>>(body, value);

        return lambda.Compile();
    }
}
