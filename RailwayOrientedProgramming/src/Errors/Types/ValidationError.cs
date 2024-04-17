namespace FunctionalDdd;

public sealed class ValidationError : Error
{
    public record FieldDetails(string Name, string[] Details);

    public ValidationError(string fieldDetail, string fieldName, string code, string detail = "", string? instance = null)
        : base(detail, code, instance)
        => Errors = [new FieldDetails(fieldName, [fieldDetail])];

    public ValidationError(FieldDetails[] fieldDetails, string code, string detail = "", string? instance = null)
        : base(detail, code, instance)
        => Errors = [.. fieldDetails];

    public IList<FieldDetails> Errors { get; set; }
}
