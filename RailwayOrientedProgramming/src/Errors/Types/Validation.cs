namespace FunctionalDDD;

public sealed class Validation : Err
{
    public Validation(string description, string fieldName, string code) : base(description, code)
    {
        FieldName = fieldName;
    }

    public string FieldName { get; }
}
