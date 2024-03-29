namespace FunctionalDdd;

public sealed class ValidationError : Error
{
    public record ModelError(string Message, string FieldName);

    public ValidationError(string message, string fieldName, string code, string? instance = null) : base(message, code, instance)
        => Errors = [new ModelError(message, fieldName)];

    public ValidationError(List<ModelError> modelErrors, string? message, string code, string? instance) : base(message ?? "", code, instance)
    {
        if (modelErrors.Count < 1)
            throw new ArgumentException("At least one error is required", nameof(modelErrors));
        Errors = modelErrors.ToList();
    }

    public List<ModelError> Errors { get; }
}
