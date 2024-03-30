namespace FunctionalDdd;

public sealed class ValidationError : Error
{
    public record FieldDetails(string Name, string[] Details);

    public ValidationError(string fieldDetail, string fieldName, string code, string detail = "", string? instance = null) : base(detail, code, instance)
        => Errors = [new FieldDetails(fieldName, [fieldDetail])];

    public ValidationError(List<FieldDetails> modelErrors, string code, string detail = "", string? instance = null) : base(detail, code, instance)
        => Errors = [.. modelErrors];

    public List<FieldDetails> Errors { get; }
}
