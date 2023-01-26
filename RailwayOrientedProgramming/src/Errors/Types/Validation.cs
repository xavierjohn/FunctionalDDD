namespace FunctionalDDD;

public sealed class Validation : Error
{
    public Validation(string description, string fieldName, string code) : base(description, code)
    {
        FieldName = fieldName;
    }

    public string FieldName { get; }
}
