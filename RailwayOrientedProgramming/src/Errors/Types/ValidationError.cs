namespace FunctionalDDD.Results.Errors;

public sealed class ValidationError : Error
{
    public record ModelError(string Message, string FieldName);

    public ValidationError(string message, string fieldName, string code) : base(message, code)
    {
        FieldName = fieldName;
        Errors = new List<ModelError> { new ModelError(message, fieldName) };
    }
    public ValidationError(List<ModelError> modelErrors, string code) : base("Validation error", code)
    {
        if (modelErrors.Count < 1)
            throw new ArgumentException("At least one error is required", nameof(modelErrors));

        FieldName = modelErrors[0].FieldName;
        Errors = modelErrors.ToList();
    }

    public string FieldName { get; }

    public List<ModelError> Errors { get; }
}
