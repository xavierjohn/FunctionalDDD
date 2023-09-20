namespace FunctionalDDD.Domain;
/// <summary>
/// Create a strongly typed Guid value object that cannot have default Guid value.
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <seealso cref="ScalarValueObject{TValue}"/>
/// <example>
/// This example shows how to create a strongly named Value Object MenuId that cannot have default Guid value.
/// <code>
/// partial class MenuId : RequiredGuid&lt;MenuId&gt;
/// </code>
/// **Note** The partial keyword is required to allow the code generator to add the generated methods.
/// </example>
public abstract class RequiredGuid<TValue> : ScalarValueObject<Guid>
    where TValue : RequiredGuid<TValue>
{

    protected RequiredGuid(Guid value) : base(value)
    {
    }
}
