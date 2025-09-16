namespace FunctionalDdd;

public sealed class ValidationError : Error
{
    public record FieldDetails(string FieldName, string[] Details);

    public ValidationError(string fieldDetail, string fieldName, string code, string? detail = null, string? instance = null)
        : base(detail ?? fieldDetail, code, instance)
        => Errors = [new FieldDetails(fieldName, [fieldDetail])];

    public ValidationError(FieldDetails[] fieldDetails, string code, string detail = "", string? instance = null)
        : base(detail, code, instance)
        => Errors = [.. fieldDetails];

    public IList<FieldDetails> Errors { get; set; }

    public override string ToString()
        => base.ToString() + "\r\n" + string.Join("\r\n", Errors.Select(e => $"{e.FieldName}: {string.Join(", ", e.Details)}"));
}
