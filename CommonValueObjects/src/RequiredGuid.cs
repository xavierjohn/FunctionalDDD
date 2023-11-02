namespace FunctionalDdd;
/// <summary>
/// Create a strongly typed Guid value object that cannot have default Guid value.
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <seealso cref="ScalarValueObject{TValue}"/>
/// <example>
/// This example shows how to create a strongly named Value Object MenuId that cannot have default Guid value.
/// <code>
/// partial class MenuId : RequiredGuid
/// </code>
/// **Note** The partial keyword is required to allow the code generator to add the generated methods.
/// </example>
public abstract class RequiredGuid : ScalarValueObject<Guid>
{

    protected RequiredGuid(Guid value) : base(value)
    {
    }
}
